using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

public class UpdateIncomeCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public UpdateIncomeCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateIncomeCommand request, CancellationToken cancellationToken)
    {
        var income = await _dbContext.Incomes.FindAsync(
            new object[] { request.IncomeId },
            cancellationToken: cancellationToken);

        if (income is null)
            return false;

        if (!string.IsNullOrWhiteSpace(request.Description))
            income.Description = request.Description.Trim();

        if (request.Amount.HasValue)
            income.Amount = request.Amount.Value;

        if (request.Date.HasValue)
            income.Date = request.Date.Value;

        income.ModifiedById = request.ModifiedById;
        income.ModifiedAt = DateTime.UtcNow;

        _dbContext.Incomes.Update(income);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
