using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;

internal static class GetAllCategoriesRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetAllCategoriesQueryHandler, GetAllCategoriesQuery, IEnumerable<PaymentCategoryDto>>();

        return services;
    }
}