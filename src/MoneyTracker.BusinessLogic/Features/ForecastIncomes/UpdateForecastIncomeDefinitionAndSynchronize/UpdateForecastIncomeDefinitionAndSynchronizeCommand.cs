namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;

public sealed class UpdateForecastIncomeDefinitionAndSynchronizeCommand
{
    public required UpdateForecastIncomeDefinition.UpdateForecastIncomeDefinitionCommand UpdateCommand { get; init; }
}
