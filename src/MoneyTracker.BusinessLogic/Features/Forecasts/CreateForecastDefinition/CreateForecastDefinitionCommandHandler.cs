using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;

public class CreateForecastDefinitionCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public CreateForecastDefinitionCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateForecastDefinitionCommand request, CancellationToken cancellationToken)
    {
        await ValidateForecastRequestAsync(request.IsIncome, request.PaymentCategoryId, request.RecurrenceStart, request.RecurrenceEnd, cancellationToken);

        var recurrenceRuleType = await GetRecurrenceRuleTypeAsync(request.ForecastRecurrenceRuleTypeId, cancellationToken);
        var forecast = CreateForecastEntity(request, recurrenceRuleType);

        if (request.IsIncome)
            _dbContext.ForecastIncomes.Add((ForecastIncome)forecast);
        else
            _dbContext.ForecastExpenses.Add((ForecastExpense)forecast);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return forecast.Id;
    }

    private async Task ValidateForecastRequestAsync(bool isIncome, Guid? paymentCategoryId, DateOnly recurrenceStart, DateOnly? recurrenceEnd, CancellationToken cancellationToken)
    {
        if (recurrenceEnd.HasValue && recurrenceEnd.Value < recurrenceStart)
            throw new InvalidOperationException("Recurrence end date cannot be earlier than recurrence start date.");

        if (isIncome)
            return;

        if (!paymentCategoryId.HasValue || paymentCategoryId == Guid.Empty)
            throw new InvalidOperationException("Expense forecasts require a payment category.");

        var categoryExists = await _dbContext.PaymentCategories.AnyAsync(x => x.Id == paymentCategoryId.Value, cancellationToken);
        if (!categoryExists)
            throw new InvalidOperationException("The requested payment category does not exist.");
    }

    private async Task<ForecastRecurrenceRuleType> GetRecurrenceRuleTypeAsync(Guid forecastRecurrenceRuleTypeId, CancellationToken cancellationToken)
    {
        var requestedId = forecastRecurrenceRuleTypeId;

        if (requestedId == Guid.Empty)
        {
            var dayRuleType = await _dbContext.ForecastRecurrenceRuleTypes
                .FirstOrDefaultAsync(x => x.Code == ForecastRecurrenceRuleType.Day, cancellationToken);

            return dayRuleType
                ?? throw new InvalidOperationException("Default recurrence rule type 'Day' was not found.");
        }

        var recurrenceRuleType = await _dbContext.ForecastRecurrenceRuleTypes
            .FirstOrDefaultAsync(x => x.Id == requestedId, cancellationToken);

        return recurrenceRuleType
            ?? throw new InvalidOperationException($"Forecast recurrence rule type '{requestedId}' was not found.");
    }

    private static BaseForecast CreateForecastEntity(CreateForecastDefinitionCommand request, ForecastRecurrenceRuleType recurrenceRuleType)
    {
        BaseForecast forecast = request.IsIncome
            ? new ForecastIncome
            {
                Description = request.Description
            }
            : new ForecastExpense
            {
                Description = request.Description,
                PaymentCategoryId = request.PaymentCategoryId ?? Guid.Empty
            };

        forecast.Id = Guid.NewGuid();
        forecast.Description = request.Description;
        forecast.Amount = request.Amount;
        forecast.RecurrenceStart = request.RecurrenceStart;
        forecast.RecurrenceEnd = request.RecurrenceEnd;
        forecast.Interval = request.Interval;
        forecast.ForecastRecurrenceRuleTypeId = recurrenceRuleType.Id;
        forecast.ForecastRecurrenceRuleType = recurrenceRuleType;
        forecast.IsActive = true;

        return forecast;
    }
}
