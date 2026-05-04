using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;

public class GetForecastExpenseRowsQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetForecastExpenseRowsQuery, List<ForecastExpenseRow>>
{
    public async Task<List<ForecastExpenseRow>> Handle(GetForecastExpenseRowsQuery query, CancellationToken cancellationToken)
    {
        if (query.EndDate < query.StartDate)
            throw new BadRequestException("End date must be greater than or equal to start date.");

        if ((query.EndDate.ToDateTime(TimeOnly.MinValue) - query.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays > 366)
            throw new BadRequestException("Date range cannot exceed 366 days.");

        return await dbContext.ForecastOccurrences
            .AsNoTracking()
            .Include(x => x.PaymentCategory)
            .Where(x => !x.IsIncome
                && x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId
                && x.ExpectedDate >= query.StartDate
                && x.ExpectedDate <= query.EndDate)
            .OrderBy(x => x.ExpectedDate)
            .ThenBy(x => x.Description)
            .Select(x => new ForecastExpenseRow
            {
                Id = x.Id,
                ForecastDefinitionId = x.ForecastDefinitionId,
                Description = x.Description,
                Amount = x.Amount,
                Date = x.ExpectedDate,
                PaymentCategoryId = x.PaymentCategoryId,
                Category = x.PaymentCategory != null ? x.PaymentCategory.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
