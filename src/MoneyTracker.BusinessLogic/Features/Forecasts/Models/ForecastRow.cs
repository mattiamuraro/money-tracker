namespace MoneyTracker.BusinessLogic.Features.Forecasts.Models;

public class ForecastRow
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public bool IsIncome { get; set; } = false;
}
