using MediatR;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Models;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.Queries.GetAllCategories
{
    /// <summary>
    /// Handler for retrieving all payment categories
    /// </summary>
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<PaymentCategoryDto>>
    {
        private readonly MoneyTrackerDbContext _dbContext;

        public GetAllCategoriesQueryHandler(MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<PaymentCategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = _dbContext.PaymentCategories.ToList();

            return categories.Select(c => new PaymentCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy,
                ModifiedAt = c.ModifiedAt,
                ModifiedBy = c.ModifiedBy
            });
        }
    }
}
