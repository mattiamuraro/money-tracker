using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinitionAndSynchronize;

public class DeleteForecastExpenseDefinitionAndSynchronizeCommandHandler(
    IHandler<DeleteForecastExpenseDefinitionCommand> deleteHandler,
    IHandler<SynchronizeForecastOccurrencesCommand> synchronizeHandler)
    : IHandler<DeleteForecastExpenseDefinitionAndSynchronizeCommand>
{
    public async Task Handle(DeleteForecastExpenseDefinitionAndSynchronizeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await deleteHandler.Handle(new DeleteForecastExpenseDefinitionCommand(request.Id), cancellationToken);
        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
    }
}
