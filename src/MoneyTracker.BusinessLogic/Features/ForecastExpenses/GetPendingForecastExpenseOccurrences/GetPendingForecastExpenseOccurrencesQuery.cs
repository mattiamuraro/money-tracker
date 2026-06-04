namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;

public class GetPendingForecastExpenseOccurrencesQuery
{
    public int Year { get; set; }
    public int Month { get; set; }

    public GetPendingForecastExpenseOccurrencesQuery(int year, int month)
    {
        Year = year;
        Month = month;
    }
}
