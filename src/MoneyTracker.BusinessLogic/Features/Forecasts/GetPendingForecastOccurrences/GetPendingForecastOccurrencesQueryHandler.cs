using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetPendingForecastOccurrences;

public class GetPendingForecastOccurrencesQueryHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public GetPendingForecastOccurrencesQueryHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ForecastOccurrenceRow>> Handle(GetPendingForecastOccurrencesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.ForecastOccurrences
            .AsNoTracking()
            .Include(x => x.PaymentCategory)
            .Where(x => x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId
                && x.IsIncome == request.IsIncome
                && x.ExpectedDate.Year == request.Year
                && x.ExpectedDate.Month == request.Month)
            .OrderBy(x => x.ExpectedDate)
            .ThenBy(x => x.Description)
            .Select(x => new ForecastOccurrenceRow
            {
                Id = x.Id,
                ForecastDefinitionId = x.ForecastDefinitionId,
                Description = x.Description,
                Amount = x.Amount,
                ExpectedDate = x.ExpectedDate,
                IsIncome = x.IsIncome,
                PaymentCategoryId = x.PaymentCategoryId,
                Category = x.PaymentCategory != null ? x.PaymentCategory.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
