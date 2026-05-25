namespace MoneyTracker.BusinessLogic.Features.Dashboard.GetDashboardSummary;

public class DashboardSummaryDto
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
