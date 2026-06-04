using MoneyTracker.Api.ExtensionMethods;

namespace MoneyTracker.Api.Endpoints.Dashboard.Contracts;

public sealed class DashboardSummaryQuery
{
    private const string MonthValidationMessage = "Month filter is required and must use yyyy-MM format.";

    public string? Month { get; set; }

    public (int Year, int Month) GetRequiredYearMonth()
        => Month.GetRequiredYearMonth(MonthValidationMessage);
}
