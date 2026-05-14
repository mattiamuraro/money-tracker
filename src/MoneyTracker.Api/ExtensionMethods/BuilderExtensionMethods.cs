using Microsoft.AspNetCore.Identity;
using MoneyTracker.Api.Middleware;
using MoneyTracker.Api.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
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
            builder.Services.Configure<ExceptionDetailOptions>(builder.Configuration.GetSection(ExceptionDetailOptions.SectionName));
            builder.Services.AddSingleton<IExceptionDetailSanitizer, ExceptionDetailSanitizer>();
        }
    }
}
