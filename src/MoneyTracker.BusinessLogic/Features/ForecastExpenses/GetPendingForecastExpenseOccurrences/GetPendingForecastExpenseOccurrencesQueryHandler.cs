using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;

public class GetPendingForecastExpenseOccurrencesQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetPendingForecastExpenseOccurrencesQuery, List<ForecastExpenseOccurrenceDto>>
{
    public async Task<List<ForecastExpenseOccurrenceDto>> Handle(GetPendingForecastExpenseOccurrencesQuery request, CancellationToken cancellationToken)
    {
        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        return await dbContext.ForecastOccurrences
            .AsNoTracking()
            .Where(x => !x.IsIncome
                && x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId
                && x.ExpectedDate >= monthStart
                && x.ExpectedDate < nextMonthStart)
            .OrderBy(x => x.ExpectedDate)
            .ThenBy(x => x.Description)
            .Select(x => new ForecastExpenseOccurrenceDto
            {
                Id = x.Id,
                ForecastDefinitionId = x.ForecastDefinitionId,
                Description = x.Description,
                Amount = x.Amount,
                ExpectedDate = x.ExpectedDate,
                PaymentCategoryId = x.PaymentCategoryId,
                Category = x.PaymentCategory != null ? x.PaymentCategory.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
