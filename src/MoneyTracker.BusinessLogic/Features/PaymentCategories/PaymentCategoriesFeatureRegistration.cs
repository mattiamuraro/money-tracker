using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories;

internal static class PaymentCategoriesFeatureRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateCategoryCommandHandler, CreateCategoryCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateCategoryCommandHandler, UpdateCategoryCommand>();
        services.AddVoidHandlerWithLogging<DeleteCategoryCommandHandler, DeleteCategoryCommand>();
        services.AddHandlerWithLogging<GetAllCategoriesQueryHandler, GetAllCategoriesQuery, IEnumerable<PaymentCategoryRow>>();
        services.AddHandlerWithLogging<GetCategoryByIdQueryHandler, GetCategoryByIdQuery, PaymentCategoryDto>();
        services.AddScoped<IValidator<CreateCategoryCommand>, CreateCategoryCommandValidator>();
        services.AddScoped<IValidator<UpdateCategoryCommand>, UpdateCategoryCommandValidator>();

        return services;
    }
}
