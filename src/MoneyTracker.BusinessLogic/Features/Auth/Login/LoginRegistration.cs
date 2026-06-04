using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Auth.Login;

internal static class LoginRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<LoginCommandHandler, LoginCommand, LoginAuthTokenDto>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();

        return services;
    }
}