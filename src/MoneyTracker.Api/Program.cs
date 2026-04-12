using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker.Api.Endpoints;
using MoneyTracker.ApiService.ExtensionMethods;
using MoneyTracker.ApiService.Middleware;
using MoneyTracker.BusinessLogic.Extensions;
using MoneyTracker.Data.EntityFramework;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

static bool IsAllowedDevelopmentOrigin(string? origin)
{
    if (string.IsNullOrWhiteSpace(origin) || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    if (uri.Scheme is not ("http" or "https"))
    {
        return false;
    }

    return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
        || uri.Host.EndsWith(".dev.localhost", StringComparison.OrdinalIgnoreCase);
}

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(IsAllowedDevelopmentOrigin);
        }
        else
        {
            policy.WithOrigins(
                    "https://localhost:7001",
                    "http://localhost:5001",
                    "http://localhost:58100",
                    "http://127.0.0.1:58100",
                    "https://localhost:58100",
                    "https://127.0.0.1:58100")
                .SetIsOriginAllowedToAllowWildcardSubdomains();
        }

        policy.AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddHttpContextAccessor();


builder.AddServices();

builder.AddSqlServerDbContext<MoneyTrackerDbContext>("moneytacker-db");

// Register business logic services and handlers
builder.Services.AddBusinessLogicServices();

// Configure JWT Bearer authentication
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

// Configure the HTTP request pipeline.
// Add custom middleware (order matters!)
app.UseHttpsRedirection();
app.UseCorrelationId();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use response compression
app.UseResponseCompression();

// Use CORS
app.UseCors("default");

app.UseAuthentication();
app.UseAuthorization();

app.AddAuthApis();
app.AddPaymentApis();
app.AddIncomeApis();
app.AddPaymentCategoryApis();
app.AddForecastApis();

app.MapDefaultEndpoints();

app.Run();