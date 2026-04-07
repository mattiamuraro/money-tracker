using MoneyTracker.Api.Endpoints;
using MoneyTracker.ApiService.ExtensionMethods;
using MoneyTracker.ApiService.Middleware;
using MoneyTracker.BusinessLogic.Extensions;
using MoneyTracker.Data.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

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
        policy.WithOrigins(
                "https://localhost:7001",
                "http://localhost:5001",
                "http://localhost:58100",
                "http://127.0.0.1:58100",
                "https://localhost:58100",
                "https://127.0.0.1:58100")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddHttpContextAccessor();


builder.AddServices();

builder.AddSqlServerDbContext<MoneyTrackerDbContext>("moneytacker-db");

// Register business logic services including MediatR
builder.Services.AddBusinessLogicServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Add custom middleware (order matters!)
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

app.AddPaymentApis();
app.AddPaymentCategoryApis();
app.AddForecastApis();

app.MapDefaultEndpoints();

app.Run();