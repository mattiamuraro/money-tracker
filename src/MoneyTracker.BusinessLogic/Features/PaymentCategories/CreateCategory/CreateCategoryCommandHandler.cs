using FluentValidation;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory
{
    /// <summary>
    /// Handler for creating a new payment category
    /// </summary>
    public class CreateCategoryCommandHandler
    {
        private readonly MoneyTrackerDbContext _dbContext;
        private readonly IValidator<CreateCategoryCommand> _validator;

        public CreateCategoryCommandHandler(IValidator<CreateCategoryCommand> validator, MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            // Check if category with same code already exists
            var existingCategory = _dbContext.PaymentCategories.FirstOrDefault(c => c.Code == command.Code);
            if (existingCategory != null)
                throw new InvalidOperationException($"Category with code '{command.Code}' already exists.");

            var category = new PaymentCategory
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                Code = command.Code
            };

            _dbContext.PaymentCategories.Add(category);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return category.Id;
        }
    }
}
