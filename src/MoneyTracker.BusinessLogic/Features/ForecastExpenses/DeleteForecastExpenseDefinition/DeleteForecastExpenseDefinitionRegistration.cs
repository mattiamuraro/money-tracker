using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;

internal static class DeleteForecastExpenseDefinitionRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<DeleteForecastExpenseDefinitionCommandHandler, DeleteForecastExpenseDefinitionCommand>();

        return services;
    }
}