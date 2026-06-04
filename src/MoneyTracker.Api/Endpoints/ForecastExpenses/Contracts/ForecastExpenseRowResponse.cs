namespace MoneyTracker.Api.Endpoints.ForecastExpenses.Contracts;

public class ForecastExpenseRowResponse
{
    public Guid Id { get; set; }
    public Guid ForecastDefinitionId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public Guid? PaymentCategoryId { get; set; }
    public string? Category { get; set; }
}
