using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;
using MoneyTracker.Data.Base;

namespace MoneyTracker.BusinessLogic.ExtensionMethods.Mapping
{
    public static class ForecastRowMappingMethods
    {
        public static ForecastRow ToForecastRow(this BaseForecast forcast, DateOnly date, bool isIncome = false)
        {
            return new ForecastRow
            {
                Id = forcast.Id,
                Description = forcast.Description,
                Amount = forcast.Amount,
                Date = date,
                IsIncome = isIncome
            };
        }
    }
}

