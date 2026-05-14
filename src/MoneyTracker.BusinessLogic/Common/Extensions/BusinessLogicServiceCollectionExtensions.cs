using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.Auth;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes;
using MoneyTracker.BusinessLogic.Features.Forecasts;
using MoneyTracker.BusinessLogic.Features.Incomes;
using MoneyTracker.BusinessLogic.Features.PaymentCategories;
using MoneyTracker.BusinessLogic.Features.Payments;

namespace MoneyTracker.BusinessLogic.Common.Extensions;

/// <summary>
/// Registers business logic services grouped by vertical slice.
/// </summary>
public static class BusinessLogicServiceCollectionExtensions
{
    /// <summary>
    /// Registers all business logic handlers and validators.
    /// </summary>
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services = services.AddAuthFeatureServices();
        services = services.AddPaymentsFeatureServices();
        services = services.AddIncomesFeatureServices();
        services = services.AddPaymentCategoriesFeatureServices();
        services = services.AddForecastIncomesFeatureServices();
        services = services.AddForecastExpensesFeatureServices();
        services = services.AddForecastsFeatureServices();

        return services;
    }
}
