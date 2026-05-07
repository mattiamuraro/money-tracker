using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Features.Forecasts;

internal static class ForecastsFeatureRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetForecastRecurrenceRuleTypesQueryHandler, GetForecastRecurrenceRuleTypesQuery, List<ForecastRecurrenceRuleTypeDto>>();
        services.AddVoidHandlerWithLogging<SynchronizeForecastOccurrencesCommandHandler, SynchronizeForecastOccurrencesCommand>();

        return services;
    }
}
