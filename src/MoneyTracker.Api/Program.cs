using System.Diagnostics.Metrics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker.Api.Endpoints.Auth;
using MoneyTracker.Api.Endpoints.ForecastExpenses;
using MoneyTracker.Api.Endpoints.ForecastIncomes;
using MoneyTracker.Api.Endpoints.ForecastRecurrenceRuleTypes;
using MoneyTracker.Api.Endpoints.Incomes;
using MoneyTracker.Api.Endpoints.PaymentCategories;
using MoneyTracker.Api.Endpoints.Payments;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.Api.Middleware;
using MoneyTracker.Api.Options;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.Auth;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddEnvironmentSecretProviders();

static bool IsAllowedDevelopmentOrigin(string? origin)
{
    if (string.IsNullOrWhiteSpace(origin) || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        return false;
    if (uri.Scheme is not ("http" or "https"))
        return false;
    return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
        || uri.Host.EndsWith(".dev.localhost", StringComparison.OrdinalIgnoreCase);
}

static string ResolveRateLimitPartition(HttpContext context)
    => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

var securityMeter = new Meter("MoneyTracker.Api.Security", "1.0.0");
var rateLimitRejectedCounter = securityMeter.CreateCounter<long>("ratelimit.rejected");

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddResponseCompression(options =>
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>());

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
    .Validate(options => options.KnownNetworks.All(value => Microsoft.AspNetCore.HttpOverrides.IPNetwork.TryParse(value, out _)),
        "ReverseProxy:KnownNetworks must contain valid CIDR values.")
    .ValidateOnStart();

builder.Services.AddOptions<RequestLimitsOptions>()
    .Bind(builder.Configuration.GetSection(RequestLimitsOptions.SectionName))
    .Validate(options => options.MaxRequestBodySizeBytes > 0,
        "Security:RequestLimits:MaxRequestBodySizeBytes must be greater than 0.")
    .Validate(options => options.MaxRequestHeadersTotalSizeBytes > 0,
        "Security:RequestLimits:MaxRequestHeadersTotalSizeBytes must be greater than 0.")
    .ValidateOnStart();

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

builder.WebHost.ConfigureKestrel((context, kestrel) =>
{
    var requestLimits = context.Configuration.GetSection(RequestLimitsOptions.SectionName).Get<RequestLimitsOptions>()
        ?? new RequestLimitsOptions();

    kestrel.Limits.MaxRequestBodySize = requestLimits.MaxRequestBodySizeBytes;
    kestrel.Limits.MaxRequestHeadersTotalSize = requestLimits.MaxRequestHeadersTotalSizeBytes;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiter");

        var endpointName = context.HttpContext.GetEndpoint()?.DisplayName ?? "unknown";
        var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        rateLimitRejectedCounter.Add(1,
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
});

// Configure HTTP request/response logging (replaces the old RequestResponseLoggingMiddleware)
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    logging.CombineLogs = true;
});

builder.Services.AddHttpContextAccessor();
builder.AddServices();
builder.AddSqlServerDbContext<MoneyTrackerDbContext>("moneytacker-db");
builder.Services.AddBusinessLogicServices();

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
                {
                    context.Fail("Invalid token algorithm.");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthAuthorization.Policies.ReadAccess, policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim(AuthAuthorization.PermissionClaimType, AuthAuthorization.Permissions.Read));

    options.AddPolicy(AuthAuthorization.Policies.WriteAccess, policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim(AuthAuthorization.PermissionClaimType, AuthAuthorization.Permissions.Write));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    var reverseProxyOptions = builder.Configuration
        .GetSection(ReverseProxyOptions.SectionName)
        .Get<ReverseProxyOptions>() ?? new ReverseProxyOptions();

    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = reverseProxyOptions.ForwardLimit;

    options.KnownNetworks.Clear();
    foreach (var network in reverseProxyOptions.KnownNetworks)
    {
        if (Microsoft.AspNetCore.HttpOverrides.IPNetwork.TryParse(network, out var ipNetwork))
            options.KnownNetworks.Add(ipNetwork);
    }

    options.KnownProxies.Clear();
    foreach (var proxy in reverseProxyOptions.KnownProxies)
    {
        if (IPAddress.TryParse(proxy, out var ipAddress))
            options.KnownProxies.Add(ipAddress);
    }
});

var app = builder.Build();

var apiVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
app.Logger.LogInformation(
    new EventId(1201, "ApiStartup"),
    "Starting {ServiceName} v{ServiceVersion} in {EnvironmentName} environment.",
    app.Environment.ApplicationName,
    apiVersion,
    app.Environment.EnvironmentName);

// Middleware pipeline (order matters)
if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseForwardedHeaders();
app.UseHttpsRedirection();

// 1. Enrich all logs with CorrelationId
app.UseCorrelationId();

// 2. Catch all unhandled exceptions
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseSecurityHeaders();
app.UseJsonContentTypeEnforcement();

// 3. Structured HTTP logging (method, path, status, duration)
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseResponseCompression();
app.UseCors(CorsPolicyNames.Public);
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// 4. Enrich all logs with authenticated user context
app.UseUserScope();

app.AddAuthApis();
app.AddPaymentApis();
app.AddIncomeApis();
app.AddPaymentCategoryApis();
app.AddForecastRecurrenceRuleTypeApis();
app.AddForecastIncomeApis();
app.AddForecastExpenseApis();

app.MapDefaultEndpoints();

app.Run();