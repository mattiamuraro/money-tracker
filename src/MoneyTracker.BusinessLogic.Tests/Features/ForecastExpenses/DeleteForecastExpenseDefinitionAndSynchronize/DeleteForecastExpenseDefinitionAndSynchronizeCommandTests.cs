using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.DeleteForecastExpenseDefinitionAndSynchronize;

public class DeleteForecastExpenseDefinitionAndSynchronizeCommandTests
{
    [Fact]
    public void Init_ShouldAssignId()
    {
        var id = Guid.NewGuid();
        var command = new DeleteForecastExpenseDefinitionAndSynchronizeCommand { Id = id };
        Assert.Equal(id, command.Id);
    }
}
