using MoneyTracker.Api.ExtensionMethods;
using Xunit;

namespace MoneyTracker.Api.Tests.ExtensionMethods;

public class CorsOriginValidatorTests
{
    [Theory]
    [InlineData("http://localhost")]
    [InlineData("http://localhost:3000")]
    [InlineData("https://localhost")]
    [InlineData("https://localhost:5173")]
    public void IsAllowedDevelopmentOrigin_ReturnsTrue_ForLocalhost(string origin)
    {
        Assert.True(CorsOriginValidator.IsAllowedDevelopmentOrigin(origin));
    }

    [Theory]
    [InlineData("http://127.0.0.1")]
    [InlineData("http://127.0.0.1:8080")]
    [InlineData("https://127.0.0.1:5001")]
    public void IsAllowedDevelopmentOrigin_ReturnsTrue_For127001(string origin)
    {
        Assert.True(CorsOriginValidator.IsAllowedDevelopmentOrigin(origin));
    }

    [Theory]
    [InlineData("http://app.dev.localhost")]
    [InlineData("https://app.dev.localhost:3000")]
    [InlineData("https://frontend.dev.localhost")]
    public void IsAllowedDevelopmentOrigin_ReturnsTrue_ForDevLocalhostSubdomain(string origin)
    {
        Assert.True(CorsOriginValidator.IsAllowedDevelopmentOrigin(origin));
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("https://evil.localhost.attacker.com")]
    [InlineData("https://notlocalhost.com")]
    public void IsAllowedDevelopmentOrigin_ReturnsFalse_ForExternalOrigins(string origin)
    {
        Assert.False(CorsOriginValidator.IsAllowedDevelopmentOrigin(origin));
    }

    [Theory]
    [InlineData("ftp://localhost")]
    [InlineData("ws://localhost")]
    [InlineData("file://localhost")]
    public void IsAllowedDevelopmentOrigin_ReturnsFalse_ForNonHttpSchemes(string origin)
    {
        Assert.False(CorsOriginValidator.IsAllowedDevelopmentOrigin(origin));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-uri")]
    public void IsAllowedDevelopmentOrigin_ReturnsFalse_ForNullOrInvalidOrigin(string? origin)
    {
        Assert.False(CorsOriginValidator.IsAllowedDevelopmentOrigin(origin));
    }
}
