using Microsoft.AspNetCore.Identity;
using MoneyTracker.BusinessLogic.Common.Options;
using MoneyTracker.Data;

namespace MoneyTracker.Api.ExtensionMethods
{
    internal static class BuilderExtensionMethods
    {
        internal static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
        }
    }
}
