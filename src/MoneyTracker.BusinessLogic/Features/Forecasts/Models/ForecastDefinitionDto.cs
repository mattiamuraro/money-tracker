namespace MoneyTracker.BusinessLogic.Features.Forecasts.Models;

public class ForecastDefinitionDto
{
    public Guid Id { get; set; }
    public Guid ForecastRecurrenceRuleTypeId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly RecurrenceStart { get; set; }
    public DateOnly? RecurrenceEnd { get; set; }
    public int DayInterval { get; set; }
    public bool IsIncome { get; set; }
}
