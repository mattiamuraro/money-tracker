using Microsoft.AspNetCore.Http;
using MoneyTracker.Api.ExtensionMethods;
using Xunit;

namespace MoneyTracker.Api.Tests.ExtensionMethods;

public class NoStoreResponseHeadersTests
{
    [Fact]
    public void Apply_Sets_CacheControl_NoStore()
    {
        var context = new DefaultHttpContext();

        NoStoreResponseHeaders.Apply(context.Response);

        Assert.Equal("no-store, no-cache, max-age=0", context.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public void Apply_Sets_Pragma_NoCache()
    {
        var context = new DefaultHttpContext();

        NoStoreResponseHeaders.Apply(context.Response);

        Assert.Equal("no-cache", context.Response.Headers.Pragma.ToString());
    }

    [Fact]
    public void Apply_Sets_Expires_Zero()
    {
        var context = new DefaultHttpContext();

        NoStoreResponseHeaders.Apply(context.Response);

        Assert.Equal("0", context.Response.Headers.Expires.ToString());
    }

    [Fact]
    public void Apply_IsIdempotent_WhenCalledTwice()
    {
        var context = new DefaultHttpContext();

        NoStoreResponseHeaders.Apply(context.Response);
        NoStoreResponseHeaders.Apply(context.Response);

        Assert.Equal("no-store, no-cache, max-age=0", context.Response.Headers.CacheControl.ToString());
        Assert.Equal("no-cache", context.Response.Headers.Pragma.ToString());
        Assert.Equal("0", context.Response.Headers.Expires.ToString());
    }
}
