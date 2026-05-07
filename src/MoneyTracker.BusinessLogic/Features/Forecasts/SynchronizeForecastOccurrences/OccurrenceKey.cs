namespace MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

internal readonly record struct OccurrenceKey(Guid ForecastDefinitionId, bool IsIncome, DateOnly ExpectedDate);
