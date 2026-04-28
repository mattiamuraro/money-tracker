using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.DeleteIncome;

public class DeleteIncomeCommandTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateInstance_WithDefaultValues()
    {
        // Arrange & Act
        var command = new DeleteIncomeCommand();

        // Assert
        Assert.Equal(Guid.Empty, command.IncomeId);
        Assert.Equal(Guid.Empty, command.DeletedBy);
        Assert.Null(command.OccurrenceAction);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIncomeIdProperty_WhenCalledWithValidGuid()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        string? occurrenceAction = "single";

        // Act
        var command = new DeleteIncomeCommand(expectedId, occurrenceAction);

        // Assert
        Assert.Equal(expectedId, command.IncomeId);
        Assert.Equal(occurrenceAction, command.OccurrenceAction);
        Assert.Equal(Guid.Empty, command.DeletedBy);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIncomeIdProperty_WhenCalledWithEmptyGuid()
    {
        // Arrange
        var expectedId = Guid.Empty;
        string? occurrenceAction = null;

        // Act
        var command = new DeleteIncomeCommand(expectedId, occurrenceAction);

        // Assert
        Assert.Equal(expectedId, command.IncomeId);
        Assert.Null(command.OccurrenceAction);
        Assert.Equal(Guid.Empty, command.DeletedBy);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIncomeIdProperty_WhenCalledWithSpecificGuid()
    {
        // Arrange
        var expectedId = new Guid("12345678-1234-1234-1234-123456789012");
        string? occurrenceAction = "all";

        // Act
        var command = new DeleteIncomeCommand(expectedId, occurrenceAction);

        // Assert
        Assert.Equal(expectedId, command.IncomeId);
        Assert.Equal(occurrenceAction, command.OccurrenceAction);
        Assert.Equal(Guid.Empty, command.DeletedBy);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetOccurrenceActionToNull_WhenCalledWithNullValue()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        string? occurrenceAction = null;

        // Act
        var command = new DeleteIncomeCommand(incomeId, occurrenceAction);

        // Assert
        Assert.Equal(incomeId, command.IncomeId);
        Assert.Null(command.OccurrenceAction);
        Assert.Equal(Guid.Empty, command.DeletedBy);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetOccurrenceAction_WhenCalledWithEmptyString()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        string? occurrenceAction = string.Empty;

        // Act
        var command = new DeleteIncomeCommand(incomeId, occurrenceAction);

        // Assert
        Assert.Equal(incomeId, command.IncomeId);
        Assert.Equal(string.Empty, command.OccurrenceAction);
        Assert.Equal(Guid.Empty, command.DeletedBy);
    }
}
