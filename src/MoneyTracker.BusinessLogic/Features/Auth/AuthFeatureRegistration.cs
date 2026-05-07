using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Register;

namespace MoneyTracker.BusinessLogic.Features.Auth;

internal static class AuthFeatureRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<LoginCommandHandler, LoginCommand, LoginAuthTokenDto>();
        services.AddHandlerWithLogging<RegisterCommandHandler, RegisterCommand, RegisterAuthTokenDto>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();

        return services;
    }
}
