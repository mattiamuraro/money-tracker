using FluentValidation;

namespace MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;

/// <summary>
/// Validator for UpdatePaymentCommand
/// </summary>
public class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");

        RuleFor(x => x.Description)
            .MaximumLength(100)
            .WithMessage("Description must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PaymentCategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("Payment category is required")
            .When(x => x.PaymentCategoryId.HasValue);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .PrecisionScale(18, 2, true)
            .WithMessage("Amount must have maximum 2 decimal places")
            .When(x => x.Amount.HasValue);
    }
}
