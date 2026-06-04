using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Common.Models;

namespace MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;

internal static class GetIncomeRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetIncomeQueryHandler, GetIncomeQuery, PaginatedResponse<IncomeDto>>();

        return services;
    }
}