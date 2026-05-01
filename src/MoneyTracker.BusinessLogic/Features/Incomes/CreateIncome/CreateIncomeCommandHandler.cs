using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;

public class CreateIncomeCommandHandler
{
    private readonly IValidator<CreateIncomeCommand> _validator;
    private readonly MoneyTrackerDbContext _dbContext;

    public CreateIncomeCommandHandler(IValidator<CreateIncomeCommand> validator, MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<Guid> Handle(CreateIncomeCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            var existing = await _dbContext.Incomes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == command.IdempotencyKey, cancellationToken);

            if (existing is not null)
                return existing.Id;
        }

        var occurrence = command.ForecastOccurrenceId.HasValue
            ? await _dbContext.ForecastOccurrences.FirstOrDefaultAsync(
                x => x.Id == command.ForecastOccurrenceId.Value && x.IsIncome,
                cancellationToken)
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

        _dbContext.Incomes.Add(income);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return income.Id;
    }
}
