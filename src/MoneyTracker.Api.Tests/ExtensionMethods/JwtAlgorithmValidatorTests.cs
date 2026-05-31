using Microsoft.IdentityModel.Tokens;
using MoneyTracker.Api.ExtensionMethods;
using Xunit;

namespace MoneyTracker.Api.Tests.ExtensionMethods;

public class JwtAlgorithmValidatorTests
{
    [Fact]
    public void Validate_WithHmacSha256_ReturnsNull()
    {
        var error = JwtAlgorithmValidator.Validate(SecurityAlgorithms.HmacSha256);

        Assert.Null(error);
    }

    [Fact]
    public void Validate_WithNullAlgorithm_ReturnsInvalidTokenTypeError()
    {
        var error = JwtAlgorithmValidator.Validate(null);

        Assert.NotNull(error);
        Assert.Contains("type", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("RS256")]
    [InlineData("HS384")]
    [InlineData("HS512")]
    [InlineData("none")]
    [InlineData("")]
    public void Validate_WithUnsupportedAlgorithm_ReturnsInvalidAlgorithmError(string algorithm)
    {
        var error = JwtAlgorithmValidator.Validate(algorithm);

        Assert.NotNull(error);
        Assert.Contains("algorithm", error, StringComparison.OrdinalIgnoreCase);
    }
}
