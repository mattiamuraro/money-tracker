using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;

internal static class ChangeTokenRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<RefreshTokenCommandHandler, RefreshTokenCommand, RefreshAuthTokenDto>();
        services.AddHandlerWithLogging<RevokeRefreshTokenCommandHandler, RevokeRefreshTokenCommand, bool>();

        services.AddScoped<IValidator<RefreshTokenCommand>, RefreshTokenCommandValidator>();
        services.AddScoped<IValidator<RevokeRefreshTokenCommand>, RevokeRefreshTokenCommandValidator>();

        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
