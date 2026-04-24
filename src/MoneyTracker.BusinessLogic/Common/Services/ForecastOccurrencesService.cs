using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Common.Services
{
    internal class ForecastOccurrencesService
    {
        private readonly MoneyTrackerDbContext _dbContext;
        public ForecastOccurrencesService(MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteOccurencesAsync(Guid id, CancellationToken cancellationToken)
        {
            await _dbContext.ForecastOccurrences.Where(x => x.ForecastDefinitionId == id && x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId)
                                                .ExecuteUpdateAsync(e => e.SetProperty(p => p.ForecastOccurrenceStatusId, ForecastOccurrenceStatus.CancelledId)
                                                , cancellationToken);
        }

        public async Task SynchronizeAsync(CancellationToken cancellationToken)
        {
            var (startDate, endDate) = GetSynchronizationWindow();
            var expectedOccurrences = new Dictionary<OccurrenceKey, OccurrenceSeed>();

            var forecastExpenses = await _dbContext.ForecastExpenses
                .AsNoTracking()
                .Include(x => x.ForecastRecurrenceRuleType)
                .Include(x => x.PaymentCategory)
                .Where(x => x.IsActive && x.RecurrenceStart <= endDate && (x.RecurrenceEnd == null || x.RecurrenceEnd >= startDate))
                .ToListAsync(cancellationToken);

            foreach (var forecast in forecastExpenses)
            {
                foreach (var recurrence in forecast.GetRecurrences(startDate, endDate))
                {
                    expectedOccurrences[new OccurrenceKey(forecast.Id, false, recurrence)] = new OccurrenceSeed(
                        forecast.Id,
                        false,
                        forecast.Description,
                        forecast.Amount,
                        recurrence,
                        forecast.PaymentCategoryId);
                }
            }

            var forecastIncomes = await _dbContext.ForecastIncomes
                .AsNoTracking()
                .Include(x => x.ForecastRecurrenceRuleType)
                .Where(x => x.IsActive && x.RecurrenceStart <= endDate && (x.RecurrenceEnd == null || x.RecurrenceEnd >= startDate))
                .ToListAsync(cancellationToken);

            foreach (var forecast in forecastIncomes)
            {
                foreach (var recurrence in forecast.GetRecurrences(startDate, endDate))
                {
                    expectedOccurrences[new OccurrenceKey(forecast.Id, true, recurrence)] = new OccurrenceSeed(
                        forecast.Id,
                        true,
                        forecast.Description,
                        forecast.Amount,
                        recurrence,
                        null);
                }
            }

            await SynchronizeAsync(expectedOccurrences, startDate, endDate, cancellationToken);
        }

        public async Task SynchronizeAsync(BaseForecast forecast, CancellationToken cancellationToken)
        {
            if (forecast is ForecastExpense expense)
                await SynchronizeAsync(expense, cancellationToken);
            else if (forecast is ForecastIncome income)
                await SynchronizeAsync(income, cancellationToken);
            else
                throw new InvalidCastException($"Unsupported forecast type: {forecast.GetType().Name}");
        }

        public async Task SynchronizeAsync(ForecastExpense forecastExpense, CancellationToken cancellationToken)
        {
            var (startDate, endDate) = GetSynchronizationWindow();
            var expectedOccurrences = new Dictionary<OccurrenceKey, OccurrenceSeed>();

            await RehydrateEntitAsync(forecastExpense, cancellationToken);
            var recurrences = forecastExpense.GetRecurrences(startDate, endDate);

            foreach (var recurrence in recurrences)
            {
                expectedOccurrences[new OccurrenceKey(forecastExpense.Id, false, recurrence)] = new OccurrenceSeed(
                    forecastExpense.Id,
                    false,
                    forecastExpense.Description,
                    forecastExpense.Amount,
                    recurrence,
                    forecastExpense.PaymentCategoryId);
            }

            await SynchronizeAsync(expectedOccurrences, forecastExpense.Id, startDate, endDate, cancellationToken);
        }

        public async Task SynchronizeAsync(ForecastIncome forecastIncome, CancellationToken cancellationToken)
        {
            var (startDate, endDate) = GetSynchronizationWindow();
            var expectedOccurrences = new Dictionary<OccurrenceKey, OccurrenceSeed>();

            await RehydrateEntitAsync(forecastIncome, cancellationToken);
            var recurrences = forecastIncome.GetRecurrences(startDate, endDate);

            foreach (var recurrence in recurrences)
            {
                expectedOccurrences[new OccurrenceKey(forecastIncome.Id, true, recurrence)] = new OccurrenceSeed(
                    forecastIncome.Id,
                    true,
                    forecastIncome.Description,
                    forecastIncome.Amount,
                    recurrence,
                    null);
            }


            await SynchronizeAsync(expectedOccurrences, forecastIncome.Id, startDate, endDate, cancellationToken);
        }

        public async Task SynchronizeAsync(Dictionary<OccurrenceKey, OccurrenceSeed> expectedOccurrences, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
        {
            var existingOccurrences = await _dbContext.ForecastOccurrences
                .Where(x => x.ExpectedDate >= startDate && x.ExpectedDate <= endDate)
                .ToListAsync(cancellationToken);

            await SynchronizeAsync(expectedOccurrences, existingOccurrences);
        }

        public async Task SynchronizeAsync(Dictionary<OccurrenceKey, OccurrenceSeed> expectedOccurrences, Guid forecastDefinitionId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
        {
            var existingOccurrences = await _dbContext.ForecastOccurrences
                .Where(x => x.ExpectedDate >= startDate && x.ExpectedDate <= endDate && x.ForecastDefinitionId == forecastDefinitionId)
                .ToListAsync(cancellationToken);

            await SynchronizeAsync(expectedOccurrences, existingOccurrences);
        }

        public async Task SynchronizeAsync(Dictionary<OccurrenceKey, OccurrenceSeed> expectedOccurrences, List<ForecastOccurrence> existingOccurrences)
        {
            var existingLookup = existingOccurrences.ToDictionary(
                x => new OccurrenceKey(x.ForecastDefinitionId, x.IsIncome, x.ExpectedDate),
                x => x);

            foreach (var existingOccurrence in existingOccurrences)
            {
                var key = new OccurrenceKey(existingOccurrence.ForecastDefinitionId, existingOccurrence.IsIncome, existingOccurrence.ExpectedDate);

                if (!expectedOccurrences.TryGetValue(key, out var seed))
                {
                    if (existingOccurrence.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId)
                    {
                        existingOccurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId;
                        existingOccurrence.ValidatedAt = null;
                    }

                    continue;
                }

                if (existingOccurrence.ForecastOccurrenceStatusId is var statusId && (statusId == ForecastOccurrenceStatus.PendingId || statusId == ForecastOccurrenceStatus.CancelledId))
                {
                    existingOccurrence.Description = seed.Description;
                    existingOccurrence.Amount = seed.Amount;
                    existingOccurrence.PaymentCategoryId = seed.PaymentCategoryId;
                    existingOccurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId;
                    existingOccurrence.ValidatedAt = null;

                }
            }

            foreach (var (key, seed) in expectedOccurrences)
            {
                if (existingLookup.ContainsKey(key))
                    continue;

                _dbContext.ForecastOccurrences.Add(new ForecastOccurrence
                {
                    Id = Guid.NewGuid(),
                    ForecastDefinitionId = seed.ForecastDefinitionId,
                    IsIncome = seed.IsIncome,
                    Description = seed.Description,
                    Amount = seed.Amount,
                    ExpectedDate = seed.ExpectedDate,
                    PaymentCategoryId = seed.PaymentCategoryId,
                    ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
                });
            }
        }

        private async Task RehydrateEntitAsync(BaseForecast baseForecast, CancellationToken cancellationToken)
        {
            if (baseForecast.ForecastRecurrenceRuleType is null)
            {
                var forecastRecurrenceRuleType = await _dbContext.ForecastRecurrenceRuleTypes.FirstOrDefaultAsync(f => f.Id == baseForecast.ForecastRecurrenceRuleTypeId, cancellationToken);
                if (forecastRecurrenceRuleType is not null)
                    baseForecast.ForecastRecurrenceRuleType = forecastRecurrenceRuleType;
            }
        }

        private static (DateOnly StartDate, DateOnly EndDate) GetSynchronizationWindow()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var startDate = new DateOnly(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(3).AddDays(-1);
            return (startDate, endDate);
        }
    }
}
