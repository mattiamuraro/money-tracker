using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.Shared;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

public class SynchronizeForecastOccurrencesCommandHandler(MoneyTrackerDbContext dbContext)
    : IHandler<SynchronizeForecastOccurrencesCommand>
{
    public async Task Handle(SynchronizeForecastOccurrencesCommand request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = ForecastSynchronizationWindow.GetWindow();
        var expectedOccurrences = new Dictionary<OccurrenceKey, OccurrenceSeed>();

        var forecastExpenses = await dbContext.ForecastExpenses
            .AsNoTracking()
            .Include(x => x.ForecastRecurrenceRuleType)
            .Include(x => x.PaymentCategory)
            .Where(x => x.IsActive && x.RecurrenceStart <= endDate && (x.RecurrenceEnd == null || x.RecurrenceEnd >= startDate))
            .ToListAsync(cancellationToken);

        foreach (var forecast in forecastExpenses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var recurrence in forecast.GetRecurrences(startDate, endDate))
            {
                cancellationToken.ThrowIfCancellationRequested();
                expectedOccurrences[new OccurrenceKey(forecast.Id, false, recurrence)] = new OccurrenceSeed(
                    forecast.Id, false, forecast.Description, forecast.Amount, recurrence, forecast.PaymentCategoryId);
            }
        }

        var forecastIncomes = await dbContext.ForecastIncomes
            .AsNoTracking()
            .Include(x => x.ForecastRecurrenceRuleType)
            .Where(x => x.IsActive && x.RecurrenceStart <= endDate && (x.RecurrenceEnd == null || x.RecurrenceEnd >= startDate))
            .ToListAsync(cancellationToken);

        foreach (var forecast in forecastIncomes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var recurrence in forecast.GetRecurrences(startDate, endDate))
            {
                cancellationToken.ThrowIfCancellationRequested();
                expectedOccurrences[new OccurrenceKey(forecast.Id, true, recurrence)] = new OccurrenceSeed(
                    forecast.Id, true, forecast.Description, forecast.Amount, recurrence, null);
            }
        }

        var existingOccurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ExpectedDate >= startDate && x.ExpectedDate <= endDate)
            .ToListAsync(cancellationToken);

        await SynchronizeAsync(expectedOccurrences, existingOccurrences, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SynchronizeAsync(
        Dictionary<OccurrenceKey, OccurrenceSeed> expectedOccurrences,
        List<ForecastOccurrence> existingOccurrences,
        CancellationToken cancellationToken)
    {
        var existingLookup = existingOccurrences.ToDictionary(
            x => new OccurrenceKey(x.ForecastDefinitionId, x.IsIncome, x.ExpectedDate), x => x);

        foreach (var existingOccurrence in existingOccurrences)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            if (existingOccurrence.ForecastOccurrenceStatusId is var statusId
                && (statusId == ForecastOccurrenceStatus.PendingId || statusId == ForecastOccurrenceStatus.CancelledId))
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
            cancellationToken.ThrowIfCancellationRequested();

            if (existingLookup.ContainsKey(key))
                continue;

            dbContext.ForecastOccurrences.Add(new ForecastOccurrence
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

        await Task.CompletedTask;
    }
}
