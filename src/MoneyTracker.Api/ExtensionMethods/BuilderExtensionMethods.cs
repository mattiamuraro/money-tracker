using Microsoft.AspNetCore.Identity;
using MoneyTracker.Api.Endpoints.Auth.Services;
using MoneyTracker.Api.Options;
using MoneyTracker.Data;

namespace MoneyTracker.Api.ExtensionMethods
{
    internal static class BuilderExtensionMethods
    {
        internal static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
        }
    }
}
