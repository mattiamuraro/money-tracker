using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
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
using System.Reflection;
using System.Text;

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

var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.SetIsOriginAllowed(IsAllowedDevelopmentOrigin);
        else
            policy.WithOrigins(corsOptions.AllowedOrigins);

        policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

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

app.UseHttpsRedirection();

// 1. Enrich all logs with CorrelationId
app.UseCorrelationId();

// 2. Catch all unhandled exceptions
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseSecurityHeaders();

// 3. Structured HTTP logging (method, path, status, duration)
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseResponseCompression();
app.UseCors("default");
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