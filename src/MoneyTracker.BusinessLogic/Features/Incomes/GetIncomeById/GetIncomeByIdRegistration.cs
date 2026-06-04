using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;

namespace MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;

internal static class GetIncomeByIdRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetIncomeByIdQueryHandler, GetIncomeByIdQuery, IncomeDto>();

        return services;
    }
}