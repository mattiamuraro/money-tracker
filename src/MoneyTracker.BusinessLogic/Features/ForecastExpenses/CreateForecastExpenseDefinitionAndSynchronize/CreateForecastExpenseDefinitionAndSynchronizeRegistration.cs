using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;

internal static class CreateForecastExpenseDefinitionAndSynchronizeRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateForecastExpenseDefinitionAndSynchronizeCommandHandler, CreateForecastExpenseDefinitionAndSynchronizeCommand, Guid>();

        return services;
    }
}