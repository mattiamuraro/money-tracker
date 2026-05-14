using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Features.Forecasts;

internal static class ForecastsServiceCollectionExtensions
{
    internal static IServiceCollection AddForecastsFeatureServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services = GetForecastRecurrenceRuleTypesRegistration.RegisterServices(services);
        services = SynchronizeForecastOccurrencesRegistration.RegisterServices(services);

        return services;
    }
}
