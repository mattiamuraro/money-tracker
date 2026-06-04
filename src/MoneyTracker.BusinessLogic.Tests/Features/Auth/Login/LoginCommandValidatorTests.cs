using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Auth.Login;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_Username_Is_Empty()
    {
        var command = new LoginCommand
        {
            Username = string.Empty,
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Fail_When_Username_Is_Null()
    {
        var command = new LoginCommand
        {
            Username = null!,
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Fail_When_Username_Is_Whitespace()
    {
        var command = new LoginCommand
        {
            Username = "   ",
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Fail_When_Password_Is_Empty()
    {
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Is_Null()
    {
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = null!
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Is_Whitespace()
    {
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "   "
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Both_Username_And_Password_Are_Empty()
    {
        var command = new LoginCommand
        {
            Username = string.Empty,
            Password = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Succeed_When_Username_And_Password_Are_Valid()
    {
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Custom_Error_Message_For_Username()
    {
        var command = new LoginCommand
        {
            Username = string.Empty,
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required.");
    }

    [Fact]
    public void Should_Have_Custom_Error_Message_For_Password()
    {
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public void Constructor_InitializesValidatorSuccessfully()
    {
        var validator = new LoginCommandValidator();

        Assert.NotNull(validator);
    }

    [Fact]
    public void Constructor_ConfiguresUsernameValidationRule()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand
        {
            Username = string.Empty,
            Password = "validPassword"
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required.");
    }

    [Fact]
    public void Constructor_ConfiguresPasswordValidationRule()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand
        {
            Username = "validUsername",
            Password = string.Empty
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }
}
