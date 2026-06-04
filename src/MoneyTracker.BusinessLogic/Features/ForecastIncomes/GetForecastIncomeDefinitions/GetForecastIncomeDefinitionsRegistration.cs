using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;

internal static class GetForecastIncomeDefinitionsRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetForecastIncomeDefinitionsQueryHandler, GetForecastIncomeDefinitionsQuery, List<ForecastIncomeDefinitionDto>>();

        return services;
    }
}