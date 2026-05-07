using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;

internal static class GetPendingForecastExpenseOccurrencesRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetPendingForecastExpenseOccurrencesQueryHandler, GetPendingForecastExpenseOccurrencesQuery, List<ForecastExpenseOccurrenceDto>>();

        return services;
    }
}