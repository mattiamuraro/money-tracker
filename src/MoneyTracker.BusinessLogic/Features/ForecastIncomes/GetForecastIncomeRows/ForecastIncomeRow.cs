namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;

public class ForecastIncomeDto
{
    public Guid Id { get; set; }
    public Guid ForecastDefinitionId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
}
