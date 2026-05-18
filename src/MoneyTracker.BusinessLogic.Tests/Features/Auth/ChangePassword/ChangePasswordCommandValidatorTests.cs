using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Features.Auth.ChangePassword;
using MoneyTracker.BusinessLogic.Features.Auth.Options;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.ChangePassword;

public class ChangePasswordCommandValidatorTests
{
    private static ChangePasswordCommandValidator CreateValidator() =>
        new(Options.Create(new PasswordPolicyOptions
        {
            MinimumLength = 12,
            MaximumLength = 128,
            PasswordHistoryCount = 5,
            BlockedPasswords = ["password123"]
        }));

    [Fact]
    public void Validate_WithBlockedPassword_ReturnsError()
    {
        var validator = CreateValidator();
        var command = new ChangePasswordCommand
        {
            UserId = Guid.NewGuid(),
            CurrentPassword = "Current#Password1",
            NewPassword = "password123"
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password is too common. Please choose a stronger password.");
    }

    [Fact]
    public void Validate_WithValidPassword_ReturnsNoErrors()
    {
        var validator = CreateValidator();
        var command = new ChangePasswordCommand
        {
            UserId = Guid.NewGuid(),
            CurrentPassword = "Current#Password1",
            NewPassword = "Stronger#Password2"
        };

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
