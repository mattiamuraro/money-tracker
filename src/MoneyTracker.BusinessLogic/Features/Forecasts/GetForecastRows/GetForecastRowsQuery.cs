using MoneyTracker.BusinessLogic.Features.Forecasts.Models;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;

/// <summary>
/// Query to retrieve expense/income forecasts within a date range
/// </summary>
public class GetForecastRowsQuery
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public GetForecastRowsQuery(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}
