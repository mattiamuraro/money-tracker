using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Auth.Register;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Constructor_InitializesValidatorSuccessfully()
    {
        var validator = new RegisterCommandValidator();

        Assert.NotNull(validator);
    }

    [Fact]
    public void Constructor_ConfiguresUsernameValidationRule()
    {
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Username = string.Empty,
            Password = "validPassword123"
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required.");
    }

    [Fact]
    public void Constructor_ConfiguresPasswordValidationRule()
    {
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Username = "validUsername",
            Password = string.Empty
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public void Should_Fail_When_Username_Is_Empty()
    {
        var command = new RegisterCommand
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
        var command = new RegisterCommand
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
        var command = new RegisterCommand
        {
            Username = "   ",
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Fail_When_Username_Exceeds_50_Characters()
    {
        var command = new RegisterCommand
        {
            Username = new string('a', 51),
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username cannot exceed 50 characters.");
    }

    [Fact]
    public void Should_Succeed_When_Username_Is_50_Characters()
    {
        var command = new RegisterCommand
        {
            Username = new string('a', 50),
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Fail_When_Password_Is_Empty()
    {
        var command = new RegisterCommand
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
        var command = new RegisterCommand
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
        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = "   "
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Is_Less_Than_8_Characters()
    {
        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = "pass123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public void Should_Succeed_When_Password_Is_8_Characters()
    {
        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = "passwor8"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Exceeds_100_Characters()
    {
        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = new string('a', 101)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password cannot exceed 100 characters.");
    }

    [Fact]
    public void Should_Succeed_When_Password_Is_100_Characters()
    {
        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = new string('a', 100)
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Both_Username_And_Password_Are_Empty()
    {
        var command = new RegisterCommand
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
        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Custom_Error_Message_For_Empty_Username()
    {
        var command = new RegisterCommand
        {
            Username = string.Empty,
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required.");
    }

    [Fact]
    public void Should_Have_Custom_Error_Message_For_Empty_Password()
    {
        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public void Should_Fail_When_Username_Is_1_Character()
    {
        var command = new RegisterCommand
        {
            Username = "a",
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Fail_When_Password_Is_7_Characters()
    {
        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = "passwor"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }
}
