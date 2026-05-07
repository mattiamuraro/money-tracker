using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;

public class GetForecastExpenseDefinitionsQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetForecastExpenseDefinitionsQuery, List<ForecastExpenseDefinitionDto>>
{
    public async Task<List<ForecastExpenseDefinitionDto>> Handle(GetForecastExpenseDefinitionsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.ForecastExpenses
            .AsNoTracking()
            .Include(x => x.PaymentCategory)
            .Where(x => x.IsActive)
            .OrderBy(x => x.RecurrenceStart)
            .ThenBy(x => x.Description)
            .Select(x => new ForecastExpenseDefinitionDto
            {
                Id = x.Id,
                ForecastRecurrenceRuleTypeId = x.ForecastRecurrenceRuleTypeId,
                Description = x.Description,
                Amount = x.Amount,
                RecurrenceStart = x.RecurrenceStart,
                RecurrenceEnd = x.RecurrenceEnd,
                Interval = x.Interval ?? 1,
                PaymentCategoryId = x.PaymentCategoryId,
                Category = x.PaymentCategory.Name
            })
            .ToListAsync(cancellationToken);
    }
}
