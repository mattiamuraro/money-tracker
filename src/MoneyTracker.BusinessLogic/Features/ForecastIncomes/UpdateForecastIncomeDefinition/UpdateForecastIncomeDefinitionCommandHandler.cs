using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.Shared;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;

public class UpdateForecastIncomeDefinitionCommandHandler(
    IValidator<UpdateForecastIncomeDefinitionCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<UpdateForecastIncomeDefinitionCommand>
{
    public async Task Handle(UpdateForecastIncomeDefinitionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var forecastIncome = await dbContext.ForecastIncomes
            .Include(x => x.ForecastRecurrenceRuleType)
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.IsActive, cancellationToken);

        if (forecastIncome is null)
            throw new EntityNotFoundException($"No active forecast income found with ID '{command.Id}'.");

        forecastIncome.Description = command.Description;
        forecastIncome.Amount = command.Amount;
        forecastIncome.RecurrenceStart = command.RecurrenceStart;
        forecastIncome.RecurrenceEnd = command.RecurrenceEnd;
        forecastIncome.Interval = command.Interval;
        forecastIncome.ForecastRecurrenceRuleTypeId = command.ForecastRecurrenceRuleTypeId;

        var (startDate, endDate) = ForecastSynchronizationWindow.GetWindow();
        var expectedOccurrences = forecastIncome.GetRecurrences(startDate, endDate)
            .Select(date => new ForecastOccurrence
            {
                Id = Guid.CreateVersion7(),
                ForecastDefinitionId = forecastIncome.Id,
                IsIncome = true,
                Description = forecastIncome.Description,
                Amount = forecastIncome.Amount,
                ExpectedDate = date,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            })
            .ToList();

        await SynchronizeOccurrencesAsync(forecastIncome.Id, startDate, endDate, expectedOccurrences, cancellationToken);
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
