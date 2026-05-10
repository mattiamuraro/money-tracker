using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
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
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.ServiceDefaults;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddResponseCompression(options =>
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.SetIsOriginAllowed(IsAllowedDevelopmentOrigin);
        else
            policy.WithOrigins(
                    "https://localhost:7001",
                    "http://localhost:5001",
                    "http://localhost:58100",
                    "http://127.0.0.1:58100",
                    "https://localhost:58100",
                    "https://127.0.0.1:58100")
                .SetIsOriginAllowedToAllowWildcardSubdomains();

        policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline (order matters)
app.UseHttpsRedirection();

// 1. Enrich all logs with CorrelationId
app.UseCorrelationId();

// 2. Catch all unhandled exceptions
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 3. Structured HTTP logging (method, path, status, duration)
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseResponseCompression();
app.UseCors("default");

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

app.Run();app.Run();