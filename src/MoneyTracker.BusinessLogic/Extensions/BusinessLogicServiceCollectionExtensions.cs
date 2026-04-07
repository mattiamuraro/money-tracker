using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.Forecasts.Queries.GetForecastRows;
using MoneyTracker.BusinessLogic.Shared.Behaviors;
using FluentValidation;
using MoneyTracker.BusinessLogic.Features.Payments.Validators;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.UpdatePayment;

namespace MoneyTracker.BusinessLogic.Extensions;

/// <summary>
/// Extension methods to register Business Logic services in Dependency Injection
/// </summary>
public static class BusinessLogicServiceCollectionExtensions
{
    /// <summary>
    /// Registers CQRS services with MediatR following the Vertical Slice Architecture pattern
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
    {
        // Register MediatR with the BusinessLogic assembly
        // Uses GetForecastRowsQueryHandler as a marker type to locate the assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<GetForecastRowsQueryHandler>();

            // Register pipeline behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Manually register FluentValidation validators
        services.AddScoped<IValidator<CreatePaymentCommand>, CreatePaymentCommandValidator>();
        services.AddScoped<IValidator<UpdatePaymentCommand>, UpdatePaymentCommandValidator>();

        return services;
    }
}

