namespace MoneyTracker.BusinessLogic.Common.Models;

internal readonly record struct OccurrenceSeed(Guid ForecastDefinitionId, bool IsIncome, string Description, decimal Amount, DateOnly ExpectedDate, Guid? PaymentCategoryId);

