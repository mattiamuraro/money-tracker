using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;

public class CreateForecastIncomeDefinitionAndSynchronizeCommandHandler(
    IHandler<CreateForecastIncomeDefinitionCommand, Guid> createHandler,
    IHandler<SynchronizeForecastOccurrencesCommand> synchronizeHandler)
    : IHandler<CreateForecastIncomeDefinitionAndSynchronizeCommand, Guid>
{
    public async Task<Guid> Handle(CreateForecastIncomeDefinitionAndSynchronizeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.CreateCommand);

        var id = await createHandler.Handle(request.CreateCommand, cancellationToken);
        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

        return id;
    }
}
