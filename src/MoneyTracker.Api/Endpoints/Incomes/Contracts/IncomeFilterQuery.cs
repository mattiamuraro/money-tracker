using MoneyTracker.Api.Endpoints.Payments.Contracts;
using System.Globalization;

namespace MoneyTracker.Api.Endpoints.Incomes.Contracts;

public class IncomeFilterQuery : PaginationQuery
{
    public string? Month { get; set; }
    public string? DescriptionFilter { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public (int Year, int Month) GetRequiredYearMonth()
    {
        if (string.IsNullOrWhiteSpace(Month))
            throw new ArgumentException("Month filter is required and must use yyyy-MM format.");

        if (!DateTime.TryParseExact(Month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedMonth))
            throw new ArgumentException("Month filter is required and must use yyyy-MM format.");

        return (parsedMonth.Year, parsedMonth.Month);
    }
}
