using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using System.Collections;

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

    public async Task<List<ForecastRow>> Handle(GetForecastRowsQuery query, CancellationToken cancellationToken)
    {

        if (query.EndDate < query.StartDate)
            throw new BadRequestException("End date must be greater than or equal to start date.");

        if ((query.EndDate.ToDateTime(TimeOnly.MinValue) - query.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays > 366)
            throw new BadRequestException("Date range cannot exceed 366 days.");

        
        return await _dbContext.ForecastOccurrences
            .AsNoTracking()
            .Include(x => x.PaymentCategory)
            .Where(x => x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId
                && x.ExpectedDate >= query.StartDate
                && x.ExpectedDate <= query.EndDate)
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
