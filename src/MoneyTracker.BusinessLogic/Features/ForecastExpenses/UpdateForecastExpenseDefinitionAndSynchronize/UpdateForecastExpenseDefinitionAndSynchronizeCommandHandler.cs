using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;

public class UpdateForecastExpenseDefinitionAndSynchronizeCommandHandler(
    IHandler<UpdateForecastExpenseDefinitionCommand> updateHandler,
    IHandler<SynchronizeForecastOccurrencesCommand> synchronizeHandler)
    : IHandler<UpdateForecastExpenseDefinitionAndSynchronizeCommand>
{
    public async Task Handle(UpdateForecastExpenseDefinitionAndSynchronizeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.UpdateCommand);

        await updateHandler.Handle(request.UpdateCommand, cancellationToken);
        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
    }
}
