using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Register;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;
using MoneyTracker.BusinessLogic.Common.Models;

namespace MoneyTracker.BusinessLogic.Common.Extensions;

/// <summary>
/// Extension methods to register Business Logic services in Dependency Injection.
/// Each handler is registered both as its concrete type (for direct injection at endpoints)
/// and as IHandler (for the logging decorator chain).
/// </summary>
public static class BusinessLogicServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
    {
        // Auth
        services.AddHandlerWithLogging<LoginCommandHandler, LoginCommand, LoginAuthToken>();
        services.AddHandlerWithLogging<RegisterCommandHandler, RegisterCommand, RegisterAuthToken>();

        // Payments
        services.AddHandlerWithLogging<CreatePaymentCommandHandler, CreatePaymentCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdatePaymentCommandHandler, UpdatePaymentCommand>();
        services.AddVoidHandlerWithLogging<DeletePaymentCommandHandler, DeletePaymentCommand>();
        services.AddHandlerWithLogging<GetPaymentQueryHandler, GetPaymentQuery, PaginatedResponse<PaymentRow>>();
        services.AddHandlerWithLogging<GetPaymentByIdQueryHandler, GetPaymentByIdQuery, PaymentRow>();

        // Incomes
        services.AddHandlerWithLogging<CreateIncomeCommandHandler, CreateIncomeCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateIncomeCommandHandler, UpdateIncomeCommand>();
        services.AddVoidHandlerWithLogging<DeleteIncomeCommandHandler, DeleteIncomeCommand>();
        services.AddHandlerWithLogging<GetIncomeQueryHandler, GetIncomeQuery, PaginatedResponse<IncomeRow>>();
        services.AddHandlerWithLogging<GetIncomeByIdQueryHandler, GetIncomeByIdQuery, IncomeRow>();

        // Forecasts (shared)
        services.AddHandlerWithLogging<GetForecastRecurrenceRuleTypesQueryHandler, GetForecastRecurrenceRuleTypesQuery, List<ForecastRecurrenceRuleTypeDto>>();
        services.AddVoidHandlerWithLogging<SynchronizeForecastOccurrencesCommandHandler, SynchronizeForecastOccurrencesCommand>();

        // Forecast Incomes
        services.AddHandlerWithLogging<CreateForecastIncomeDefinitionCommandHandler, CreateForecastIncomeDefinitionCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateForecastIncomeDefinitionCommandHandler, UpdateForecastIncomeDefinitionCommand>();
        services.AddVoidHandlerWithLogging<DeleteForecastIncomeDefinitionCommandHandler, DeleteForecastIncomeDefinitionCommand>();
        services.AddHandlerWithLogging<GetForecastIncomeDefinitionsQueryHandler, GetForecastIncomeDefinitionsQuery, List<ForecastIncomeDefinitionRow>>();
        services.AddHandlerWithLogging<GetForecastIncomeRowsQueryHandler, GetForecastIncomeRowsQuery, List<ForecastIncomeRow>>();
        services.AddHandlerWithLogging<GetPendingForecastIncomeOccurrencesQueryHandler, GetPendingForecastIncomeOccurrencesQuery, List<ForecastIncomeOccurrenceRow>>();
        services.AddVoidHandlerWithLogging<DiscardForecastIncomeOccurrenceCommandHandler, DiscardForecastIncomeOccurrenceCommand>();

        // Forecast Expenses
        services.AddHandlerWithLogging<CreateForecastExpenseDefinitionCommandHandler, CreateForecastExpenseDefinitionCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateForecastExpenseDefinitionCommandHandler, UpdateForecastExpenseDefinitionCommand>();
        services.AddVoidHandlerWithLogging<DeleteForecastExpenseDefinitionCommandHandler, DeleteForecastExpenseDefinitionCommand>();
        services.AddHandlerWithLogging<GetForecastExpenseDefinitionsQueryHandler, GetForecastExpenseDefinitionsQuery, List<ForecastExpenseDefinitionRow>>();
        services.AddHandlerWithLogging<GetForecastExpenseRowsQueryHandler, GetForecastExpenseRowsQuery, List<ForecastExpenseRow>>();
        services.AddHandlerWithLogging<GetPendingForecastExpenseOccurrencesQueryHandler, GetPendingForecastExpenseOccurrencesQuery, List<ForecastExpenseOccurrenceRow>>();
        services.AddVoidHandlerWithLogging<DiscardForecastExpenseOccurrenceCommandHandler, DiscardForecastExpenseOccurrenceCommand>();

        // Payment Categories
        services.AddHandlerWithLogging<CreateCategoryCommandHandler, CreateCategoryCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateCategoryCommandHandler, UpdateCategoryCommand>();
        services.AddVoidHandlerWithLogging<DeleteCategoryCommandHandler, DeleteCategoryCommand>();
        services.AddHandlerWithLogging<GetAllCategoriesQueryHandler, GetAllCategoriesQuery, IEnumerable<PaymentCategoryRow>>();
        services.AddHandlerWithLogging<GetCategoryByIdQueryHandler, GetCategoryByIdQuery, PaymentCategoryDto>();

        // Validators
        services.AddScoped<IValidator<CreatePaymentCommand>, CreatePaymentCommandValidator>();
        services.AddScoped<IValidator<UpdatePaymentCommand>, UpdatePaymentCommandValidator>();
        services.AddScoped<IValidator<CreateIncomeCommand>, CreateIncomeCommandValidator>();
        services.AddScoped<IValidator<UpdateIncomeCommand>, UpdateIncomeCommandValidator>();
        services.AddScoped<IValidator<CreateCategoryCommand>, CreateCategoryCommandValidator>();
        services.AddScoped<IValidator<UpdateCategoryCommand>, UpdateCategoryCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();
        services.AddScoped<IValidator<CreateForecastIncomeDefinitionCommand>, CreateForecastIncomeDefinitionCommandValidator>();
        services.AddScoped<IValidator<UpdateForecastIncomeDefinitionCommand>, UpdateForecastIncomeDefinitionCommandValidator>();
        services.AddScoped<IValidator<CreateForecastExpenseDefinitionCommand>, CreateForecastExpenseDefinitionCommandValidator>();
        services.AddScoped<IValidator<UpdateForecastExpenseDefinitionCommand>, UpdateForecastExpenseDefinitionCommandValidator>();

        return services;
    }

    /// <summary>
    /// Registers a handler that returns TResult with a logging decorator.
    /// The concrete type is also registered directly so endpoints can still inject it by type.
    /// </summary>
    private static IServiceCollection AddHandlerWithLogging<THandler, TRequest, TResult>(
        this IServiceCollection services)
        where THandler : class, IHandler<TRequest, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IHandler<TRequest, TResult>>(sp =>
            new LoggingHandlerDecorator<TRequest, TResult>(
                sp.GetRequiredService<THandler>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingHandlerDecorator<TRequest, TResult>>>()));
        return services;
    }

    /// <summary>
    /// Registers a void handler (no return value) with a logging decorator.
    /// </summary>
    private static IServiceCollection AddVoidHandlerWithLogging<THandler, TRequest>(
        this IServiceCollection services)
        where THandler : class, IHandler<TRequest>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IHandler<TRequest>>(sp =>
            new LoggingHandlerDecorator<TRequest>(
                sp.GetRequiredService<THandler>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingHandlerDecorator<TRequest>>>()));
        return services;
    }
}

