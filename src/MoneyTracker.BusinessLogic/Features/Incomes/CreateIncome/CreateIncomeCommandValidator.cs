using FluentValidation;

namespace MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;

public class CreateIncomeCommandValidator : AbstractValidator<CreateIncomeCommand>
{
    public CreateIncomeCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(100)
            .WithMessage("Description must not exceed 100 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .PrecisionScale(18, 2, true)
            .WithMessage("Amount must have maximum 2 decimal places");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required");

        RuleFor(x => x.CreatedById)
            .NotEmpty()
            .WithMessage("CreatedBy is required");

        RuleFor(x => x.IdempotencyKey)
            .MaximumLength(256)
            .WithMessage("Idempotency key must not exceed 256 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.IdempotencyKey));
    }
}
