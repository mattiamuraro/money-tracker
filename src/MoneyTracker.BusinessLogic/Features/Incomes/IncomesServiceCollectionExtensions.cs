using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

namespace MoneyTracker.BusinessLogic.Features.Incomes;

internal static class IncomesServiceCollectionExtensions
{
    internal static IServiceCollection AddIncomesFeatureServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services = CreateIncomeRegistration.RegisterServices(services);
        services = DeleteIncomeRegistration.RegisterServices(services);
        services = GetIncomeRegistration.RegisterServices(services);
        services = GetIncomeByIdRegistration.RegisterServices(services);
        services = UpdateIncomeRegistration.RegisterServices(services);

        return services;
    }
}
