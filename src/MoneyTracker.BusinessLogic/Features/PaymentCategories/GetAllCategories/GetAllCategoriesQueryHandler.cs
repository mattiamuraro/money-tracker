using MoneyTracker.BusinessLogic.Features.PaymentCategories.Models;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories
{
    /// <summary>
    /// Handler for retrieving all payment categories
    /// </summary>
    public class GetAllCategoriesQueryHandler
    {
        private readonly MoneyTrackerDbContext _dbContext;

        public GetAllCategoriesQueryHandler(MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IEnumerable<PaymentCategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = _dbContext.PaymentCategories.ToList();

            IEnumerable<PaymentCategoryDto> result = categories.Select(c => new PaymentCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                CreatedAt = c.CreatedAt,
                CreatedById = c.CreatedById,
                ModifiedAt = c.ModifiedAt,
                ModifiedById = c.ModifiedById
            });

            return Task.FromResult(result);
        }
    }
}
