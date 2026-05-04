namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;

public sealed class GetForecastExpenseRowsQuery
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public GetForecastExpenseRowsQuery(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}
