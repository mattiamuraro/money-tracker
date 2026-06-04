using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.ExtensionMethods;

public class HelperTests
{
    [Fact]
    public void BuildToken_ShouldCreateJwtWithExpectedClaims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "john",
            PasswordHash = "hash"
        };

        var options = new JwtOptions
        {
            Key = "0123456789abcdef0123456789abcdef",
            Issuer = "issuer-test",
            Audience = "audience-test",
            ExpiryMinutes = 30
        };

        var helperType = typeof(MoneyTracker.BusinessLogic.Common.Extensions.BusinessLogicServiceCollectionExtensions)
            .Assembly
            .GetType("MoneyTracker.BusinessLogic.Features.Auth.ExtensionMethods.Helper");
        Assert.NotNull(helperType);

        var method = helperType!.GetMethod("BuildToken", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        Assert.NotNull(method);

        var tokenString = (string)method!.Invoke(null, [user, options])!;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

        Assert.Equal("issuer-test", jwt.Issuer);
        Assert.Equal("audience-test", jwt.Audiences.Single());
        Assert.Equal("john", jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }
}
