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
            .WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date cannot be in the future");

        RuleFor(x => x.PaymentCategoryId)
            .NotEmpty()
            .WithMessage("Payment category is required");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("CreatedBy is required")
            .MaximumLength(100)
            .WithMessage("CreatedBy must not exceed 100 characters");

        RuleFor(x => x.IsOneShot)
            .NotNull()
            .WithMessage("IsOneShot must be specified");
    }
}
