using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;

public class GetForecastDefinitionByIdQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetForecastDefinitionByIdQuery, ForecastDefinitionDto>
{
    public async Task<ForecastDefinitionDto> Handle(GetForecastDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await dbContext.ForecastExpenses
            .AsNoTracking()
            .Include(x => x.ForecastRecurrenceRuleType)
            .Include(x => x.PaymentCategory)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);
        if (expense != null)
            return ToDefinitionDto(expense);

        var income = await dbContext.ForecastIncomes
            .AsNoTracking()
            .Include(x => x.ForecastRecurrenceRuleType)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);
        if (income == null)
            throw new EntityNotFoundException($"Forecast with id {request.Id} not found.");

        return ToDefinitionDto(income);
    }

    private static ForecastDefinitionDto ToDefinitionDto(ForecastExpense forecast) => new()
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

    private static ForecastDefinitionDto ToDefinitionDto(ForecastIncome income) => new()
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
