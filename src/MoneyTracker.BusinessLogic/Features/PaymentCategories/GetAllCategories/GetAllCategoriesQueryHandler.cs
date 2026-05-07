using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;

/// <summary>
/// Handler for retrieving all payment categories
/// </summary>
public class GetAllCategoriesQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetAllCategoriesQuery, IEnumerable<PaymentCategoryRow>>
{
    public async Task<IEnumerable<PaymentCategoryRow>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await dbContext.PaymentCategories
            .AsNoTracking()
            .Select(c => new PaymentCategoryRow
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return categories;
    }
}
