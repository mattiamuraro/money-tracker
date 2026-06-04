using FluentValidation;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

public class UpdateIncomeCommandHandler(
    IValidator<UpdateIncomeCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<UpdateIncomeCommand>
{
    public async Task Handle(UpdateIncomeCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var income = await dbContext.Incomes.FindAsync(
            new object[] { command.IncomeId }, cancellationToken: cancellationToken);
        if (income is null)
            throw new EntityNotFoundException($"Income with id {command.IncomeId} not found");

        if (!string.IsNullOrWhiteSpace(command.Description))
        {
            var normalizedDescription = command.Description.Trim();
            income.Description = normalizedDescription;
            income.DescriptionNormalized = normalizedDescription.ToUpperInvariant();
        }
        if (command.Amount.HasValue)
            income.Amount = command.Amount.Value;
        if (command.Date.HasValue)
            income.Date = command.Date.Value;

        dbContext.Incomes.Update(income);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
