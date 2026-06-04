namespace MoneyTracker.Api.Endpoints.ForecastIncomes.Contracts;

public class ForecastIncomeDefinitionResponse
{
    public Guid Id { get; set; }
    public Guid ForecastRecurrenceRuleTypeId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly RecurrenceStart { get; set; }
    public DateOnly? RecurrenceEnd { get; set; }
    public int Interval { get; set; }
}
