using MoneyTracker.Api.Endpoints.Payments.Contracts;
using MoneyTracker.Api.ExtensionMethods;

namespace MoneyTracker.Api.Endpoints.Incomes.Contracts;

public class IncomeFilterQuery : PaginationQuery
{
    private const string MonthValidationMessage = "Month filter is required and must use yyyy-MM format.";

    public string? Month { get; set; }
    public string? DescriptionFilter { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public (int Year, int Month) GetRequiredYearMonth()
        => Month.GetRequiredYearMonth(MonthValidationMessage);
}
