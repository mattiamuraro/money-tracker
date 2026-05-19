using Xunit;

namespace MoneyTracker.Api.Tests.Architecture;

public class ProgramObservabilityArchitectureTests
{
    [Fact]
    public void Program_Should_Map_Readiness_And_Liveness_Health_Endpoints()
    {
        var sourceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        var programPath = Path.Combine(sourceRoot, "MoneyTracker.Api", "Program.cs");
        var programContent = File.ReadAllText(programPath);

        Assert.Contains("app.MapHealthChecks(\"/health/ready\")", programContent, StringComparison.Ordinal);
        Assert.Contains("app.MapHealthChecks(\"/health/live\"", programContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_Should_Register_Request_Observability_Middleware()
    {
        var sourceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        var programPath = Path.Combine(sourceRoot, "MoneyTracker.Api", "Program.cs");
        var programContent = File.ReadAllText(programPath);

        Assert.Contains("app.UseRequestObservability();", programContent, StringComparison.Ordinal);
    }
}
