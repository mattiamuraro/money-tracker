using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.Shared;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;

public class CreateForecastExpenseDefinitionCommandHandler(
    IValidator<CreateForecastExpenseDefinitionCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<CreateForecastExpenseDefinitionCommand, Guid>
{
    public async Task<Guid> Handle(CreateForecastExpenseDefinitionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var categoryExists = await dbContext.PaymentCategories.AnyAsync(x => x.Id == command.PaymentCategoryId, cancellationToken);
        if (!categoryExists)
            throw new EntityNotFoundException("The requested payment category does not exist.");

        var forecastExpense = new ForecastExpense
        {
            Id = Guid.CreateVersion7(),
            PaymentCategoryId = command.PaymentCategoryId,
            Description = command.Description,
            Amount = command.Amount,
            RecurrenceStart = command.RecurrenceStart,
            RecurrenceEnd = command.RecurrenceEnd,
            Interval = command.Interval,
            ForecastRecurrenceRuleTypeId = command.ForecastRecurrenceRuleTypeId,
            IsActive = true
        };

        dbContext.ForecastExpenses.Add(forecastExpense);
        await GenerateOccurrencesAsync(forecastExpense, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return forecastExpense.Id;
    }

    private async Task GenerateOccurrencesAsync(ForecastExpense forecastExpense, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = ForecastSynchronizationWindow.GetWindow();

        var ruleType = await dbContext.ForecastRecurrenceRuleTypes
            .FirstOrDefaultAsync(f => f.Id == forecastExpense.ForecastRecurrenceRuleTypeId, cancellationToken);
        if (ruleType is not null)
            forecastExpense.ForecastRecurrenceRuleType = ruleType;

        var occurrences = forecastExpense.GetRecurrences(startDate, endDate)
            .Select(date => new ForecastOccurrence
            {
                Id = Guid.CreateVersion7(),
                ForecastDefinitionId = forecastExpense.Id,
                IsIncome = false,
                Description = forecastExpense.Description,
                Amount = forecastExpense.Amount,
                ExpectedDate = date,
                PaymentCategoryId = forecastExpense.PaymentCategoryId,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            });

        dbContext.ForecastOccurrences.AddRange(occurrences);
    }
}
