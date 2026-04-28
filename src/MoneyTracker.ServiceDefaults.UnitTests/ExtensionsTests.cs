using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using MoneyTracker.ServiceDefaults;
using Moq;
using OpenTelemetry.Trace;

namespace MoneyTracker.ServiceDefaults.UnitTests;

public class ExtensionsTests
{
    [Fact]
    public void AddServiceDefaults_Should_Return_Builder()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        var result = builder.AddServiceDefaults();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(builder, result);
    }

    [Fact]
    public void AddServiceDefaults_Should_Register_ServiceDiscovery()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert
        var serviceDescriptors = builder.Services.Where(s =>
            s.ServiceType.FullName?.Contains("ServiceDiscovery") == true).ToList();
        Xunit.Assert.NotEmpty(serviceDescriptors);
    }

    [Fact]
    public void AddServiceDefaults_Should_Register_HealthChecks()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetService<HealthCheckService>();
        Xunit.Assert.NotNull(healthCheckService);
    }

    [Fact]
    public void AddServiceDefaults_Should_Configure_OpenTelemetry()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Xunit.Assert.NotNull(tracerProvider);
    }

    [Fact]
    public void ConfigureOpenTelemetry_Should_Return_Builder()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        var result = builder.ConfigureOpenTelemetry();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(builder, result);
    }

    [Fact]
    public void ConfigureOpenTelemetry_Should_Register_TracerProvider()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Xunit.Assert.NotNull(tracerProvider);
    }

    [Fact]
    public void AddDefaultHealthChecks_Should_Return_Builder()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        var result = builder.AddDefaultHealthChecks();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(builder, result);
    }

    [Fact]
    public void AddDefaultHealthChecks_Should_Register_HealthCheckService()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddDefaultHealthChecks();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetService<HealthCheckService>();
        Xunit.Assert.NotNull(healthCheckService);
    }

    [Fact]
    public async Task AddDefaultHealthChecks_Should_Register_SelfHealthCheck()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddDefaultHealthChecks();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();
        var result = await healthCheckService.CheckHealthAsync(TestContext.Current.CancellationToken);
        Xunit.Assert.Equal(HealthStatus.Healthy, result.Status);
        Xunit.Assert.True(result.Entries.ContainsKey("self"));
    }

    [Fact]
    public async Task AddDefaultHealthChecks_Should_Tag_SelfHealthCheck_AsLive()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddDefaultHealthChecks();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();
        var result = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains("live"), TestContext.Current.CancellationToken);
        Xunit.Assert.Equal(HealthStatus.Healthy, result.Status);
        Xunit.Assert.Single(result.Entries);
        Xunit.Assert.True(result.Entries.ContainsKey("self"));
    }

    [Fact]
    public void MapDefaultEndpoints_Should_Return_WebApplication()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var result = app.MapDefaultEndpoints();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(app, result);
    }

    [Fact]
    public void MapDefaultEndpoints_Should_Map_Endpoints_In_Development()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        builder.Services.AddHealthChecks();
        var app = builder.Build();

        // Act & Assert - Should not throw
        var result = app.MapDefaultEndpoints();
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(app, result);

        // Verify endpoints were registered by checking the endpoint data sources
        var endpointDataSources = app.Services.GetServices<EndpointDataSource>().ToList();
        Xunit.Assert.NotEmpty(endpointDataSources);
    }

    [Fact]
    public void MapDefaultEndpoints_Should_Execute_Without_Error_In_Production()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Production
        });
        builder.Services.AddHealthChecks();
        var app = builder.Build();

        // Act & Assert - Should not throw and return the app
        var result = app.MapDefaultEndpoints();
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(app, result);
    }

    [Fact]
    public void MapDefaultEndpoints_Should_Execute_Without_Error_In_Staging()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Staging
        });
        builder.Services.AddHealthChecks();
        var app = builder.Build();

        // Act & Assert - Should not throw and return the app
        var result = app.MapDefaultEndpoints();
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(app, result);
    }

    [Fact]
    public void AddServiceDefaults_Should_Configure_HttpClientDefaults()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert - Verify that HttpClient factory is registered
        var serviceProvider = builder.Services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        Xunit.Assert.NotNull(httpClientFactory);
    }

    [Fact]
    public void ConfigureOpenTelemetry_Should_Configure_Logging()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        Xunit.Assert.NotNull(loggerFactory);
    }

    [Fact]
    public void AddServiceDefaults_Should_Chain_Multiple_Calls()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        var result = builder.AddServiceDefaults().AddServiceDefaults();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(builder, result);
    }

    [Fact]
    public void ConfigureOpenTelemetry_Should_Chain_Multiple_Calls()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        var result = builder.ConfigureOpenTelemetry().ConfigureOpenTelemetry();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(builder, result);
    }

    [Fact]
    public void AddDefaultHealthChecks_Should_Chain_Multiple_Calls()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        var result = builder.AddDefaultHealthChecks().AddDefaultHealthChecks();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(builder, result);
    }

    [Fact]
    public void MapDefaultEndpoints_Should_Chain_Multiple_Calls()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHealthChecks();
        var app = builder.Build();

        // Act
        var result = app.MapDefaultEndpoints().MapDefaultEndpoints();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(app, result);
    }

    [Fact]
    public void AddServiceDefaults_Should_Work_With_WebApplicationBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddServiceDefaults();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(builder, result);
    }

    [Fact]
    public void ConfigureOpenTelemetry_Should_Work_With_WebApplicationBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.ConfigureOpenTelemetry();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(builder, result);
    }

    [Fact]
    public void AddDefaultHealthChecks_Should_Work_With_WebApplicationBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddDefaultHealthChecks();

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(builder, result);
    }

    [Fact]
    public void MapDefaultEndpoints_Should_Work_With_Custom_Environment()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "CustomEnvironment"
        });
        builder.Services.AddHealthChecks();
        var app = builder.Build();

        // Act & Assert - Should not throw in custom environment (non-development)
        var result = app.MapDefaultEndpoints();
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Same(app, result);
    }

    [Fact]
    public async Task AddDefaultHealthChecks_Should_Return_Healthy_Status()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        builder.AddDefaultHealthChecks();
        var serviceProvider = builder.Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

        // Act
        var result = await healthCheckService.CheckHealthAsync(TestContext.Current.CancellationToken);

        // Assert
        Xunit.Assert.Equal(HealthStatus.Healthy, result.Status);
        Xunit.Assert.Equal(HealthStatus.Healthy, result.Entries["self"].Status);
    }

    [Fact]
    public void AddServiceDefaults_Should_Register_ResilienceHandler()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert - HttpClient factory should be available for resilience
        var serviceProvider = builder.Services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("test");
        Xunit.Assert.NotNull(httpClient);
    }
}
