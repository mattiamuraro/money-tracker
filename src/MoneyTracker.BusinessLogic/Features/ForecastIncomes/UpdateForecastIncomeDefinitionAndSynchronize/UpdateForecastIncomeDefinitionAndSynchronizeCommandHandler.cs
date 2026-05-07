using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;

public class UpdateForecastIncomeDefinitionAndSynchronizeCommandHandler(
    IHandler<UpdateForecastIncomeDefinitionCommand> updateHandler,
    IHandler<SynchronizeForecastOccurrencesCommand> synchronizeHandler)
    : IHandler<UpdateForecastIncomeDefinitionAndSynchronizeCommand>
{
    public async Task Handle(UpdateForecastIncomeDefinitionAndSynchronizeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.UpdateCommand);

        await updateHandler.Handle(request.UpdateCommand, cancellationToken);
        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
    }
}
