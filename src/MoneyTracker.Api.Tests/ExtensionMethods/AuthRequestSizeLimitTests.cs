using Microsoft.AspNetCore.Http;
using MoneyTracker.Api.ExtensionMethods;
using Xunit;

namespace MoneyTracker.Api.Tests.ExtensionMethods;

public class AuthRequestSizeLimitTests
{
    [Fact]
    public void CheckLimit_WithNullContentLength_ReturnsNull()
    {
        var result = AuthRequestSizeLimit.CheckLimit(null);

        Assert.Null(result);
    }

    [Fact]
    public void CheckLimit_WithContentLengthBelowLimit_ReturnsNull()
    {
        var result = AuthRequestSizeLimit.CheckLimit(AuthRequestSizeLimit.LimitBytes - 1);

        Assert.Null(result);
    }

    [Fact]
    public void CheckLimit_WithContentLengthEqualToLimit_ReturnsNull()
    {
        var result = AuthRequestSizeLimit.CheckLimit(AuthRequestSizeLimit.LimitBytes);

        Assert.Null(result);
    }

    [Fact]
    public void CheckLimit_WithContentLengthAboveLimit_ReturnsProblemResult()
    {
        var result = AuthRequestSizeLimit.CheckLimit(AuthRequestSizeLimit.LimitBytes + 1);

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(4097)]
    [InlineData(1_000_000)]
    [InlineData(long.MaxValue)]
    public void CheckLimit_WithVariousOversizedLengths_ReturnsProblemResult(long contentLength)
    {
        var result = AuthRequestSizeLimit.CheckLimit(contentLength);

        Assert.NotNull(result);
    }
}
