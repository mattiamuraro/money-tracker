using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;

internal static class CreateCategoryRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateCategoryCommandHandler, CreateCategoryCommand, Guid>();
        services.AddScoped<IValidator<CreateCategoryCommand>, CreateCategoryCommandValidator>();

        return services;
    }
}