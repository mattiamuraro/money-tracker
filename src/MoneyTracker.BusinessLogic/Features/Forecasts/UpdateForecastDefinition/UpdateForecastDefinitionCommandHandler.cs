using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition;

public class UpdateForecastDefinitionCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public UpdateForecastDefinitionCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateForecastDefinitionCommand request, CancellationToken cancellationToken)
    {
        await ValidateForecastRequestAsync(request.IsIncome, request.PaymentCategoryId, request.RecurrenceStart, request.RecurrenceEnd, cancellationToken);

        var recurrenceRuleType = await GetRecurrenceRuleTypeAsync(request.ForecastRecurrenceRuleTypeId, cancellationToken);

        var expense = await _dbContext.ForecastExpenses
            .Include(x => x.ForecastRecurrenceRuleType)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (expense != null)
        {
            await UpdateForecastAsync(request.Id, expense, request, recurrenceRuleType, false, cancellationToken);
            return true;
        }

        var income = await _dbContext.ForecastIncomes
            .Include(x => x.ForecastRecurrenceRuleType)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (income != null)
        {
            await UpdateForecastAsync(request.Id, income, request, recurrenceRuleType, true, cancellationToken);
            return true;
        }

        return false;
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

    private async Task UpdateForecastAsync(Guid id, BaseForecast existingForecast, UpdateForecastDefinitionCommand request, ForecastRecurrenceRuleType recurrenceRuleType, bool isIncome, CancellationToken cancellationToken)
    {
        if (isIncome == request.IsIncome)
        {
            ApplyForecastValues(existingForecast, request, recurrenceRuleType);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        existingForecast.IsActive = false;

        BaseForecast replacement = request.IsIncome
            ? new ForecastIncome
            {
                Description = existingForecast.Description
            }
            : new ForecastExpense
            {
                Description = existingForecast.Description
            };

        replacement.Id = id;
        replacement.CreatedAt = existingForecast.CreatedAt;
        replacement.CreatedById = existingForecast.CreatedById;
        replacement.IsActive = true;

        ApplyForecastValues(replacement, request, recurrenceRuleType);

        if (isIncome)
        {
            _dbContext.ForecastExpenses.Add((ForecastExpense)replacement);
        }
        else
        {
            _dbContext.ForecastIncomes.Add((ForecastIncome)replacement);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyForecastValues(BaseForecast forecast, UpdateForecastDefinitionCommand request, ForecastRecurrenceRuleType recurrenceRuleType)
    {
        forecast.Description = request.Description;
        forecast.Amount = request.Amount;
        forecast.RecurrenceStart = request.RecurrenceStart;
        forecast.RecurrenceEnd = request.RecurrenceEnd;
        forecast.Interval = request.Interval;
        forecast.ForecastRecurrenceRuleTypeId = recurrenceRuleType.Id;
        forecast.ForecastRecurrenceRuleType = recurrenceRuleType;
        forecast.IsActive = true;

        if (forecast is ForecastExpense expense)
            expense.PaymentCategoryId = request.PaymentCategoryId ?? Guid.Empty;
    }
}
