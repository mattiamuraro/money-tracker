using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory
{
    /// <summary>
    /// Handler for deleting a payment category
    /// </summary>
    public class DeleteCategoryCommandHandler
    {
        private readonly MoneyTrackerDbContext _dbContext;

        public DeleteCategoryCommandHandler(MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _dbContext.PaymentCategories
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (category == null)
                throw new EntityNotFoundException($"Payment category with id {request.Id} not found");

            // Check if category has associated payments
            if (category.Payments.Any())
                throw new InvalidOperationException("Cannot delete a category that has associated payments.");

            _dbContext.PaymentCategories.Remove(category);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
