namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;

public class DeleteForecastExpenseDefinitionCommand
{
    public Guid Id { get; set; }

    public DeleteForecastExpenseDefinitionCommand(Guid id)
    {
        Id = id;
    }
}
