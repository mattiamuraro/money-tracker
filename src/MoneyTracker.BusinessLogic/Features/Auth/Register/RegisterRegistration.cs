using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Auth.Register;

internal static class RegisterRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<RegisterCommandHandler, RegisterCommand, RegisterAuthTokenDto>();
        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();

        return services;
    }
}
