using MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.GetPaymentById;

public class GetPaymentByIdQueryTests
{
    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithValidGuid()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var query = new GetPaymentByIdQuery(expectedId);

        // Assert
        Xunit.Assert.Equal(expectedId, query.Id);
    }

    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithEmptyGuid()
    {
        // Arrange
        var expectedId = Guid.Empty;

        // Act
        var query = new GetPaymentByIdQuery(expectedId);

        // Assert
        Xunit.Assert.Equal(expectedId, query.Id);
    }

    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithSpecificGuid()
    {
        // Arrange
        var expectedId = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var query = new GetPaymentByIdQuery(expectedId);

        // Assert
        Xunit.Assert.Equal(expectedId, query.Id);
    }
}
