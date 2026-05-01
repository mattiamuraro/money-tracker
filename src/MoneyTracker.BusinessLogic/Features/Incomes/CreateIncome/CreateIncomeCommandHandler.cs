using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;

public class CreateIncomeCommandHandler(
    IValidator<CreateIncomeCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<CreateIncomeCommand, Guid>
{
    public async Task<Guid> Handle(CreateIncomeCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            var existing = await dbContext.Incomes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == command.IdempotencyKey, cancellationToken);
            if (existing is not null)
                return existing.Id;
        }

        var occurrence = command.ForecastOccurrenceId.HasValue
            ? await dbContext.ForecastOccurrences.FirstOrDefaultAsync(
                x => x.Id == command.ForecastOccurrenceId.Value && x.IsIncome, cancellationToken)
            : null;

        if (command.ForecastOccurrenceId.HasValue)
        {
            if (occurrence == null)
                throw new InvalidOperationException($"ForecastOccurrence with id {command.ForecastOccurrenceId.Value} not found");
            if (occurrence.ForecastOccurrenceStatusId != ForecastOccurrenceStatus.PendingId)
                throw new InvalidOperationException($"ForecastOccurrence with id {command.ForecastOccurrenceId.Value} is not pending");
        }

        var income = new Income
        {
            Id = Guid.NewGuid(),
            Description = command.Description.Trim(),
            ForecastOccurrenceId = command.ForecastOccurrenceId,
            Amount = command.Amount,
            Date = command.Date,
            IdempotencyKey = string.IsNullOrWhiteSpace(command.IdempotencyKey) ? null : command.IdempotencyKey,
        };

        if (occurrence != null)
        {
            occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId;
            occurrence.ValidatedAt = DateTime.UtcNow;
        }

        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync(cancellationToken);

        return income.Id;
    }
}
