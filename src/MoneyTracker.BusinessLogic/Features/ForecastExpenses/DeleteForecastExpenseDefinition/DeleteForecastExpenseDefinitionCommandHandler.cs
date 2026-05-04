using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;

public class DeleteForecastExpenseDefinitionCommandHandler(MoneyTrackerDbContext dbContext)
    : IHandler<DeleteForecastExpenseDefinitionCommand>
{
    public async Task Handle(DeleteForecastExpenseDefinitionCommand request, CancellationToken cancellationToken)
    {
        var expense = await dbContext.ForecastExpenses
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (expense is null)
            throw new EntityNotFoundException($"No active forecast expense found with ID '{request.Id}'.");

        expense.IsActive = false;

        var occurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == request.Id
                && x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId)
            .ToListAsync(cancellationToken);

        foreach (var occurrence in occurrences)
            occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
