namespace MoneyTracker.BusinessLogic.Common.Services.ExtensionMethods
{
    internal static class ForecastOccurrencesHelper
    {
        public static (DateOnly StartDate, DateOnly EndDate) GetSynchronizationWindow()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var startDate = new DateOnly(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(3).AddDays(-1);
            return (startDate, endDate);
        }
    }
}
