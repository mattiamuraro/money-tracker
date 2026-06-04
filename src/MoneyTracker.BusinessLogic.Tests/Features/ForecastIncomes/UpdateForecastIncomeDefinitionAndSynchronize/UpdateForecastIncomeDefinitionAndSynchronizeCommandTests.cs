using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;

public class UpdateForecastIncomeDefinitionAndSynchronizeCommandTests
{
    [Fact]
    public void Init_ShouldAssignUpdateCommand()
    {
        var update = new UpdateForecastIncomeDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "Salary",
            Amount = 2500m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        var command = new UpdateForecastIncomeDefinitionAndSynchronizeCommand { UpdateCommand = update };

        Assert.Same(update, command.UpdateCommand);
    }
}
