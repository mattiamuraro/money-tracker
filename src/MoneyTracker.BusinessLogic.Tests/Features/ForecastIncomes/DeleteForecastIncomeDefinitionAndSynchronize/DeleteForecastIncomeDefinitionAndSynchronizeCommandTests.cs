using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize;

public class DeleteForecastIncomeDefinitionAndSynchronizeCommandTests
{
    [Fact]
    public void Init_ShouldAssignId()
    {
        var id = Guid.NewGuid();
        var command = new DeleteForecastIncomeDefinitionAndSynchronizeCommand { Id = id };
        Assert.Equal(id, command.Id);
    }
}
