namespace MoneyTracker.Api.Endpoints.ForecastRecurrenceRuleTypes.Contracts;

public class ForecastRecurrenceRuleTypeResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
}
