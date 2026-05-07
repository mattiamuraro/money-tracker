namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;

public sealed class CreateForecastExpenseDefinitionAndSynchronizeCommand
{
    public required CreateForecastExpenseDefinition.CreateForecastExpenseDefinitionCommand CreateCommand { get; init; }
}
