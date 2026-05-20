using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using AspNetIPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker.Api.Middleware;
using MoneyTracker.Api.Options;
using MoneyTracker.Api.Services;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.Auth;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.ServiceDefaults;

namespace MoneyTracker.Api.ExtensionMethods;

internal static class ApiServiceCollectionExtensions
{
    internal static Meter SecurityMeter { get; } = new("MoneyTracker.Api.Security", "1.0.0");
    internal static Counter<long> RateLimitRejectedCounter { get; } = SecurityMeter.CreateCounter<long>("ratelimit.rejected");

    internal static void AddApiApplicationServices(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddEnvironmentSecretProviders();

        builder.AddServiceDefaults();
        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApi();
        builder.Services.AddResponseCompression(options =>
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>());

        builder.AddApiOptions();
        builder.AddApiCors();
        builder.WebHost.ConfigureKestrel((context, kestrel) => ConfigureKestrel(context.Configuration, kestrel));

        builder.Services.AddRateLimiter(options => ConfigureRateLimiter(options));
        builder.Services.AddHttpLogging(logging => ConfigureHttpLogging(logging));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.AddSqlServerDbContext<MoneyTrackerDbContext>("moneytacker-db");
        builder.Services.AddBusinessLogicServices();
        builder.AddApiAuthentication();
        builder.Services.AddApiAuthorization();
        builder.Services.ConfigureApiForwardedHeaders(builder.Configuration);
        builder.Services.AddSingleton<IExceptionDetailSanitizer, ExceptionDetailSanitizer>();
        builder.Services.AddSingleton<ILoginAttemptService, MemoryCacheLoginAttemptService>();
    }

    private static void AddApiOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<CorsOptions>()
            .Bind(builder.Configuration.GetSection(CorsOptions.SectionName))
            .Validate(options => builder.Environment.IsDevelopment() || options.AllowedOrigins.Length > 0,
                "Cors:AllowedOrigins must contain at least one origin outside Development.")
            .Validate(options => options.AllowedOrigins.All(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                && uri.Scheme is "http" or "https"),
                "Cors:AllowedOrigins must contain only absolute http/https origins.")
            .ValidateOnStart();

        builder.Services.AddOptions<ReverseProxyOptions>()
            .Bind(builder.Configuration.GetSection(ReverseProxyOptions.SectionName))
            .Validate(options => options.ForwardLimit > 0, "ReverseProxy:ForwardLimit must be greater than 0.")
            .Validate(options => options.KnownProxies.All(value => IPAddress.TryParse(value, out _)),
                "ReverseProxy:KnownProxies must contain valid IP addresses.")
            .Validate(options => options.KnownNetworks.All(value => AspNetIPNetwork.TryParse(value, out _)),
                "ReverseProxy:KnownNetworks must contain valid CIDR values.")
            .ValidateOnStart();

