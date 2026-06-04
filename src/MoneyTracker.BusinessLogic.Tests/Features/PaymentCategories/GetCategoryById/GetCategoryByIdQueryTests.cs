using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;

namespace MoneyTracker.BusinessLogic.Tests.Features.PaymentCategories.GetCategoryById;

public class GetCategoryByIdQueryTests
{
    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithValidGuid()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var query = new GetCategoryByIdQuery(expectedId);

        // Assert
        Xunit.Assert.Equal(expectedId, query.Id);
    }

    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithEmptyGuid()
    {
        // Arrange
        var expectedId = Guid.Empty;

        // Act
        var query = new GetCategoryByIdQuery(expectedId);

        // Assert
        Xunit.Assert.Equal(expectedId, query.Id);
    }

    [Fact]
    public void Constructor_ShouldSetIdProperty_WhenCalledWithSpecificGuid()
    {
        // Arrange
        var expectedId = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var query = new GetCategoryByIdQuery(expectedId);

        // Assert
        Xunit.Assert.Equal(expectedId, query.Id);
    }
}
