namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;

public class CreateForecastIncomeDefinitionCommand
{
    public Guid ForecastRecurrenceRuleTypeId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly RecurrenceStart { get; set; }
    public DateOnly? RecurrenceEnd { get; set; }
    public int Interval { get; set; }
}
