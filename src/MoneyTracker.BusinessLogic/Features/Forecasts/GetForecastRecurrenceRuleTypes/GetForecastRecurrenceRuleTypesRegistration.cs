using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;

internal static class GetForecastRecurrenceRuleTypesRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetForecastRecurrenceRuleTypesQueryHandler, GetForecastRecurrenceRuleTypesQuery, List<ForecastRecurrenceRuleTypeDto>>();

        return services;
    }
}