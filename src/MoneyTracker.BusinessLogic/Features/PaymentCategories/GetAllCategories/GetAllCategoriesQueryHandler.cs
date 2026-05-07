using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;

/// <summary>
/// Handler for retrieving all payment categories
/// </summary>
public class GetAllCategoriesQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetAllCategoriesQuery, IEnumerable<PaymentCategoryDto>>
{
    public async Task<IEnumerable<PaymentCategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await dbContext.PaymentCategories
            .AsNoTracking()
            .Select(c => new PaymentCategoryDto
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
