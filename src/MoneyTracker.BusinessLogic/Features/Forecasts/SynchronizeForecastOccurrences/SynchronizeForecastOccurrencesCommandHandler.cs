using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

public class SynchronizeForecastOccurrencesCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public SynchronizeForecastOccurrencesCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(SynchronizeForecastOccurrencesCommand request, CancellationToken cancellationToken)
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

        var existingOccurrences = await _dbContext.ForecastOccurrences
            .Where(x => x.ExpectedDate >= startDate && x.ExpectedDate <= endDate)
            .ToListAsync(cancellationToken);

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

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static (DateOnly StartDate, DateOnly EndDate) GetSynchronizationWindow()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var startDate = new DateOnly(today.Year, today.Month, 1);
        var endDate = startDate.AddMonths(3).AddDays(-1);
        return (startDate, endDate);
    }

    private readonly record struct OccurrenceKey(Guid ForecastDefinitionId, bool IsIncome, DateOnly ExpectedDate);
    private readonly record struct OccurrenceSeed(Guid ForecastDefinitionId, bool IsIncome, string Description, decimal Amount, DateOnly ExpectedDate, Guid? PaymentCategoryId);
}
