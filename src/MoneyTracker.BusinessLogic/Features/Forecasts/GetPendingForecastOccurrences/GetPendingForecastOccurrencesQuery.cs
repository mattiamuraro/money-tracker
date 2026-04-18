namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetPendingForecastOccurrences;

public class GetPendingForecastOccurrencesQuery
{
    public int Year { get; set; }
    public int Month { get; set; }
    public bool IsIncome { get; set; }

    public GetPendingForecastOccurrencesQuery(int year, int month, bool isIncome)
    {
        Year = year;
        Month = month;
        IsIncome = isIncome;
    }
}
