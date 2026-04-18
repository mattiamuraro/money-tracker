namespace MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;

public class DeleteForecastDefinitionCommand
{
    public Guid Id { get; set; }

    public DeleteForecastDefinitionCommand(Guid id)
    {
        Id = id;
    }
}
