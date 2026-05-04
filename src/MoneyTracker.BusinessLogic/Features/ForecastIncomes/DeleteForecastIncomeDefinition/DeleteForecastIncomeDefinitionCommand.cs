namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;

public class DeleteForecastIncomeDefinitionCommand
{
    public Guid Id { get; set; }

    public DeleteForecastIncomeDefinitionCommand(Guid id)
    {
        Id = id;
    }
}
