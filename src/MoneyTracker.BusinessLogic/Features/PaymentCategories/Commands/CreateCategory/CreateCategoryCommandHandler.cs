using MediatR;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.CreateCategory
{
    /// <summary>
    /// Handler for creating a new payment category
    /// </summary>
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
    {
        private readonly MoneyTrackerDbContext _dbContext;

        public CreateCategoryCommandHandler(MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            // Check if category with same code already exists
            var existingCategory = _dbContext.PaymentCategories.FirstOrDefault(c => c.Code == request.Code);
            if (existingCategory != null)
                throw new InvalidOperationException($"Category with code '{request.Code}' already exists.");

            var category = new PaymentCategory
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy,
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = request.CreatedBy
            };

            _dbContext.PaymentCategories.Add(category);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return category.Id;
        }
    }
}
