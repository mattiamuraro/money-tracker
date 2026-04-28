using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.UpdateIncome;

public class UpdateIncomeCommandTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateInstance_WithDefaultValues()
    {
        // Arrange & Act
        var command = new UpdateIncomeCommand();

        // Assert
        Assert.Equal(Guid.Empty, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetAllProperties_WhenCalledWithAllParameters()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedDescription = "Test income description";
        var expectedAmount = 1500.75m;
        var expectedDate = new DateTime(2024, 1, 15);

        // Act
        var command = new UpdateIncomeCommand(
            expectedIncomeId,
            expectedDescription,
            expectedAmount,
            expectedDate);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Equal(expectedDate, command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIncomeId_WhenCalledWithOnlyRequiredParameter()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithEmptyGuid()
    {
        // Arrange
        var expectedIncomeId = Guid.Empty;
        var expectedDescription = "Empty income";

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, expectedDescription);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNullDescription()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        string? expectedDescription = null;
        var expectedAmount = 100m;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, expectedDescription, expectedAmount);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNullAmount()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedDescription = "Test income";
        decimal? expectedAmount = null;
        var expectedDate = DateTime.UtcNow;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, expectedDescription, expectedAmount, expectedDate);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.Amount);
        Assert.Equal(expectedDate, command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNullDate()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedDescription = "Test income";
        var expectedAmount = 500.50m;
        DateTime? expectedDate = null;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, expectedDescription, expectedAmount, expectedDate);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithAllNullOptionalParameters()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, null, null, null);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithZeroAmount()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedAmount = 0m;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, amount: expectedAmount);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNegativeAmount()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedAmount = -100.50m;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, amount: expectedAmount);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithEmptyDescription()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedDescription = string.Empty;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, expectedDescription);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithMinDateTimeValue()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedDate = DateTime.MinValue;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, date: expectedDate);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Null(command.Amount);
        Assert.Equal(expectedDate, command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithMaxDateTimeValue()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedDate = DateTime.MaxValue;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, date: expectedDate);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Null(command.Amount);
        Assert.Equal(expectedDate, command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithMaxDecimalValue()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedAmount = decimal.MaxValue;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, amount: expectedAmount);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithMinDecimalValue()
    {
        // Arrange
        var expectedIncomeId = Guid.NewGuid();
        var expectedAmount = decimal.MinValue;

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, amount: expectedAmount);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Null(command.Description);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithSpecificGuid()
    {
        // Arrange
        var expectedIncomeId = new Guid("12345678-1234-1234-1234-123456789012");
        var expectedDescription = "Specific GUID test";

        // Act
        var command = new UpdateIncomeCommand(expectedIncomeId, expectedDescription);

        // Assert
        Assert.Equal(expectedIncomeId, command.IncomeId);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
    }
}
