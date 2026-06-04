using FluentValidation;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;

public class UpdateForecastIncomeDefinitionCommandValidator : AbstractValidator<UpdateForecastIncomeDefinitionCommand>
{
    public UpdateForecastIncomeDefinitionCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Description must not exceed 100 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .PrecisionScale(18, 2, true)
            .WithMessage("Amount must have maximum 2 decimal places");

        RuleFor(x => x.Interval)
            .GreaterThan(0)
            .WithMessage("Interval must be greater than 0");

        RuleFor(x => x.RecurrenceEnd)
            .GreaterThanOrEqualTo(x => x.RecurrenceStart)
            .WithMessage("Recurrence end date cannot be earlier than recurrence start date.")
            .When(x => x.RecurrenceEnd.HasValue);
    }
}
