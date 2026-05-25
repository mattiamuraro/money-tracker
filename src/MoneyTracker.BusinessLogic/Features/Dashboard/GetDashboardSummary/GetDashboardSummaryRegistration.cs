using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Dashboard.GetDashboardSummary;

internal static class GetDashboardSummaryRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetDashboardSummaryQueryHandler, GetDashboardSummaryQuery, DashboardSummaryDto>();

        return services;
    }
}
