using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;

public class GetPendingForecastIncomeOccurrencesQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetPendingForecastIncomeOccurrencesQuery, List<ForecastIncomeOccurrenceDto>>
{
    public async Task<List<ForecastIncomeOccurrenceDto>> Handle(GetPendingForecastIncomeOccurrencesQuery request, CancellationToken cancellationToken)
    {
        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        return await dbContext.ForecastOccurrences
            .AsNoTracking()
            .Where(x => x.IsIncome
                && x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId
                && x.ExpectedDate >= monthStart
                && x.ExpectedDate < nextMonthStart)
            .OrderBy(x => x.ExpectedDate)
            .ThenBy(x => x.Description)
            .Select(x => new ForecastIncomeOccurrenceDto
            {
                Id = x.Id,
                ForecastDefinitionId = x.ForecastDefinitionId,
                Description = x.Description,
                Amount = x.Amount,
                ExpectedDate = x.ExpectedDate
            })
            .ToListAsync(cancellationToken);
    }
}
