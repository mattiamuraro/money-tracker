using FluentValidation;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.CreatePayment;

namespace MoneyTracker.BusinessLogic.Features.Payments.Validators;

/// <summary>
/// Validator for CreatePaymentCommand
/// </summary>
public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(100)
            .WithMessage("Description must not exceed 100 characters");

        RuleFor(x => x.Amount)
            .NotEmpty()
            .WithMessage("Amount is required")
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .PrecisionScale(18, 2, true)
            .WithMessage("Amount must have maximum 2 decimal places");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required");

        RuleFor(x => x.PaymentCategoryId)
            .NotEmpty()
            .WithMessage("Payment category is required");

        RuleFor(x => x .CreatedById)
            .NotEmpty()
            .WithMessage("CreatedBy is required");

        RuleFor(x => x.IsOneShot)
            .NotNull()
            .WithMessage("IsOneShot must be specified");

        RuleFor(x => x.IdempotencyKey)
            .MaximumLength(256)
            .WithMessage("Idempotency key must not exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.IdempotencyKey));
    }
}
