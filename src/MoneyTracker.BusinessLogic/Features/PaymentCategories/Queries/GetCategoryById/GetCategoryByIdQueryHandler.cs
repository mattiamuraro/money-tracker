using MediatR;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Models;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.Queries.GetCategoryById
{
    /// <summary>
    /// Handler for retrieving a specific payment category by ID
    /// </summary>
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, PaymentCategoryDto?>
    {
        private readonly MoneyTrackerDbContext _dbContext;

        public GetCategoryByIdQueryHandler(MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaymentCategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _dbContext.PaymentCategories.FindAsync(
                new object[] { request.Id },
                cancellationToken: cancellationToken);

            if (category == null)
                return null;

            return new PaymentCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                CreatedAt = category.CreatedAt,
                CreatedBy = category.CreatedBy,
                ModifiedAt = category.ModifiedAt,
                ModifiedBy = category.ModifiedBy
            };
        }
    }
}