        builder.Services.AddOptions<RequestLimitsOptions>()
            .Bind(builder.Configuration.GetSection(RequestLimitsOptions.SectionName))
            .Validate(options => options.MaxRequestBodySizeBytes > 0,
                "Security:RequestLimits:MaxRequestBodySizeBytes must be greater than 0.")
            .Validate(options => options.MaxRequestHeadersTotalSizeBytes > 0,
                "Security:RequestLimits:MaxRequestHeadersTotalSizeBytes must be greater than 0.")
            .ValidateOnStart();

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
    }

    private static void AddApiCors(this WebApplicationBuilder builder)
    {
        var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyNames.Public, policy =>
            {
                if (builder.Environment.IsDevelopment())
                    policy.SetIsOriginAllowed(IsAllowedDevelopmentOrigin);
                else
                    policy.WithOrigins(corsOptions.AllowedOrigins);

                policy.AllowAnyMethod().AllowAnyHeader();
            });

            options.AddPolicy(CorsPolicyNames.Credentialed, policy =>
            {
                if (builder.Environment.IsDevelopment())
                    policy.SetIsOriginAllowed(IsAllowedDevelopmentOrigin);
                else
                    policy.WithOrigins(corsOptions.AllowedOrigins);

                policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            });
        });
    }

    private static void AddApiAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var algorithm = context.SecurityToken switch
                        {
                            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtSecurityToken => jwtSecurityToken.Header.Alg,
                            Microsoft.IdentityModel.JsonWebTokens.JsonWebToken jsonWebToken => jsonWebToken.Alg,
                            _ => null
                        };

                        if (algorithm is null)
                        {
                            context.Fail("Invalid token type.");
                            return Task.CompletedTask;
                        }

                        if (!string.Equals(algorithm, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
                            context.Fail("Invalid token algorithm.");

                        return Task.CompletedTask;
                    }
                };
            });
    }

    private static void AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthAuthorization.Policies.ReadAccess, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(AuthAuthorization.PermissionClaimType, AuthAuthorization.Permissions.Read));

            options.AddPolicy(AuthAuthorization.Policies.WriteAccess, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(AuthAuthorization.PermissionClaimType, AuthAuthorization.Permissions.Write));
        });
    }

    private static void ConfigureApiForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            var reverseProxyOptions = configuration
                .GetSection(ReverseProxyOptions.SectionName)
                .Get<ReverseProxyOptions>() ?? new ReverseProxyOptions();

            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = reverseProxyOptions.ForwardLimit;

            options.KnownNetworks.Clear();
            foreach (var network in reverseProxyOptions.KnownNetworks)
            {
                if (AspNetIPNetwork.TryParse(network, out var ipNetwork))
                    options.KnownNetworks.Add(ipNetwork);
            }

            options.KnownProxies.Clear();
            foreach (var proxy in reverseProxyOptions.KnownProxies)
            {
                if (IPAddress.TryParse(proxy, out var ipAddress))
                    options.KnownProxies.Add(ipAddress);
            }
        });
    }

    private static void ConfigureKestrel(IConfiguration configuration, Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions kestrel)
    {
        var requestLimits = configuration.GetSection(RequestLimitsOptions.SectionName).Get<RequestLimitsOptions>()
            ?? new RequestLimitsOptions();

        kestrel.Limits.MaxRequestBodySize = requestLimits.MaxRequestBodySizeBytes;
        kestrel.Limits.MaxRequestHeadersTotalSize = requestLimits.MaxRequestHeadersTotalSizeBytes;
    }

    private static void ConfigureRateLimiter(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, cancellationToken) =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("RateLimiter");

            var endpointName = context.HttpContext.GetEndpoint()?.DisplayName ?? "unknown";
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            RateLimitRejectedCounter.Add(1,
                KeyValuePair.Create<string, object?>("endpoint", endpointName),
                KeyValuePair.Create<string, object?>("ip", remoteIp));

            logger.LogWarning(
                new EventId(1401, "RateLimitRejected"),
                "Rate limit rejected request for endpoint {Endpoint} from {RemoteIp}.",
                endpointName,
                remoteIp);

            context.HttpContext.Response.ContentType = "application/problem+json";
            var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                ? retryAfterValue.TotalSeconds
                : (double?)null;

            await context.HttpContext.Response.WriteAsJsonAsync(
                new
                {
                    title = "Too Many Requests",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Rate limit exceeded. Please retry later.",
                    retryAfterSeconds = retryAfter
                },
                cancellationToken);
        };

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: ResolveRateLimitPartition(context),
                factory: static _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

        options.AddPolicy("auth-login", context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: ResolveRateLimitPartition(context),
                factory: static _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

        options.AddPolicy("auth-register", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ResolveRateLimitPartition(context),
                factory: static _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }

    private static void ConfigureHttpLogging(HttpLoggingOptions logging)
    {
        logging.LoggingFields = HttpLoggingFields.RequestMethod
            | HttpLoggingFields.RequestPath
            | HttpLoggingFields.ResponseStatusCode
            | HttpLoggingFields.Duration;
        logging.CombineLogs = true;
    }

    private static bool IsAllowedDevelopmentOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin) || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme is not ("http" or "https"))
            return false;
        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith(".dev.localhost", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveRateLimitPartition(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
