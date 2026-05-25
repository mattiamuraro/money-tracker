using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MoneyTracker.Api.Endpoints.Auth;
using MoneyTracker.Api.Endpoints.Dashboard;
using MoneyTracker.Api.Endpoints.ForecastExpenses;
using MoneyTracker.Api.Endpoints.ForecastIncomes;
using MoneyTracker.Api.Endpoints.ForecastRecurrenceRuleTypes;
using MoneyTracker.Api.Endpoints.Incomes;
using MoneyTracker.Api.Endpoints.PaymentCategories;
using MoneyTracker.Api.Endpoints.Payments;
using MoneyTracker.Api.Middleware;
using MoneyTracker.Api.Options;
using MoneyTracker.ServiceDefaults;

namespace MoneyTracker.Api.ExtensionMethods;

internal static class ApiApplicationBuilderExtensions
{
    internal static void UseApiMiddlewarePipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseRequestObservability();
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseSecurityHeaders();
        app.UseJsonContentTypeEnforcement();
        app.UseHttpLogging();

        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseResponseCompression();
        app.UseCors(CorsPolicyNames.Public);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseUserScope();
    }

    internal static void LogApiStartup(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var apiVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
        app.Logger.LogInformation(
            new EventId(1201, "ApiStartup"),
            "Starting {ServiceName} v{ServiceVersion} in {EnvironmentName} environment.",
            app.Environment.ApplicationName,
            apiVersion,
            app.Environment.EnvironmentName);

        var startupRequestLimits = app.Services.GetRequiredService<IOptions<RequestLimitsOptions>>().Value;
        var startupReverseProxyOptions = app.Configuration
            .GetSection(ReverseProxyOptions.SectionName)
            .Get<ReverseProxyOptions>() ?? new ReverseProxyOptions();
        var startupCorsOptions = app.Configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();

        app.Logger.LogInformation(
            new EventId(1202, "ObservabilityStartupConfiguration"),
            "Observability startup configuration: HttpLoggingEnabled={HttpLoggingEnabled}, CorrelationMiddlewareEnabled={CorrelationMiddlewareEnabled}, UserScopeMiddlewareEnabled={UserScopeMiddlewareEnabled}, MaxRequestBodySizeBytes={MaxRequestBodySizeBytes}, MaxRequestHeadersTotalSizeBytes={MaxRequestHeadersTotalSizeBytes}, ForwardLimit={ForwardLimit}, KnownProxiesCount={KnownProxiesCount}, KnownNetworksCount={KnownNetworksCount}, AllowedOriginsCount={AllowedOriginsCount}.",
            true,
            true,
            true,
            startupRequestLimits.MaxRequestBodySizeBytes,
            startupRequestLimits.MaxRequestHeadersTotalSizeBytes,
            startupReverseProxyOptions.ForwardLimit,
            startupReverseProxyOptions.KnownProxies.Length,
            startupReverseProxyOptions.KnownNetworks.Length,
            startupCorsOptions.AllowedOrigins.Length);
    }

    internal static void MapApiEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.AddAuthApis();
        app.AddPaymentApis();
        app.AddIncomeApis();
        app.AddPaymentCategoryApis();
        app.AddForecastRecurrenceRuleTypeApis();
        app.AddForecastIncomeApis();
        app.AddForecastExpenseApis();
        app.AddDashboardApis();
        app.MapHealthChecks("/health/ready");
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live")
        });
        app.MapDefaultEndpoints();
    }
}
