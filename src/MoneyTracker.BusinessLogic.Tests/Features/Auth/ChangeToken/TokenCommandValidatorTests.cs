using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.ChangeToken;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenRefreshTokenIsProvided()
    {
        var command = new RefreshTokenCommand { RefreshToken = "valid-refresh-token" };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_WhenRefreshTokenIsEmptyOrWhitespace(string token)
    {
        var command = new RefreshTokenCommand { RefreshToken = token };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}

public class RevokeRefreshTokenCommandValidatorTests
{
    private readonly RevokeRefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenRefreshTokenIsProvided()
    {
        var command = new RevokeRefreshTokenCommand { RefreshToken = "valid-refresh-token" };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_WhenRefreshTokenIsEmptyOrWhitespace(string token)
    {
        var command = new RevokeRefreshTokenCommand { RefreshToken = token };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
