using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories;

internal static class PaymentCategoriesServiceCollectionExtensions
{
    internal static IServiceCollection AddPaymentCategoriesFeatureServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services = CreateCategoryRegistration.RegisterServices(services);
        services = DeleteCategoryRegistration.RegisterServices(services);
        services = GetAllCategoriesRegistration.RegisterServices(services);
        services = GetCategoryByIdRegistration.RegisterServices(services);
        services = UpdateCategoryRegistration.RegisterServices(services);

        return services;
    }
}
