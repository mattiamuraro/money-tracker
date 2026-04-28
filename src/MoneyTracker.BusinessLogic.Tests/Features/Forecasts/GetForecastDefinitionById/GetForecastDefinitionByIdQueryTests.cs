using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.GetForecastDefinitionById;

public class GetForecastDefinitionByIdQueryTests
{
    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithValidGuid()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var query = new GetForecastDefinitionByIdQuery(expectedId);

        // Assert
        Assert.Equal(expectedId, query.Id);
    }

    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithEmptyGuid()
    {
        // Arrange
        var expectedId = Guid.Empty;

        // Act
        var query = new GetForecastDefinitionByIdQuery(expectedId);

        // Assert
        Assert.Equal(expectedId, query.Id);
    }

    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithSpecificGuid()
    {
        // Arrange
        var expectedId = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var query = new GetForecastDefinitionByIdQuery(expectedId);

        // Assert
        Assert.Equal(expectedId, query.Id);
    }
}
