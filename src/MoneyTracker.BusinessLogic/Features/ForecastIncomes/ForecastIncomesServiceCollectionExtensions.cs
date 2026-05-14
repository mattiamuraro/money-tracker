using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes;

internal static class ForecastIncomesServiceCollectionExtensions
{
    internal static IServiceCollection AddForecastIncomesFeatureServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services = CreateForecastIncomeDefinitionRegistration.RegisterServices(services);
        services = CreateForecastIncomeDefinitionAndSynchronizeRegistration.RegisterServices(services);
        services = DeleteForecastIncomeDefinitionRegistration.RegisterServices(services);
        services = DeleteForecastIncomeDefinitionAndSynchronizeRegistration.RegisterServices(services);
        services = DiscardForecastIncomeOccurrenceRegistration.RegisterServices(services);
        services = GetForecastIncomeDefinitionsRegistration.RegisterServices(services);
        services = GetForecastIncomeRowsRegistration.RegisterServices(services);
        services = GetPendingForecastIncomeOccurrencesRegistration.RegisterServices(services);
        services = UpdateForecastIncomeDefinitionRegistration.RegisterServices(services);
        services = UpdateForecastIncomeDefinitionAndSynchronizeRegistration.RegisterServices(services);

        return services;
    }
}
