namespace MoneyTracker.Api.Endpoints.Dashboard.Contracts;

public sealed class DashboardSummaryResponse
{
    public int PaymentsCount { get; set; }
    public decimal PaymentTotal { get; set; }
    public string? LatestPaymentDescription { get; set; }
    public DateTime? LatestPaymentDate { get; set; }
    public decimal ForecastIncomeTotal { get; set; }
    public decimal ForecastExpenseTotal { get; set; }
    public decimal ForecastBalance { get; set; }
    public string? NextUpcomingExpenseDescription { get; set; }
}
