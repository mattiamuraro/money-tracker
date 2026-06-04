namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;

public class ForecastExpenseOccurrenceDto
{
    public Guid Id { get; set; }
    public Guid ForecastDefinitionId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpectedDate { get; set; }
    public Guid? PaymentCategoryId { get; set; }
    public string? Category { get; set; }
}
