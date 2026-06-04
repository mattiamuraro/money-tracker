using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;

public class CreateForecastExpenseDefinitionAndSynchronizeCommandHandler(
    IHandler<CreateForecastExpenseDefinitionCommand, Guid> createHandler,
    IHandler<SynchronizeForecastOccurrencesCommand> synchronizeHandler)
    : IHandler<CreateForecastExpenseDefinitionAndSynchronizeCommand, Guid>
{
    public async Task<Guid> Handle(CreateForecastExpenseDefinitionAndSynchronizeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.CreateCommand);

        var id = await createHandler.Handle(request.CreateCommand, cancellationToken);
        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

        return id;
    }
}
