using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.UpdatePayment;

public class UpdatePaymentCommandTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateInstance_WithDefaultValues()
    {
        // Arrange & Act
        var command = new UpdatePaymentCommand();

        // Assert
        Assert.Equal(Guid.Empty, command.PaymentId);
        Assert.Equal(Guid.Empty, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetAllProperties_WhenCalledWithAllParameters()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedDescription = "Test payment description";
        var expectedPaymentCategoryId = Guid.NewGuid();
        var expectedAmount = 1500.75m;
        var expectedDate = new DateTime(2024, 1, 15);
        var expectedIsOneShot = true;

        // Act
        var command = new UpdatePaymentCommand(
            expectedPaymentId,
            expectedModifiedById,
            expectedDescription,
            expectedPaymentCategoryId,
            expectedAmount,
            expectedDate,
            expectedIsOneShot);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Equal(expectedPaymentCategoryId, command.PaymentCategoryId);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Equal(expectedDate, command.Date);
        Assert.Equal(expectedIsOneShot, command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetRequiredProperties_WhenCalledWithOnlyRequiredParameters()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithEmptyGuids()
    {
        // Arrange
        var expectedPaymentId = Guid.Empty;
        var expectedModifiedById = Guid.Empty;
        var expectedDescription = "Empty payment";

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, expectedDescription);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNullDescription()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        string? expectedDescription = null;
        var expectedAmount = 100m;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, expectedDescription, amount: expectedAmount);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNullPaymentCategoryId()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedDescription = "Test payment";
        Guid? expectedPaymentCategoryId = null;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, expectedDescription, expectedPaymentCategoryId);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNullAmount()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedDescription = "Test payment";
        decimal? expectedAmount = null;
        var expectedDate = DateTime.UtcNow;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, expectedDescription, amount: expectedAmount, date: expectedDate);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Equal(expectedDate, command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNullDate()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedDescription = "Test payment";
        var expectedAmount = 500.50m;
        DateTime? expectedDate = null;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, expectedDescription, amount: expectedAmount, date: expectedDate);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNullIsOneShot()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedDescription = "Test payment";
        bool? expectedIsOneShot = null;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, expectedDescription, isOneShot: expectedIsOneShot);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithAllNullOptionalParameters()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, null, null, null, null, null);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithZeroAmount()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedAmount = 0m;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, amount: expectedAmount);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithNegativeAmount()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedAmount = -100.50m;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, amount: expectedAmount);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithEmptyDescription()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedDescription = string.Empty;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, expectedDescription);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithMinDateTimeValue()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedDate = DateTime.MinValue;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, date: expectedDate);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Equal(expectedDate, command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithMaxDateTimeValue()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedDate = DateTime.MaxValue;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, date: expectedDate);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Equal(expectedDate, command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithMaxDecimalValue()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedAmount = decimal.MaxValue;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, amount: expectedAmount);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithMinDecimalValue()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedAmount = decimal.MinValue;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, amount: expectedAmount);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Equal(expectedAmount, command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithSpecificGuids()
    {
        // Arrange
        var expectedPaymentId = new Guid("12345678-1234-1234-1234-123456789012");
        var expectedModifiedById = new Guid("87654321-4321-4321-4321-210987654321");
        var expectedDescription = "Specific GUID test";

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, expectedDescription);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Equal(expectedDescription, command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithIsOneShotTrue()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedIsOneShot = true;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, isOneShot: expectedIsOneShot);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Equal(expectedIsOneShot, command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithIsOneShotFalse()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedIsOneShot = false;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, isOneShot: expectedIsOneShot);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Null(command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Equal(expectedIsOneShot, command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithPaymentCategoryId()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedPaymentCategoryId = Guid.NewGuid();

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, paymentCategoryId: expectedPaymentCategoryId);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Equal(expectedPaymentCategoryId, command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPropertiesCorrectly_WhenCalledWithEmptyPaymentCategoryId()
    {
        // Arrange
        var expectedPaymentId = Guid.NewGuid();
        var expectedModifiedById = Guid.NewGuid();
        var expectedPaymentCategoryId = Guid.Empty;

        // Act
        var command = new UpdatePaymentCommand(expectedPaymentId, expectedModifiedById, paymentCategoryId: expectedPaymentCategoryId);

        // Assert
        Assert.Equal(expectedPaymentId, command.PaymentId);
        Assert.Equal(expectedModifiedById, command.ModifiedById);
        Assert.Null(command.Description);
        Assert.Equal(expectedPaymentCategoryId, command.PaymentCategoryId);
        Assert.Null(command.Amount);
        Assert.Null(command.Date);
        Assert.Null(command.IsOneShot);
    }
}
