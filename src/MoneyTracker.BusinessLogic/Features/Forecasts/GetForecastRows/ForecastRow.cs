namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;

public class ForecastRow
{
    public Guid Id { get; set; }
    public Guid ForecastDefinitionId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public bool IsIncome { get; set; } = false;
    public Guid? PaymentCategoryId { get; set; }
    public string? Category { get; set; }
}
