using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Handlers;

namespace MoneyTracker.BusinessLogic.Common.Extensions;

internal static class HandlerRegistrationServiceCollectionExtensions
{
    /// <summary>
    /// Registers a handler that returns a result with structured logging.
    /// </summary>
    internal static IServiceCollection AddHandlerWithLogging<THandler, TRequest, TResult>(
        this IServiceCollection services)
        where THandler : class, IHandler<TRequest, TResult>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<THandler>();
        services.AddScoped<IHandler<TRequest, TResult>>(sp =>
            new LoggingHandlerDecorator<TRequest, TResult>(
                sp.GetRequiredService<THandler>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingHandlerDecorator<TRequest, TResult>>>()));

        return services;
    }

    /// <summary>
    /// Registers a handler that does not return a result with structured logging.
    /// </summary>
    internal static IServiceCollection AddVoidHandlerWithLogging<THandler, TRequest>(
        this IServiceCollection services)
        where THandler : class, IHandler<TRequest>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<THandler>();
        services.AddScoped<IHandler<TRequest>>(sp =>
            new LoggingHandlerDecorator<TRequest>(
                sp.GetRequiredService<THandler>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingHandlerDecorator<TRequest>>>()));

        return services;
    }
}
