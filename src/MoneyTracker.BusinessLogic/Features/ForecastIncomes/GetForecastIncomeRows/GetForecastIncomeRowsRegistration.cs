using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;

internal static class GetForecastIncomeRowsRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetForecastIncomeRowsQueryHandler, GetForecastIncomeRowsQuery, List<ForecastIncomeDto>>();

        return services;
    }
}