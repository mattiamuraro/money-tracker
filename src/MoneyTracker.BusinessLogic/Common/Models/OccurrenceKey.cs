namespace MoneyTracker.BusinessLogic.Common.Models;

internal readonly record struct OccurrenceKey(Guid ForecastDefinitionId, bool IsIncome, DateOnly ExpectedDate);
