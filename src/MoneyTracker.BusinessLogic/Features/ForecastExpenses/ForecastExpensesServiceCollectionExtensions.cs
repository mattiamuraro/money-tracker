using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses;

internal static class ForecastExpensesServiceCollectionExtensions
{
    internal static IServiceCollection AddForecastExpensesFeatureServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services = CreateForecastExpenseDefinitionRegistration.RegisterServices(services);
        services = CreateForecastExpenseDefinitionAndSynchronizeRegistration.RegisterServices(services);
        services = DeleteForecastExpenseDefinitionRegistration.RegisterServices(services);
        services = DeleteForecastExpenseDefinitionAndSynchronizeRegistration.RegisterServices(services);
        services = DiscardForecastExpenseOccurrenceRegistration.RegisterServices(services);
        services = GetForecastExpenseDefinitionsRegistration.RegisterServices(services);
        services = GetForecastExpenseRowsRegistration.RegisterServices(services);
        services = GetPendingForecastExpenseOccurrencesRegistration.RegisterServices(services);
        services = UpdateForecastExpenseDefinitionRegistration.RegisterServices(services);
        services = UpdateForecastExpenseDefinitionAndSynchronizeRegistration.RegisterServices(services);

        return services;
    }
}
