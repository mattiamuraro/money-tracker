namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;

public sealed class CreateForecastIncomeDefinitionAndSynchronizeCommand
{
    public required CreateForecastIncomeDefinition.CreateForecastIncomeDefinitionCommand CreateCommand { get; init; }
}
