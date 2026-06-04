using FluentValidation;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory
{
    /// <summary>
    /// Validator for UpdateCategoryCommand
    /// </summary>
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category ID is required.");

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
