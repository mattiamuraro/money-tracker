using Xunit;

namespace MoneyTracker.Api.Tests.Architecture;

public class ProgramObservabilityArchitectureTests
{
    [Fact]
    public void Program_Should_Compose_Startup_Through_Shared_Extensions()
    {
        var programContent = ReadApiFile("Program.cs");

        Assert.Contains("builder.AddApiApplicationServices();", programContent, StringComparison.Ordinal);
        Assert.Contains("app.LogApiStartup();", programContent, StringComparison.Ordinal);
        Assert.Contains("app.UseApiMiddlewarePipeline();", programContent, StringComparison.Ordinal);
        Assert.Contains("app.MapApiEndpoints();", programContent, StringComparison.Ordinal);
    }

    [Fact]
    public void ApiApplicationBuilderExtensions_Should_Map_Readiness_And_Liveness_Health_Endpoints()
    {
        var content = ReadApiFile("ExtensionMethods/ApiApplicationBuilderExtensions.cs");

        Assert.Contains("app.MapHealthChecks(\"/health/ready\")", content, StringComparison.Ordinal);
        Assert.Contains("app.MapHealthChecks(\"/health/live\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void ApiApplicationBuilderExtensions_Should_Register_Request_Observability_Middleware()
    {
        var content = ReadApiFile("ExtensionMethods/ApiApplicationBuilderExtensions.cs");

        Assert.Contains("app.UseRequestObservability();", content, StringComparison.Ordinal);
    }

    private static string ReadApiFile(string relativePath)
    {
        var sourceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        var filePath = Path.Combine(sourceRoot, "MoneyTracker.Api", relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.ReadAllText(filePath);
    }
}
