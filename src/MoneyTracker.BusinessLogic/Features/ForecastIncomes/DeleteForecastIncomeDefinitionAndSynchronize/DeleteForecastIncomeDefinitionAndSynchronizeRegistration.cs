using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize;

internal static class DeleteForecastIncomeDefinitionAndSynchronizeRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<DeleteForecastIncomeDefinitionAndSynchronizeCommandHandler, DeleteForecastIncomeDefinitionAndSynchronizeCommand>();

        return services;
    }
}