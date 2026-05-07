using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

internal static class SynchronizeForecastOccurrencesRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<SynchronizeForecastOccurrencesCommandHandler, SynchronizeForecastOccurrencesCommand>();

        return services;
    }
}