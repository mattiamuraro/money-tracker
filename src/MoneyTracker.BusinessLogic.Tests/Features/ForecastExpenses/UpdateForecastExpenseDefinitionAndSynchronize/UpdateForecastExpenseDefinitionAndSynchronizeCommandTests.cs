using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;

public class UpdateForecastExpenseDefinitionAndSynchronizeCommandTests
{
    [Fact]
    public void Init_ShouldAssignUpdateCommand()
    {
        var update = new UpdateForecastExpenseDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1200m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1,
            PaymentCategoryId = Guid.NewGuid()
        };

        var command = new UpdateForecastExpenseDefinitionAndSynchronizeCommand { UpdateCommand = update };

        Assert.Same(update, command.UpdateCommand);
    }
}
