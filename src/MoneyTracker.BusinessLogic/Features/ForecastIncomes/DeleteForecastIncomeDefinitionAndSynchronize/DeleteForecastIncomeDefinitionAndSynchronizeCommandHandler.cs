using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize;

public class DeleteForecastIncomeDefinitionAndSynchronizeCommandHandler(
    IHandler<DeleteForecastIncomeDefinitionCommand> deleteHandler,
    IHandler<SynchronizeForecastOccurrencesCommand> synchronizeHandler)
    : IHandler<DeleteForecastIncomeDefinitionAndSynchronizeCommand>
{
    public async Task Handle(DeleteForecastIncomeDefinitionAndSynchronizeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await deleteHandler.Handle(new DeleteForecastIncomeDefinitionCommand(request.Id), cancellationToken);
        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
    }
}
