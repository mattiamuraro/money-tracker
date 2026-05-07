namespace MoneyTracker.BusinessLogic.Features.Forecasts.Shared;

internal static class ForecastSynchronizationWindow
{
    internal static (DateOnly StartDate, DateOnly EndDate) GetWindow()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var startDate = new DateOnly(today.Year, today.Month, 1);
        var endDate = startDate.AddMonths(3).AddDays(-1);
        return (startDate, endDate);
    }
}
