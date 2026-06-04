using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Auth.ChangePassword;

internal static class ChangePasswordRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<ChangePasswordCommandHandler, ChangePasswordCommand, bool>();
        services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();

        return services;
    }
}
