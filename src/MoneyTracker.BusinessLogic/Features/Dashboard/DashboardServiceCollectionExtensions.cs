using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.Dashboard.GetDashboardSummary;

namespace MoneyTracker.BusinessLogic.Features.Dashboard;

internal static class DashboardServiceCollectionExtensions
{
    internal static IServiceCollection AddDashboardFeatureServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services = GetDashboardSummaryRegistration.RegisterServices(services);

        return services;
    }
}
