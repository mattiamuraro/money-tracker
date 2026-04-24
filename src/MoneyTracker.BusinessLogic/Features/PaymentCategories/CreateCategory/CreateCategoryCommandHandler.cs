using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
            await _validator.ValidateAndThrowAsync(command, cancellationToken);
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
