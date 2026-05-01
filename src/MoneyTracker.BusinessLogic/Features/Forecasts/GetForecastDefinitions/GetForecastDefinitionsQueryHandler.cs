using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitions;

public class GetForecastDefinitionsQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetForecastDefinitionsQuery, List<ForecastDefinitionRow>>
{
    public async Task<List<ForecastDefinitionRow>> Handle(GetForecastDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var expenses = await dbContext.ForecastExpenses
            .AsNoTracking()
            .Include(x => x.ForecastRecurrenceRuleType)
            .Include(x => x.PaymentCategory)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        var incomes = await dbContext.ForecastIncomes
            .AsNoTracking()
            .Include(x => x.ForecastRecurrenceRuleType)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        return expenses.Select(ToForecastDefinitionRow)
            .Concat(incomes.Select(ToForecastDefinitionRow))
            .OrderBy(x => x.RecurrenceStart)
            .ThenBy(x => x.Description)
            .ToList();
    }

    private static ForecastDefinitionRow ToForecastDefinitionRow(ForecastExpense forecast) => new()
    {
        Id = forecast.Id,
        ForecastRecurrenceRuleTypeId = forecast.ForecastRecurrenceRuleTypeId,
        Description = forecast.Description,
        Amount = forecast.Amount,
        RecurrenceStart = forecast.RecurrenceStart,
        RecurrenceEnd = forecast.RecurrenceEnd,
        Interval = forecast.Interval ?? 1,
        IsIncome = false,
        PaymentCategoryId = forecast.PaymentCategoryId,
        Category = forecast.PaymentCategory.Name
    };

    private static ForecastDefinitionRow ToForecastDefinitionRow(ForecastIncome income) => new()
    {
        Id = income.Id,
        ForecastRecurrenceRuleTypeId = income.ForecastRecurrenceRuleTypeId,
        Description = income.Description,
        Amount = income.Amount,
        RecurrenceStart = income.RecurrenceStart,
        RecurrenceEnd = income.RecurrenceEnd,
        Interval = income.Interval ?? 1,
        IsIncome = true
    };
}
