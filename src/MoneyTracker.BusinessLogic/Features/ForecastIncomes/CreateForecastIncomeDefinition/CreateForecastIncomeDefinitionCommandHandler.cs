using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.Shared;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;

public class CreateForecastIncomeDefinitionCommandHandler(
    IValidator<CreateForecastIncomeDefinitionCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<CreateForecastIncomeDefinitionCommand, Guid>
{
    public async Task<Guid> Handle(CreateForecastIncomeDefinitionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var forecastIncome = new ForecastIncome
        {
            Id = Guid.NewGuid(),
            Description = command.Description,
            Amount = command.Amount,
            RecurrenceStart = command.RecurrenceStart,
            RecurrenceEnd = command.RecurrenceEnd,
            Interval = command.Interval,
            ForecastRecurrenceRuleTypeId = command.ForecastRecurrenceRuleTypeId,
            IsActive = true
        };

        await dbContext.ForecastIncomes.AddAsync(forecastIncome, cancellationToken);
        await GenerateOccurrencesAsync(forecastIncome, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return forecastIncome.Id;
    }

    private async Task GenerateOccurrencesAsync(ForecastIncome forecastIncome, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = ForecastSynchronizationWindow.GetWindow();

        var ruleType = await dbContext.ForecastRecurrenceRuleTypes
            .FirstOrDefaultAsync(f => f.Id == forecastIncome.ForecastRecurrenceRuleTypeId, cancellationToken);
        if (ruleType is not null)
            forecastIncome.ForecastRecurrenceRuleType = ruleType;

        var occurrences = forecastIncome.GetRecurrences(startDate, endDate)
            .Select(date => new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = forecastIncome.Id,
                IsIncome = true,
                Description = forecastIncome.Description,
                Amount = forecastIncome.Amount,
                ExpectedDate = date,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            });

        dbContext.ForecastOccurrences.AddRange(occurrences);
    }
}
