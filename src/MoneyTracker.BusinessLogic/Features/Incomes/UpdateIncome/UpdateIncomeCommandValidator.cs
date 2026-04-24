using FluentValidation;

namespace MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

public class UpdateIncomeCommandValidator : AbstractValidator<UpdateIncomeCommand>
{
    public UpdateIncomeCommandValidator()
    {
        RuleFor(x => x.IncomeId)
            .NotEmpty()
            .WithMessage("Income ID is required");

        RuleFor(x => x.Description)
            .MaximumLength(100)
            .WithMessage("Description must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .PrecisionScale(18, 2, true)
            .WithMessage("Amount must have maximum 2 decimal places")
            .When(x => x.Amount.HasValue);
    }
}
