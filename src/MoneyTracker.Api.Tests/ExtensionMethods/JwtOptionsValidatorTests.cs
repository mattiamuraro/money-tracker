using MoneyTracker.Api.ExtensionMethods;
using Xunit;

namespace MoneyTracker.Api.Tests.ExtensionMethods;

public class JwtOptionsValidatorTests
{
    // ── Key presence ─────────────────────────────────────────────────────────

    [Fact]
    public void ValidateKeyPresence_WithValidKey_ReturnsNull()
    {
        var error = JwtOptionsValidator.ValidateKeyPresence("some-secret-key");

        Assert.Null(error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateKeyPresence_WithMissingKey_ReturnsError(string? key)
    {
        var error = JwtOptionsValidator.ValidateKeyPresence(key);

        Assert.NotNull(error);
        Assert.Contains("Jwt:Key", error, StringComparison.Ordinal);
    }

    // ── Key length ───────────────────────────────────────────────────────────

    [Fact]
    public void ValidateKeyLength_WithKeyAtMinimumBytes_ReturnsNull()
    {
        // exactly 32 ASCII chars = 32 bytes
        var key = new string('a', JwtOptionsValidator.MinimumKeyBytes);

        var error = JwtOptionsValidator.ValidateKeyLength(key);

        Assert.Null(error);
    }

    [Fact]
    public void ValidateKeyLength_WithKeyAboveMinimumBytes_ReturnsNull()
    {
        var key = new string('x', JwtOptionsValidator.MinimumKeyBytes + 10);

        var error = JwtOptionsValidator.ValidateKeyLength(key);

        Assert.Null(error);
    }

    [Fact]
    public void ValidateKeyLength_WithKeyBelowMinimumBytes_ReturnsError()
    {
        var key = new string('a', JwtOptionsValidator.MinimumKeyBytes - 1); // 31 bytes

        var error = JwtOptionsValidator.ValidateKeyLength(key);

        Assert.NotNull(error);
        Assert.Contains("32 bytes", error, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateKeyLength_WithNullOrWhitespace_ReturnsNull_BecausePresenceCheckOwnsIt(string? key)
    {
        // Length validation defers to presence validation for missing keys
        var error = JwtOptionsValidator.ValidateKeyLength(key);

        Assert.Null(error);
    }

    // ── Required fields ──────────────────────────────────────────────────────

    [Fact]
    public void ValidateRequiredField_WithValue_ReturnsNull()
    {
        var error = JwtOptionsValidator.ValidateRequiredField("https://example.com", "Jwt:Issuer");

        Assert.Null(error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRequiredField_WithMissingValue_ReturnsErrorWithFieldName(string? value)
    {
        var error = JwtOptionsValidator.ValidateRequiredField(value, "Jwt:Issuer");

        Assert.NotNull(error);
        Assert.Contains("Jwt:Issuer", error, StringComparison.Ordinal);
    }

    // ── Expiry minutes ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(480)]
    public void ValidateExpiryMinutes_WithPositiveValue_ReturnsNull(int minutes)
    {
        var error = JwtOptionsValidator.ValidateExpiryMinutes(minutes);

        Assert.Null(error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ValidateExpiryMinutes_WithNonPositiveValue_ReturnsError(int minutes)
    {
        var error = JwtOptionsValidator.ValidateExpiryMinutes(minutes);

        Assert.NotNull(error);
        Assert.Contains("ExpiryMinutes", error, StringComparison.Ordinal);
    }
}
