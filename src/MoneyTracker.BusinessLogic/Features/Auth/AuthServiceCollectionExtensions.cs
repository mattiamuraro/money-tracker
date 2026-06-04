using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.Auth.ChangePassword;
using MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Register;

namespace MoneyTracker.BusinessLogic.Features.Auth;

internal static class AuthServiceCollectionExtensions
{
    internal static IServiceCollection AddAuthFeatureServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services = LoginRegistration.RegisterServices(services);
        services = RegisterRegistration.RegisterServices(services);
        services = ChangeTokenRegistration.RegisterServices(services);
        services = ChangePasswordRegistration.RegisterServices(services);

        return services;
    }
}
