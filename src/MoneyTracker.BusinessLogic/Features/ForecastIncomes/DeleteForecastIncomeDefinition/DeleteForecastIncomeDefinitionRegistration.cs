using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;

internal static class DeleteForecastIncomeDefinitionRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<DeleteForecastIncomeDefinitionCommandHandler, DeleteForecastIncomeDefinitionCommand>();

        return services;
    }
}