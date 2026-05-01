using FluentValidation;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

public class UpdateIncomeCommandHandler
{
    private readonly IValidator<UpdateIncomeCommand> _validator;
    private readonly MoneyTrackerDbContext _dbContext;

    public UpdateIncomeCommandHandler(IValidator<UpdateIncomeCommand> validator, MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task Handle(UpdateIncomeCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var income = await _dbContext.Incomes.FindAsync(
            new object[] { command.IncomeId },
            cancellationToken: cancellationToken);

        if (income is null)
            throw new EntityNotFoundException($"Income with id {command.IncomeId} not found");

        if (!string.IsNullOrWhiteSpace(command.Description))
            income.Description = command.Description.Trim();

        if (command.Amount.HasValue)
            income.Amount = command.Amount.Value;

        if (command.Date.HasValue)
            income.Date = command.Date.Value;

        _dbContext.Incomes.Update(income);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
