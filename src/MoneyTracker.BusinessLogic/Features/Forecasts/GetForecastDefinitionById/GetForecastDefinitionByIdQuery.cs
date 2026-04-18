namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;

public class GetForecastDefinitionByIdQuery
{
    public Guid Id { get; set; }

    public GetForecastDefinitionByIdQuery(Guid id)
    {
        Id = id;
    }
}
