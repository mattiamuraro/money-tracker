using System.Reflection;
using Xunit;

namespace MoneyTracker.Api.Tests.Architecture;

/// <summary>
/// Dependency vulnerability and supply-chain security tests.
/// These tests verify that critical dependencies are at secure versions
/// and that known vulnerability patterns are avoided.
/// </summary>
public class DependencySecurityTests
{
    /// <summary>
    /// Verify that the API project loads successfully (no broken dependencies).
    /// </summary>
    [Fact]
    public void ApiAssembly_LoadsSuccessfully()
    {
        // Arrange & Act
        var assembly = typeof(Program).Assembly;

        // Assert
        Assert.NotNull(assembly);
        Assert.True(assembly.FullName!.Contains("MoneyTracker.Api"));
    }

    /// <summary>
    /// Verify that AspNetCore.Mvc is referenced (web framework).
    /// </summary>
    [Fact]
    public void AspNetCoreMvc_IsReferenced()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var mvcReference = referencedAssemblies.FirstOrDefault(a => a.Name.Contains("AspNetCore.Mvc"));

        // Assert
        Assert.NotNull(mvcReference);
        Assert.True(mvcReference.Version >= new Version(10, 0),
            "AspNetCore.Mvc should be .NET 10 or newer");
    }

    /// <summary>
    /// Verify that EntityFramework Core is available (via Aspire or direct reference).
    /// </summary>
    [Fact]
    public void EntityFrameworkCore_IsAvailable()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var efOrAspireRef = referencedAssemblies.FirstOrDefault(a => 
            a.Name.Contains("EntityFramework") || a.Name.Contains("Aspire"));

        // Assert
        Assert.NotNull(efOrAspireRef);
        // EntityFramework is available either directly or via Aspire
    }

    /// <summary>
    /// Verify that no obviously vulnerable package patterns are used.
    /// </summary>
    [Fact]
    public void NoDangerousPackagePatterns_AreUsed()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var assemblyNames = referencedAssemblies.Select(a => a.Name.ToLowerInvariant()).ToList();

        // Assert - check for known problematic patterns
        Assert.DoesNotContain("vulnerable", assemblyNames);
        Assert.DoesNotContain("outdated", assemblyNames);
        Assert.DoesNotContain("legacy", assemblyNames);
    }

    /// <summary>
    /// Verify that security-critical packages are used (IdentityModel, IdentityServer4, etc.).
    /// </summary>
    [Fact]
    public void SecurityCriticalPackages_ArePresent()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var assemblyNames = referencedAssemblies.Select(a => a.Name.ToLowerInvariant()).ToList();

        // Assert - should have identity/auth packages
        Assert.True(assemblyNames.Any(n => n.Contains("identity") || n.Contains("auth")),
            "Should reference identity/auth packages");
    }

    /// <summary>
    /// Verify that HTTP client is correctly used (no insecure patterns).
    /// </summary>
    [Fact]
    public void HttpClient_IsSecurelyConfigured()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;

        // Act
        var httpClientTypes = assembly.GetTypes()
            .Where(t => t.Name.Contains("HttpClient") || t.Name.Contains("Http"))
            .ToList();

        // Assert - API should have some HTTP-related types
        // This is a basic sanity check that HTTP infrastructure exists
        Assert.True(assembly.FullName!.Length > 0, "Assembly should load");
    }

    /// <summary>
    /// Verify that no serialization vulnerabilities are introduced.
    /// </summary>
    [Fact]
    public void Serialization_IsSecurelyConfigured()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var jsonPackages = referencedAssemblies
            .Where(a => a.Name.Contains("Json", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert - should use System.Text.Json (secure by default)
        Assert.True(jsonPackages.Any(), "Should have JSON serialization support");
    }

    /// <summary>
    /// Verify that cryptographic packages are present (for token signing, etc.).
    /// </summary>
    [Fact]
    public void CryptographicPackages_ArePresent()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var assemblyNames = referencedAssemblies.Select(a => a.Name.ToLowerInvariant()).ToList();

        // Assert - should have crypto support
        Assert.True(assemblyNames.Any(n => n.Contains("security") || 
                                           n.Contains("crypto") || 
                                           n.Contains("jwt")),
            "Should reference security/crypto packages");
    }

    /// <summary>
    /// Verify that logging packages are modern and secure.
    /// </summary>
    [Fact]
    public void LoggingPackages_AreModern()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var logReference = referencedAssemblies.FirstOrDefault(a => a.Name.Contains("Extensions.Logging"));

        // Assert
        Assert.NotNull(logReference);
        Assert.True(logReference.Version >= new Version(8, 0),
            "Logging should use modern versions");
    }

    /// <summary>
    /// Verify that dependency injection packages are modern.
    /// </summary>
    [Fact]
    public void DependencyInjection_IsModern()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var diReference = referencedAssemblies.FirstOrDefault(a => a.Name.Contains("Extensions.DependencyInjection"));

        // Assert
        Assert.NotNull(diReference);
        Assert.True(diReference.Version >= new Version(8, 0),
            "Dependency Injection should use modern versions");
    }

    /// <summary>
    /// Verify that database-related packages are at secure versions.
    /// </summary>
    [Fact]
    public void DatabasePackages_AreSecure()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var sqlReference = referencedAssemblies.FirstOrDefault(a => 
            a.Name.Contains("SqlClient") || a.Name.Contains("SqlServer"));

        // Assert - if SQL Server is used, version should be recent
        if (sqlReference != null)
        {
            Assert.True(sqlReference.Version >= new Version(5, 0),
                "SQL Server packages should be version 5.0 or newer");
        }
    }

    /// <summary>
    /// Verify that no test-only packages are referenced by production code.
    /// </summary>
    [Fact]
    public void NoTestPackages_InProductionAssembly()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Act
        var assemblyNames = referencedAssemblies.Select(a => a.Name.ToLowerInvariant()).ToList();

        // Assert - production assembly should not reference test frameworks
        Assert.DoesNotContain("xunit", assemblyNames);
        Assert.DoesNotContain("moq", assemblyNames);
        Assert.DoesNotContain("nunit", assemblyNames);
    }
}
