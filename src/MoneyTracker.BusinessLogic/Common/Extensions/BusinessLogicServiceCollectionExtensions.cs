using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Register;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DiscardPendingForecastOccurrence;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitions;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetPendingForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;

namespace MoneyTracker.BusinessLogic.Common.Extensions;

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
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RegisterCommandHandler>();

        services.AddScoped<CreatePaymentCommandHandler>();
        services.AddScoped<UpdatePaymentCommandHandler>();
        services.AddScoped<DeletePaymentCommandHandler>();
        services.AddScoped<GetPaymentQueryHandler>();
        services.AddScoped<GetPaymentByIdQueryHandler>();

        services.AddScoped<CreateIncomeCommandHandler>();
        services.AddScoped<UpdateIncomeCommandHandler>();
        services.AddScoped<DeleteIncomeCommandHandler>();
        services.AddScoped<GetIncomeQueryHandler>();
        services.AddScoped<GetIncomeByIdQueryHandler>();

        services.AddScoped<GetForecastRowsQueryHandler>();
        services.AddScoped<GetPendingForecastOccurrencesQueryHandler>();
        services.AddScoped<GetForecastRecurrenceRuleTypesQueryHandler>();
        services.AddScoped<GetForecastDefinitionsQueryHandler>();
        services.AddScoped<GetForecastDefinitionByIdQueryHandler>();
        services.AddScoped<SynchronizeForecastOccurrencesCommandHandler>();
        services.AddScoped<DiscardPendingForecastOccurrenceCommandHandler>();
        services.AddScoped<CreateForecastDefinitionCommandHandler>();
        services.AddScoped<UpdateForecastDefinitionCommandHandler>();
        services.AddScoped<DeleteForecastDefinitionCommandHandler>();

        services.AddScoped<CreateCategoryCommandHandler>();
        services.AddScoped<UpdateCategoryCommandHandler>();
        services.AddScoped<DeleteCategoryCommandHandler>();
        services.AddScoped<GetAllCategoriesQueryHandler>();
        services.AddScoped<GetCategoryByIdQueryHandler>();

        services.AddScoped<IValidator<CreatePaymentCommand>, CreatePaymentCommandValidator>();
        services.AddScoped<IValidator<UpdatePaymentCommand>, UpdatePaymentCommandValidator>();
        services.AddScoped<IValidator<CreateIncomeCommand>, CreateIncomeCommandValidator>();
        services.AddScoped<IValidator<UpdateIncomeCommand>, UpdateIncomeCommandValidator>();
        services.AddScoped<IValidator<CreateCategoryCommand>, CreateCategoryCommandValidator>();
        services.AddScoped<IValidator<UpdateCategoryCommand>, UpdateCategoryCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();
        services.AddScoped<IValidator<CreateForecastDefinitionCommand>, CreateForecastDefinitionCommandValidator>(); 
        services.AddScoped<IValidator<UpdateForecastDefinitionCommand>, UpdateForecastDefinitionCommandValidator>();

        return services;
    }
}

