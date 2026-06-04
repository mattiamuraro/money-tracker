using System.Net;
using Microsoft.AspNetCore.Http;
using MoneyTracker.Api.ExtensionMethods;
using Xunit;

namespace MoneyTracker.Api.Tests.ExtensionMethods;

public class RateLimitPartitionResolverTests
{
    [Fact]
    public void Resolve_WithRemoteIpAddress_ReturnsIpString()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.42");

        var result = RateLimitPartitionResolver.Resolve(context);

        Assert.Equal("192.168.1.42", result);
    }

    [Fact]
    public void Resolve_WithIPv6Address_ReturnsIpString()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("::1");

        var result = RateLimitPartitionResolver.Resolve(context);

        Assert.Equal("::1", result);
    }

    [Fact]
    public void Resolve_WithNullRemoteIpAddress_ReturnsUnknown()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = null;

        var result = RateLimitPartitionResolver.Resolve(context);

        Assert.Equal("unknown", result);
    }

    [Fact]
    public void Resolve_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RateLimitPartitionResolver.Resolve(null!));
    }
}
