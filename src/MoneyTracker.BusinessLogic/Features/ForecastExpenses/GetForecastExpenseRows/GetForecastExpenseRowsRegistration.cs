using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;

internal static class GetForecastExpenseRowsRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetForecastExpenseRowsQueryHandler, GetForecastExpenseRowsQuery, List<ForecastExpenseDto>>();

        return services;
    }
}