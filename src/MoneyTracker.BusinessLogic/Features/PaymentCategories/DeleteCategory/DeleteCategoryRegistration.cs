using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;

internal static class DeleteCategoryRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<DeleteCategoryCommandHandler, DeleteCategoryCommand>();

        return services;
    }
}