using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;

/// <summary>
/// Handler for retrieving a specific payment category by ID
/// </summary>
public class GetCategoryByIdQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetCategoryByIdQuery, PaymentCategoryDto>
{
    public async Task<PaymentCategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await dbContext.PaymentCategories.FindAsync(
            new object[] { request.Id }, cancellationToken: cancellationToken);

        if (category == null)
            throw new EntityNotFoundException($"Payment category with id {request.Id} not found");

        return new PaymentCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code
        };
    }
}
