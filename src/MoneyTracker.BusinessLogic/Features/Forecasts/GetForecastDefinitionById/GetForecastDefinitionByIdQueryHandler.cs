using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;

public class GetForecastDefinitionByIdQueryHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public GetForecastDefinitionByIdQueryHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ForecastDefinitionDto?> Handle(GetForecastDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await _dbContext.ForecastExpenses
            .AsNoTracking()
            .Include(x => x.ForecastRecurrenceRuleType)
            .Include(x => x.PaymentCategory)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (expense != null)
            return ToDefinitionDto(expense);

        var income = await _dbContext.ForecastIncomes
            .AsNoTracking()
            .Include(x => x.ForecastRecurrenceRuleType)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        return income != null ? ToDefinitionDto(income) : null;
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
            PaymentCategoryId = forecast.PaymentCategoryId
        };
    }

    private static ForecastDefinitionDto ToDefinitionDto(ForecastIncome income)
    {
        return new ForecastDefinitionDto
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
