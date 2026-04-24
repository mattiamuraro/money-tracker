using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment.ExtensionMethods;
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

    public async Task Handle(DeleteIncomeCommand request, CancellationToken cancellationToken)
    {
        if (!request.OccurrenceAction.TryParseOccurrenceAction(out var parsedAction))
            throw new BadRequestException("Occurrence action must be Auto, Reopen, or Skip.");

        var income = await _dbContext.Incomes.FindAsync(
            new object[] { request.IncomeId },
            cancellationToken: cancellationToken);

        if (income == null)
            throw new EntityNotFoundException($"Income with id {request.IncomeId} not found");

        if (income.ForecastOccurrenceId.HasValue)
        {
            var occurrence = await _dbContext.ForecastOccurrences
                .FirstOrDefaultAsync(x => x.Id == income.ForecastOccurrenceId.Value, cancellationToken);

            if (occurrence != null)
            {
                occurrence.ForecastOccurrenceStatusId = ResolveOccurrenceStatusId(occurrence.ExpectedDate, parsedAction);
                occurrence.ValidatedAt = null;
            }
        }

        income.IsDeleted = true;

        _dbContext.Incomes.Update(income);
        await _dbContext.SaveChangesAsync(cancellationToken);
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
