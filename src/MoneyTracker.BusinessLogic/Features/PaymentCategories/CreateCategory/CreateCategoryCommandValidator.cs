using FluentValidation;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory
{
    /// <summary>
    /// Validator for CreateCategoryCommand
    /// </summary>
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(50).WithMessage("Category name cannot exceed 50 characters.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Category code is required.")
                .MaximumLength(5).WithMessage("Category code cannot exceed 5 characters.")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Category code must contain only uppercase letters and numbers.");
        }
    }
}
