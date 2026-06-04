using FluentValidation;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Options;

namespace MoneyTracker.BusinessLogic.Features.Auth.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator() : this(Microsoft.Extensions.Options.Options.Create(new PasswordPolicyOptions()))
    {
    }

    public RegisterCommandValidator(IOptions<PasswordPolicyOptions> passwordPolicyOptions)
    {
        ArgumentNullException.ThrowIfNull(passwordPolicyOptions);
        var options = passwordPolicyOptions.Value;

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(options.MinimumLength).WithMessage($"Password must be at least {options.MinimumLength} characters.")
            .MaximumLength(options.MaximumLength).WithMessage($"Password cannot exceed {options.MaximumLength} characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
            .Must(password => !options.BlockedPasswords.Contains(password, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Password is too common. Please choose a stronger password.");
    }
}
