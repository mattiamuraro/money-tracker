using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;

internal static class UpdateForecastExpenseDefinitionAndSynchronizeRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<UpdateForecastExpenseDefinitionAndSynchronizeCommandHandler, UpdateForecastExpenseDefinitionAndSynchronizeCommand>();

        return services;
    }
}