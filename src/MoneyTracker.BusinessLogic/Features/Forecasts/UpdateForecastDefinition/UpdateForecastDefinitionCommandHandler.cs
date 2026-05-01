using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition.ExtensionMethods;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition;

public class UpdateForecastDefinitionCommandHandler(
    MoneyTrackerDbContext dbContext,
    IValidator<UpdateForecastDefinitionCommand> validator)
    : IHandler<UpdateForecastDefinitionCommand>
{
    public async Task Handle(UpdateForecastDefinitionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (command.IsIncome)
            await UpdateForecastIncomeDefinitionAsync(command, cancellationToken);
        else
        {
            var categoryExists = await dbContext.PaymentCategories.AnyAsync(x => x.Id == command.PaymentCategoryId!.Value, cancellationToken);
            if (!categoryExists)
                throw new InvalidOperationException("The requested payment category does not exist.");
            await UpdateForecastExpenseDefinitionAsync(command, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateForecastExpenseDefinitionAsync(UpdateForecastDefinitionCommand command, CancellationToken cancellationToken)
    {
        var forecastExpense = await dbContext.ForecastExpenses
            .Include(x => x.ForecastRecurrenceRuleType)
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.IsActive, cancellationToken);
        if (forecastExpense is null)
            throw new EntityNotFoundException($"No active forecast expense found with ID '{command.Id}'.");

        forecastExpense.ApplyExpenseForecastDefinitionEdit(command);

        var (startDate, endDate) = ForecastOccurrencesHelper.GetSynchronizationWindow();
        var expectedOccurrences = forecastExpense.GetRecurrences(startDate, endDate)
            .Select(s => forecastExpense.ToNewForecastOccurrence(s));
        await SynchronizeAsync(expectedOccurrences, forecastExpense.Id, startDate, endDate, cancellationToken);
    }

    private async Task UpdateForecastIncomeDefinitionAsync(UpdateForecastDefinitionCommand command, CancellationToken cancellationToken)
    {
        var forecastIncome = await dbContext.ForecastIncomes
            .Include(x => x.ForecastRecurrenceRuleType)
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.IsActive, cancellationToken);
        if (forecastIncome is null)
            throw new EntityNotFoundException($"No active forecast income found with ID '{command.Id}'.");

        forecastIncome.ApplyIncomeForecastDefinitionEdit(command);

        var (startDate, endDate) = ForecastOccurrencesHelper.GetSynchronizationWindow();
        var expectedOccurrences = forecastIncome.GetRecurrences(startDate, endDate)
            .Select(s => forecastIncome.ToNewForecastOccurrence(s));
        await SynchronizeAsync(expectedOccurrences, forecastIncome.Id, startDate, endDate, cancellationToken);
    }

    private async Task SynchronizeAsync(IEnumerable<ForecastOccurrence> expectedOccurrences, Guid forecastDefinitionId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var existingOccurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ExpectedDate >= startDate && x.ExpectedDate <= endDate && x.ForecastDefinitionId == forecastDefinitionId)
            .ToListAsync(cancellationToken);

        UpdateExistingOccurrences(existingOccurrences, expectedOccurrences);
        CreateNewOccurrences(existingOccurrences, expectedOccurrences);
    }

    private void UpdateExistingOccurrences(List<ForecastOccurrence> existingOccurrences, IEnumerable<ForecastOccurrence> expectedOccurrences)
    {
        foreach (var existingOccurrence in existingOccurrences)
        {
            var occurrence = expectedOccurrences.FirstOrDefault(x => x.ExpectedDate == existingOccurrence.ExpectedDate);
            if (occurrence is null)
            {
                if (existingOccurrence.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId)
                {
                    existingOccurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId;
                    existingOccurrence.ValidatedAt = null;
                }
            }
            else if (existingOccurrence.ForecastOccurrenceStatusId is var statusId
                && (statusId == ForecastOccurrenceStatus.PendingId || statusId == ForecastOccurrenceStatus.CancelledId))
            {
                existingOccurrence.Description = occurrence.Description;
                existingOccurrence.Amount = occurrence.Amount;
                existingOccurrence.PaymentCategoryId = occurrence.PaymentCategoryId;
                existingOccurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId;
                existingOccurrence.ValidatedAt = null;
            }
        }
    }

    private void CreateNewOccurrences(List<ForecastOccurrence> existingOccurrences, IEnumerable<ForecastOccurrence> expectedOccurrences)
    {
        foreach (var expectedOccurrence in expectedOccurrences)
        {
            if (!existingOccurrences.Any(x => x.ForecastDefinitionId == expectedOccurrence.ForecastDefinitionId
                && x.ExpectedDate == expectedOccurrence.ExpectedDate))
                dbContext.ForecastOccurrences.Add(expectedOccurrence);
        }
    }
}
