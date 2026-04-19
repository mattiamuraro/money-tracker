namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;

public class ForecastRecurrenceRuleTypeDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
}
