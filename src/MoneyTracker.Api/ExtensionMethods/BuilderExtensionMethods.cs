using Microsoft.AspNetCore.Identity;
using MoneyTracker.Api.Middleware;
using MoneyTracker.Api.Options;
using MoneyTracker.Api.Services;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using System.Text;

namespace MoneyTracker.Api.ExtensionMethods
{
    internal static class BuilderExtensionMethods
    {
        internal static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            builder.Services.AddOptions<AuthOptions>()
                .Bind(builder.Configuration.GetSection(AuthOptions.SectionName));

            builder.Services.AddOptions<JwtOptions>()
                .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
                .Validate(options => !string.IsNullOrWhiteSpace(options.Key), "Jwt:Key is required.")
                .Validate(options => Encoding.UTF8.GetByteCount(options.Key) >= 32, "Jwt:Key must be at least 32 bytes.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
                .Validate(options => options.ExpiryMinutes > 0, "Jwt:ExpiryMinutes must be greater than 0.")
                .ValidateOnStart();

            builder.Services.AddOptions<RefreshTokenOptions>()
                .Bind(builder.Configuration.GetSection(RefreshTokenOptions.SectionName))
                .Validate(options => options.ExpiryDays >= 1, "Auth:RefreshToken:ExpiryDays must be at least 1.")
                .ValidateOnStart();

            builder.Services.AddOptions<PasswordPolicyOptions>()
                .Bind(builder.Configuration.GetSection(PasswordPolicyOptions.SectionName))
                .Validate(options => options.MinimumLength >= 8, "Auth:PasswordPolicy:MinimumLength must be at least 8.")
                .Validate(options => options.MaximumLength >= options.MinimumLength, "Auth:PasswordPolicy:MaximumLength must be greater or equal to minimum length.")
                .Validate(options => options.PasswordHistoryCount >= 1, "Auth:PasswordPolicy:PasswordHistoryCount must be at least 1.")
                .ValidateOnStart();

            builder.Services.AddOptions<LoginProtectionOptions>()
                .Bind(builder.Configuration.GetSection(LoginProtectionOptions.SectionName))
                .Validate(options => options.MaxFailedAttempts >= 1, "Security:LoginProtection:MaxFailedAttempts must be at least 1.")
                .Validate(options => options.LockoutMinutes >= 1, "Security:LoginProtection:LockoutMinutes must be at least 1.")
                .ValidateOnStart();

            builder.Services.Configure<ExceptionDetailOptions>(builder.Configuration.GetSection(ExceptionDetailOptions.SectionName));
            builder.Services.AddSingleton<IExceptionDetailSanitizer, ExceptionDetailSanitizer>();
            builder.Services.AddSingleton<ILoginAttemptService, MemoryCacheLoginAttemptService>();
        }
    }
}
