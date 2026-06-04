using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;

internal static class GetCategoryByIdRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetCategoryByIdQueryHandler, GetCategoryByIdQuery, PaymentCategoryDto>();

        return services;
    }
}