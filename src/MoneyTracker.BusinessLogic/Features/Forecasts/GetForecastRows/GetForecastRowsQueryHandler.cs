using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;

/// <summary>
/// Handler for the GetForecastRowsQuery query
/// </summary>
public class GetForecastRowsQueryHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public GetForecastRowsQueryHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ForecastRow>> Handle(GetForecastRowsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.ForecastOccurrences
            .AsNoTracking()
            .Include(x => x.PaymentCategory)
            .Where(x => x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId
                && x.ExpectedDate >= request.StartDate
                && x.ExpectedDate <= request.EndDate)
            .OrderBy(x => x.ExpectedDate)
            .ThenBy(x => x.Description)
            .Select(x => new ForecastRow
            {
                Id = x.Id,
                ForecastDefinitionId = x.ForecastDefinitionId,
                Description = x.Description,
                Amount = x.Amount,
                Date = x.ExpectedDate,
                IsIncome = x.IsIncome,
                PaymentCategoryId = x.PaymentCategoryId,
                Category = x.PaymentCategory != null ? x.PaymentCategory.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
