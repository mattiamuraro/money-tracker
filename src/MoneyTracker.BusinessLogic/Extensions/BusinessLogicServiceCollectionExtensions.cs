using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.Forecasts.Queries.GetForecastRows;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.UpdateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Queries.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Queries.GetCategoryById;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Validators;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.UpdatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.Queries.GetPaymentHistory;
using MoneyTracker.BusinessLogic.Features.Payments.Validators;

namespace MoneyTracker.BusinessLogic.Extensions;

/// <summary>
/// Extension methods to register Business Logic services in Dependency Injection
/// </summary>
public static class BusinessLogicServiceCollectionExtensions
{
    /// <summary>
    /// Registers business logic handlers and validators.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
    {
        services.AddScoped<CreatePaymentCommandHandler>();
        services.AddScoped<UpdatePaymentCommandHandler>();
        services.AddScoped<DeletePaymentCommandHandler>();
        services.AddScoped<GetPaymentQueryHandler>();

        services.AddScoped<CreateCategoryCommandHandler>();
        services.AddScoped<UpdateCategoryCommandHandler>();
        services.AddScoped<DeleteCategoryCommandHandler>();
        services.AddScoped<GetAllCategoriesQueryHandler>();
        services.AddScoped<GetCategoryByIdQueryHandler>();

        services.AddScoped<GetForecastRowsQueryHandler>();

        services.AddScoped<IValidator<CreatePaymentCommand>, CreatePaymentCommandValidator>();
        services.AddScoped<IValidator<UpdatePaymentCommand>, UpdatePaymentCommandValidator>();
        services.AddScoped<IValidator<CreateCategoryCommand>, CreateCategoryCommandValidator>();
        services.AddScoped<IValidator<UpdateCategoryCommand>, UpdateCategoryCommandValidator>();

        return services;
    }
}

