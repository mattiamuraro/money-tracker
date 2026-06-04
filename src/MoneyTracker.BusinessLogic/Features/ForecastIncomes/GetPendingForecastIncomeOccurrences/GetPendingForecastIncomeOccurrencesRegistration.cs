using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;

internal static class GetPendingForecastIncomeOccurrencesRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetPendingForecastIncomeOccurrencesQueryHandler, GetPendingForecastIncomeOccurrencesQuery, List<ForecastIncomeOccurrenceDto>>();

        return services;
    }
}