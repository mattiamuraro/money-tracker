namespace MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;

public class CreateForecastDefinitionCommand
{
    public Guid ForecastRecurrenceRuleTypeId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly RecurrenceStart { get; set; }
    public DateOnly? RecurrenceEnd { get; set; }
    public int Interval { get; set; }
    public bool IsIncome { get; set; }
    public Guid? PaymentCategoryId { get; set; }
}
