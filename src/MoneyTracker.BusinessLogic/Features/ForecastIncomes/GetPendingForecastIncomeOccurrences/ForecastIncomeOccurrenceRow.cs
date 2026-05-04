namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;

public class ForecastIncomeOccurrenceRow
{
    public Guid Id { get; set; }
    public Guid ForecastDefinitionId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpectedDate { get; set; }
}
