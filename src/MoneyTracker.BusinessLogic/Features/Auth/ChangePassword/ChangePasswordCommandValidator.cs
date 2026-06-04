using FluentValidation;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Options;

namespace MoneyTracker.BusinessLogic.Features.Auth.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator() : this(Microsoft.Extensions.Options.Options.Create(new PasswordPolicyOptions()))
    {
    }

    public ChangePasswordCommandValidator(IOptions<PasswordPolicyOptions> passwordPolicyOptions)
    {
        ArgumentNullException.ThrowIfNull(passwordPolicyOptions);
        var options = passwordPolicyOptions.Value;

        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty).WithMessage("User identifier is required.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(options.MinimumLength).WithMessage($"Password must be at least {options.MinimumLength} characters.")
            .MaximumLength(options.MaximumLength).WithMessage($"Password cannot exceed {options.MaximumLength} characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
            .Must(password => !options.BlockedPasswords.Contains(password, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Password is too common. Please choose a stronger password.");

        RuleFor(x => x)
            .Must(command => !string.Equals(command.CurrentPassword, command.NewPassword, StringComparison.Ordinal))
            .WithMessage("New password must be different from the current password.");
    }
}
