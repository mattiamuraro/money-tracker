namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;

public sealed class UpdateForecastExpenseDefinitionAndSynchronizeCommand
{
    public required UpdateForecastExpenseDefinition.UpdateForecastExpenseDefinitionCommand UpdateCommand { get; init; }
}
