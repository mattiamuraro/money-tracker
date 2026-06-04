using MoneyTracker.Api.ExtensionMethods;

namespace MoneyTracker.Api.Endpoints.ForecastIncomes.Contracts;

public class ForecastIncomeOccurrencesQuery
{
    private const string MonthValidationMessage = "Month is required and must use yyyy-MM format.";

    public string Month { get; set; } = string.Empty;

    public (int Year, int Month) GetRequiredYearMonth()
        => Month.GetRequiredYearMonth(MonthValidationMessage);
}
