namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;

public class GetPendingForecastIncomeOccurrencesQuery
{
    public int Year { get; set; }
    public int Month { get; set; }

    public GetPendingForecastIncomeOccurrencesQuery(int year, int month)
    {
        Year = year;
        Month = month;
    }
}
