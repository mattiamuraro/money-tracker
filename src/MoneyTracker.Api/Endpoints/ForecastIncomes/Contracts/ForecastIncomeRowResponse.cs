namespace MoneyTracker.Api.Endpoints.ForecastIncomes.Contracts;

public class ForecastIncomeRowResponse
{
    public Guid Id { get; set; }
    public Guid ForecastDefinitionId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
}
