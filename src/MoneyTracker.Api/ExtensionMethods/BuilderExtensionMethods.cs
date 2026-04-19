using Microsoft.AspNetCore.Identity;
using MoneyTracker.Api.Endpoints.Auth.Services;
using MoneyTracker.Data;

namespace MoneyTracker.Api.ExtensionMethods
{
    internal static class BuilderExtensionMethods
    {
        internal static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        }
    }
}
