using FluentValidation;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition
{
    internal class UpdateForecastDefinitionCommandValidator : AbstractValidator<UpdateForecastDefinitionCommand>
    {
        public UpdateForecastDefinitionCommandValidator()
        {
            RuleFor(x => x.RecurrenceEnd)
                .GreaterThan(x => x.RecurrenceStart)
                .When(x => x.RecurrenceEnd.HasValue)
                .WithMessage("Recurrence end date cannot be earlier than recurrence start date.");

            RuleFor(x => x.PaymentCategoryId)
                .NotEmpty()
                .NotEqual(Guid.Empty)
                .WithMessage("Expense forecasts require a payment category.")
                .When(x => !x.IsIncome);
        }
    }
}
