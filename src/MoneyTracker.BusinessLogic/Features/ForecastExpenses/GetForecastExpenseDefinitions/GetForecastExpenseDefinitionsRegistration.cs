using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;

internal static class GetForecastExpenseDefinitionsRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetForecastExpenseDefinitionsQueryHandler, GetForecastExpenseDefinitionsQuery, List<ForecastExpenseDefinitionDto>>();

        return services;
    }
}