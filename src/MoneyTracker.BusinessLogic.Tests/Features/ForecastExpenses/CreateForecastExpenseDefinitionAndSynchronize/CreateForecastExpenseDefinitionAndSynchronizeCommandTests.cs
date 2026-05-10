using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;

public class CreateForecastExpenseDefinitionAndSynchronizeCommandTests
{
    [Fact]
    public void Init_ShouldAssignCreateCommand()
    {
        var create = new CreateForecastExpenseDefinitionCommand
        {
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1200m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1,
            PaymentCategoryId = Guid.NewGuid()
        };

        var command = new CreateForecastExpenseDefinitionAndSynchronizeCommand { CreateCommand = create };

        Assert.Same(create, command.CreateCommand);
    }
}
