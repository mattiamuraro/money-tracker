using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.Shared;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;

public class UpdateForecastExpenseDefinitionCommandHandler(
    IValidator<UpdateForecastExpenseDefinitionCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<UpdateForecastExpenseDefinitionCommand>
{
    public async Task Handle(UpdateForecastExpenseDefinitionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var forecastExpense = await dbContext.ForecastExpenses
            .Include(x => x.ForecastRecurrenceRuleType)
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.IsActive, cancellationToken);

        if (forecastExpense is null)
            throw new EntityNotFoundException($"No active forecast expense found with ID '{command.Id}'.");

        var categoryExists = await dbContext.PaymentCategories.AnyAsync(x => x.Id == command.PaymentCategoryId, cancellationToken);
        if (!categoryExists)
            throw new EntityNotFoundException("The requested payment category does not exist.");

        forecastExpense.Description = command.Description;
        forecastExpense.Amount = command.Amount;
        forecastExpense.RecurrenceStart = command.RecurrenceStart;
        forecastExpense.RecurrenceEnd = command.RecurrenceEnd;
        forecastExpense.Interval = command.Interval;
        forecastExpense.ForecastRecurrenceRuleTypeId = command.ForecastRecurrenceRuleTypeId;
        forecastExpense.PaymentCategoryId = command.PaymentCategoryId;

        var (startDate, endDate) = ForecastSynchronizationWindow.GetWindow();
        var expectedOccurrences = forecastExpense.GetRecurrences(startDate, endDate)
            .Select(date => new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = forecastExpense.Id,
                IsIncome = false,
                Description = forecastExpense.Description,
                Amount = forecastExpense.Amount,
                ExpectedDate = date,
                PaymentCategoryId = forecastExpense.PaymentCategoryId,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            })
            .ToList();

        await SynchronizeOccurrencesAsync(forecastExpense.Id, startDate, endDate, expectedOccurrences, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SynchronizeOccurrencesAsync(
        Guid definitionId,
        DateOnly startDate,
        DateOnly endDate,
        List<ForecastOccurrence> expectedOccurrences,
        CancellationToken cancellationToken)
    {
        var existingOccurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == definitionId
                && x.ExpectedDate >= startDate
                && x.ExpectedDate <= endDate)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingOccurrences)
        {
            var expected = expectedOccurrences.FirstOrDefault(x => x.ExpectedDate == existing.ExpectedDate);
            if (expected is null)
            {
                if (existing.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId)
                {
                    existing.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId;
                    existing.ValidatedAt = null;
                }
            }
            else if (existing.ForecastOccurrenceStatusId is var statusId
                && (statusId == ForecastOccurrenceStatus.PendingId || statusId == ForecastOccurrenceStatus.CancelledId))
            {
                existing.Description = expected.Description;
                existing.Amount = expected.Amount;
                existing.PaymentCategoryId = expected.PaymentCategoryId;
                existing.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId;
                existing.ValidatedAt = null;
            }
        }

        foreach (var expected in expectedOccurrences)
        {
            if (!existingOccurrences.Any(x => x.ForecastDefinitionId == expected.ForecastDefinitionId
                && x.ExpectedDate == expected.ExpectedDate))
                dbContext.ForecastOccurrences.Add(expected);
        }
    }
}
