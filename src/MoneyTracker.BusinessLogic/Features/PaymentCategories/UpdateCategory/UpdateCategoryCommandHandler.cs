using FluentValidation;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory
{
    /// <summary>
    /// Handler for updating an existing payment category
    /// </summary>
    public class UpdateCategoryCommandHandler
    {
        private readonly MoneyTrackerDbContext _dbContext;
        private readonly IValidator<UpdateCategoryCommand> _validator;

        public UpdateCategoryCommandHandler(IValidator<UpdateCategoryCommand> validator, MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
            var category = await _dbContext.PaymentCategories.FindAsync(
                new object[] { command.Id },
                cancellationToken: cancellationToken);

            if (category == null)
                throw new EntityNotFoundException($"Payment category with id {command.Id} not found");

            // Check if new code conflicts with existing categories
            var existingCategory = _dbContext.PaymentCategories.FirstOrDefault(c => c.Code == command.Code && c.Id != command.Id);
            if (existingCategory != null)
                throw new InvalidOperationException($"Category with code '{command.Code}' already exists.");

            category.Name = command.Name;
            category.Code = command.Code;

            _dbContext.PaymentCategories.Update(category);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
