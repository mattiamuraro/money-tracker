using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;

internal static class DiscardForecastExpenseOccurrenceRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<DiscardForecastExpenseOccurrenceCommandHandler, DiscardForecastExpenseOccurrenceCommand>();

        return services;
    }
}