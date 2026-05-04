namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;

public sealed class GetForecastIncomeRowsQuery
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public GetForecastIncomeRowsQuery(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}
