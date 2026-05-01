using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;

/// <summary>
/// Handler for deleting a payment category
/// </summary>
public class DeleteCategoryCommandHandler(MoneyTrackerDbContext dbContext)
    : IHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await dbContext.PaymentCategories
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
            throw new EntityNotFoundException($"Payment category with id {request.Id} not found");

        if (category.Payments.Any())
            throw new InvalidOperationException("Cannot delete a category that has associated payments.");

        dbContext.PaymentCategories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
