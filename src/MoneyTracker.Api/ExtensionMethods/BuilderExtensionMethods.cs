using Microsoft.AspNetCore.Identity;
using MoneyTracker.Api.Auth;
using MoneyTracker.Api.Services;
using MoneyTracker.BusinessLogic.Services;
using MoneyTracker.Data;

namespace MoneyTracker.ApiService.ExtensionMethods
{
    internal static class BuilderExtensionMethods
    {
        internal static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<PaymentCategoryService>();
            builder.Services.AddScoped<ForecastService>();
            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        }
    }
}
