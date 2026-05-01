using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;

/// <summary>
/// Handler for retrieving all payment categories
/// </summary>
public class GetAllCategoriesQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetAllCategoriesQuery, IEnumerable<PaymentCategoryRow>>
{
    public Task<IEnumerable<PaymentCategoryRow>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = dbContext.PaymentCategories.ToList();

        IEnumerable<PaymentCategoryRow> result = categories.Select(c => new PaymentCategoryRow
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            CreatedAt = c.CreatedAt
        });

        return Task.FromResult(result);
    }
}
