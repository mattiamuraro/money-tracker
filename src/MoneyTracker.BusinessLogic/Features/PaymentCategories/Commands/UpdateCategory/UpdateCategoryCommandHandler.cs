using MediatR;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.UpdateCategory
{
    /// <summary>
    /// Handler for updating an existing payment category
    /// </summary>
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
    {
        private readonly MoneyTrackerDbContext _dbContext;

        public UpdateCategoryCommandHandler(MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _dbContext.PaymentCategories.FindAsync(
                new object[] { request.Id },
                cancellationToken: cancellationToken);

            if (category == null)
                return false;

            // Check if new code conflicts with existing categories
            var existingCategory = _dbContext.PaymentCategories.FirstOrDefault(c => c.Code == request.Code && c.Id != request.Id);
            if (existingCategory != null)
                throw new InvalidOperationException($"Category with code '{request.Code}' already exists.");

            category.Name = request.Name;
            category.Code = request.Code;
            category.ModifiedAt = DateTime.UtcNow;
            category.ModifiedBy = request.ModifiedBy;

            _dbContext.PaymentCategories.Update(category);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
