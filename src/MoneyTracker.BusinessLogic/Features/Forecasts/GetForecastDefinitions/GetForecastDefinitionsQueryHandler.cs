using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitions;

public class GetForecastDefinitionsQueryHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public GetForecastDefinitionsQueryHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ForecastDefinitionRow>> Handle(GetForecastDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var expenses = await _dbContext.ForecastExpenses
            .AsNoTracking()
            .Include(x => x.ForecastRecurrenceRuleType)
            .Include(x => x.PaymentCategory)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
        var incomes = await _dbContext.ForecastIncomes
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

    private static ForecastDefinitionRow ToForecastDefinitionRow(ForecastExpense forecast)
    {
        return new ForecastDefinitionRow
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
    }

    private static ForecastDefinitionRow ToForecastDefinitionRow(ForecastIncome income)
    {
        return new ForecastDefinitionRow
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
}
