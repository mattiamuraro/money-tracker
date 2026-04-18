using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Forecasts.Models;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitions;

public class GetForecastDefinitionsQueryHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public GetForecastDefinitionsQueryHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ForecastDefinitionDto>> Handle(GetForecastDefinitionsQuery request, CancellationToken cancellationToken)
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

        return expenses.Select(ToDefinitionDto)
            .Concat(incomes.Select(x => ToDefinitionDto(x, true)))
            .OrderBy(x => x.RecurrenceStart)
            .ThenBy(x => x.Description)
            .ToList();
    }

    private static ForecastDefinitionDto ToDefinitionDto(ForecastExpense forecast)
    {
        return new ForecastDefinitionDto
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

    private static ForecastDefinitionDto ToDefinitionDto(BaseForecast forecast, bool isIncome)
    {
        return new ForecastDefinitionDto
        {
            Id = forecast.Id,
            ForecastRecurrenceRuleTypeId = forecast.ForecastRecurrenceRuleTypeId,
            Description = forecast.Description,
            Amount = forecast.Amount,
            RecurrenceStart = forecast.RecurrenceStart,
            RecurrenceEnd = forecast.RecurrenceEnd,
            Interval = forecast.Interval ?? 1,
            IsIncome = isIncome
        };
    }
}
