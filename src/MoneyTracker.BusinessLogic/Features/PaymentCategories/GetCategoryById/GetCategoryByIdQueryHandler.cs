using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.Data.EntityFramework;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById
{
    /// <summary>
    /// Handler for retrieving a specific payment category by ID
    /// </summary>
    public class GetCategoryByIdQueryHandler
    {
        private readonly MoneyTrackerDbContext _dbContext;

        public GetCategoryByIdQueryHandler(MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaymentCategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _dbContext.PaymentCategories.FindAsync(
                new object[] { request.Id },
                cancellationToken: cancellationToken);

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
}
