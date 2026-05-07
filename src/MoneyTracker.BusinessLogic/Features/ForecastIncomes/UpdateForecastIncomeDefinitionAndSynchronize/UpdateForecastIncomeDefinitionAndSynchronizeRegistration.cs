using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;

internal static class UpdateForecastIncomeDefinitionAndSynchronizeRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<UpdateForecastIncomeDefinitionAndSynchronizeCommandHandler, UpdateForecastIncomeDefinitionAndSynchronizeCommand>();

        return services;
    }
}