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
            .Where(x => x.IsActive && x.RecurrenceStart <= endDate && (x.RecurrenceEnd == null || x.RecurrenceEnd >= startDate))
            .Select(x => new ForecastDefinitionData(
                x.Id,
                x.Description,
                x.Amount,
                x.RecurrenceStart,
                x.RecurrenceEnd,
                x.Interval,
                x.ForecastRecurrenceRuleType.Code,
                x.PaymentCategoryId))
            .ToListAsync(cancellationToken);

        foreach (var forecast in forecastExpenses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var recurrence in GetRecurrences(forecast, startDate, endDate))
            {
                cancellationToken.ThrowIfCancellationRequested();
                expectedOccurrences[new OccurrenceKey(forecast.Id, false, recurrence)] = new OccurrenceSeed(
                    forecast.Id, false, forecast.Description, forecast.Amount, recurrence, forecast.PaymentCategoryId);
            }
        }

        var forecastIncomes = await dbContext.ForecastIncomes
            .AsNoTracking()
            .Where(x => x.IsActive && x.RecurrenceStart <= endDate && (x.RecurrenceEnd == null || x.RecurrenceEnd >= startDate))
            .Select(x => new ForecastDefinitionData(
                x.Id,
                x.Description,
                x.Amount,
                x.RecurrenceStart,
                x.RecurrenceEnd,
                x.Interval,
                x.ForecastRecurrenceRuleType.Code,
                null))
            .ToListAsync(cancellationToken);

        foreach (var forecast in forecastIncomes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var recurrence in GetRecurrences(forecast, startDate, endDate))
            {
                cancellationToken.ThrowIfCancellationRequested();
                expectedOccurrences[new OccurrenceKey(forecast.Id, true, recurrence)] = new OccurrenceSeed(
                    forecast.Id, true, forecast.Description, forecast.Amount, recurrence, null);
            }
        }

        var existingOccurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ExpectedDate >= startDate && x.ExpectedDate <= endDate)
            .ToListAsync(cancellationToken);

        Synchronize(expectedOccurrences, existingOccurrences, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void Synchronize(
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
                Id = Guid.CreateVersion7(),
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

    private static IEnumerable<DateOnly> GetRecurrences(ForecastDefinitionData forecast, DateOnly startDate, DateOnly endDate)
    {
        var occurrenceDate = forecast.RecurrenceStart;

        while (occurrenceDate <= endDate && (forecast.RecurrenceEnd is null || occurrenceDate <= forecast.RecurrenceEnd))
        {
            if (occurrenceDate >= startDate)
                yield return occurrenceDate;

            var nextOccurrence = GetNextOccurrence(occurrenceDate, forecast.Interval, forecast.RecurrenceCode);
            if (nextOccurrence is null)
                yield break;

            occurrenceDate = nextOccurrence.Value;
        }
    }

    private static DateOnly? GetNextOccurrence(DateOnly currentDate, int? interval, string recurrenceCode)
    {
        if (interval is not int value)
            return null;

        return recurrenceCode switch
        {
            ForecastRecurrenceRuleType.OneTime => null,
            ForecastRecurrenceRuleType.Day => currentDate.AddDays(value),
            ForecastRecurrenceRuleType.Week => currentDate.AddDays(7 * value),
            ForecastRecurrenceRuleType.Month => currentDate.AddMonths(value),
            ForecastRecurrenceRuleType.Year => currentDate.AddYears(value),
            _ => null,
        };
    }

    private readonly record struct ForecastDefinitionData(
        Guid Id,
        string Description,
        decimal Amount,
        DateOnly RecurrenceStart,
        DateOnly? RecurrenceEnd,
        int? Interval,
        string RecurrenceCode,
        Guid? PaymentCategoryId);
}
