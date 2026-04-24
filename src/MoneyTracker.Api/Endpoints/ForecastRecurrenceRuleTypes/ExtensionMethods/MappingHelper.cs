using MoneyTracker.Api.Endpoints.ForecastRecurrenceRuleTypes.Contracts;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;

namespace MoneyTracker.Api.Endpoints.ForecastRecurrenceRuleTypes.ExtensionMethods
{
    internal static class MappingHelper
    {
        public static ForecastRecurrenceRuleTypeResponse ToForecastRecurrenceRuleTypeResponse(this ForecastRecurrenceRuleTypeDto entity)
        {
            return new ForecastRecurrenceRuleTypeResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code
            };
        }
    }
}
