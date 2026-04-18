using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;

public class DeleteIncomeCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public DeleteIncomeCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteIncomeCommand request, CancellationToken cancellationToken)
    {
        var income = await _dbContext.Incomes.FindAsync(
            new object[] { request.IncomeId },
            cancellationToken: cancellationToken);

        if (income == null)
            return false;

        if (income.ForecastOccurrenceId.HasValue)
        {
            var occurrence = await _dbContext.ForecastOccurrences
                .FirstOrDefaultAsync(x => x.Id == income.ForecastOccurrenceId.Value, cancellationToken);

            if (occurrence != null)
            {
                occurrence.ForecastOccurrenceStatusId = ResolveOccurrenceStatusId(occurrence.ExpectedDate, request.OccurrenceAction);
                occurrence.ValidatedAt = null;
            }
        }

        income.Delete(request.DeletedBy == Guid.Empty ? SystemUsers.SystemUserId : request.DeletedBy);

        _dbContext.Incomes.Update(income);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static Guid ResolveOccurrenceStatusId(DateOnly expectedDate, ForecastOccurrenceDeleteAction action)
    {
        return action switch
        {
            ForecastOccurrenceDeleteAction.Reopen => ForecastOccurrenceStatus.PendingId,
            ForecastOccurrenceDeleteAction.Skip => ForecastOccurrenceStatus.SkippedId,
            _ => expectedDate >= DateOnly.FromDateTime(DateTime.Today)
                ? ForecastOccurrenceStatus.PendingId
                : ForecastOccurrenceStatus.SkippedId
        };
    }
}
