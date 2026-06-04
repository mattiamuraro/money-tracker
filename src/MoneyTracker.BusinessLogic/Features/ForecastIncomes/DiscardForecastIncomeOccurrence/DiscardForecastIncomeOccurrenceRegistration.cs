using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;

internal static class DiscardForecastIncomeOccurrenceRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<DiscardForecastIncomeOccurrenceCommandHandler, DiscardForecastIncomeOccurrenceCommand>();

        return services;
    }
}