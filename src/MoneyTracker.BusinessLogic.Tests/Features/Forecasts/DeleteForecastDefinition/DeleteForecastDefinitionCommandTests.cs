using MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.DeleteForecastDefinition;

public class DeleteForecastDefinitionCommandTests
{
    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithValidGuid()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var command = new DeleteForecastDefinitionCommand(expectedId);

        // Assert
        Xunit.Assert.Equal(expectedId, command.Id);
    }

    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithEmptyGuid()
    {
        // Arrange
        var expectedId = Guid.Empty;

        // Act
        var command = new DeleteForecastDefinitionCommand(expectedId);

        // Assert
        Xunit.Assert.Equal(expectedId, command.Id);
    }

    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithSpecificGuid()
    {
        // Arrange
        var expectedId = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var command = new DeleteForecastDefinitionCommand(expectedId);

        // Assert
        Xunit.Assert.Equal(expectedId, command.Id);
    }
}
