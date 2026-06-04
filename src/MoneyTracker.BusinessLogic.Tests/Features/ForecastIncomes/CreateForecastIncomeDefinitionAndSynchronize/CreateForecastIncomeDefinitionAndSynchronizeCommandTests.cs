using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;

public class CreateForecastIncomeDefinitionAndSynchronizeCommandTests
{
    [Fact]
    public void Init_ShouldAssignCreateCommand()
    {
        var create = new CreateForecastIncomeDefinitionCommand
        {
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "Salary",
            Amount = 2500m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        var command = new CreateForecastIncomeDefinitionAndSynchronizeCommand { CreateCommand = create };

        Assert.Same(create, command.CreateCommand);
    }
}
