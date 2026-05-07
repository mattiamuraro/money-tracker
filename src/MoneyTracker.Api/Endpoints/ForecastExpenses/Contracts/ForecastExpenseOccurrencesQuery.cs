using MoneyTracker.BusinessLogic.Common.Exceptions;

namespace MoneyTracker.Api.Endpoints.ForecastExpenses.Contracts;

public class ForecastExpenseOccurrencesQuery
{
    public string Month { get; set; } = string.Empty;

    public (int Year, int Month) GetRequiredYearMonth()
    {
        if (string.IsNullOrWhiteSpace(Month))
            throw new BadRequestException("Month is required and must use yyyy-MM format.");

        var parts = Month.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var year) || !int.TryParse(parts[1], out var month) || month is < 1 or > 12)
            throw new BadRequestException("Month is required and must use yyyy-MM format.");

        return (year, month);
    }
}
