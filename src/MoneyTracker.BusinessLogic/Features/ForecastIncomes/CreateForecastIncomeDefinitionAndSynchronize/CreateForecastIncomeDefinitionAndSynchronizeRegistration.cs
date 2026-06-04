using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;

internal static class CreateForecastIncomeDefinitionAndSynchronizeRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateForecastIncomeDefinitionAndSynchronizeCommandHandler, CreateForecastIncomeDefinitionAndSynchronizeCommand, Guid>();

        return services;
    }
}