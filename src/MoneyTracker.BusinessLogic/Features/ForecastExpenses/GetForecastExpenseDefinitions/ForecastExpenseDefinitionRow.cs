namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;

public class ForecastExpenseDefinitionDto
{
    public Guid Id { get; set; }
    public Guid ForecastRecurrenceRuleTypeId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly RecurrenceStart { get; set; }
    public DateOnly? RecurrenceEnd { get; set; }
    public int Interval { get; set; }
    public Guid PaymentCategoryId { get; set; }
    public required string Category { get; set; }
}
