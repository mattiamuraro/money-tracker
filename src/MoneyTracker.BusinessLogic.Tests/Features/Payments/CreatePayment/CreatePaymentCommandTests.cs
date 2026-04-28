using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.CreatePayment;

public class CreatePaymentCommandTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateInstance_WithDefaultValues()
    {
        // Arrange & Act
        var command = new CreatePaymentCommand();

        // Assert
        Assert.Equal(string.Empty, command.Description);
        Assert.Equal(Guid.Empty, command.PaymentCategoryId);
        Assert.Null(command.ForecastOccurrenceId);
        Assert.Equal(0m, command.Amount);
        Assert.Equal(default(DateTime), command.Date);
        Assert.False(command.IsOneShot);
        Assert.Equal(Guid.Empty, command.CreatedById);
        Assert.Null(command.IdempotencyKey);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetAllProperties_WhenCalledWithValidParameters()
    {
        // Arrange
        var description = "Test Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 100.50m;
        var date = new DateTime(2024, 1, 15);
        var createdById = Guid.NewGuid();
        var isOneShot = true;
        var idempotencyKey = "test-key-123";
        var forecastOccurrenceId = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById,
            isOneShot,
            idempotencyKey,
            forecastOccurrenceId);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(paymentCategoryId, command.PaymentCategoryId);
        Assert.Equal(amount, command.Amount);
        Assert.Equal(date, command.Date);
        Assert.Equal(createdById, command.CreatedById);
        Assert.Equal(isOneShot, command.IsOneShot);
        Assert.Equal(idempotencyKey, command.IdempotencyKey);
        Assert.Equal(forecastOccurrenceId, command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetDefaultValues_WhenOptionalParametersNotProvided()
    {
        // Arrange
        var description = "Test Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 50.25m;
        var date = new DateTime(2024, 2, 20);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(paymentCategoryId, command.PaymentCategoryId);
        Assert.Equal(amount, command.Amount);
        Assert.Equal(date, command.Date);
        Assert.Equal(createdById, command.CreatedById);
        Assert.False(command.IsOneShot);
        Assert.Null(command.IdempotencyKey);
        Assert.Null(command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldThrowArgumentNullException_WhenDescriptionIsNull()
    {
        // Arrange
        string? description = null;
        var paymentCategoryId = Guid.NewGuid();
        var amount = 100m;
        var date = DateTime.Now;
        var createdById = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CreatePaymentCommand(
                description!,
                paymentCategoryId,
                amount,
                date,
                createdById));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithEmptyGuids()
    {
        // Arrange
        var description = "Test Payment";
        var paymentCategoryId = Guid.Empty;
        var amount = 0m;
        var date = DateTime.MinValue;
        var createdById = Guid.Empty;

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(Guid.Empty, command.PaymentCategoryId);
        Assert.Equal(0m, command.Amount);
        Assert.Equal(DateTime.MinValue, command.Date);
        Assert.Equal(Guid.Empty, command.CreatedById);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithNegativeAmount()
    {
        // Arrange
        var description = "Refund Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = -50.75m;
        var date = new DateTime(2024, 3, 10);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(amount, command.Amount);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithZeroAmount()
    {
        // Arrange
        var description = "Zero Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 0m;
        var date = new DateTime(2024, 4, 5);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(0m, command.Amount);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithLargeAmount()
    {
        // Arrange
        var description = "Large Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 999999999.99m;
        var date = new DateTime(2024, 5, 25);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(999999999.99m, command.Amount);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithEmptyDescription()
    {
        // Arrange
        var description = string.Empty;
        var paymentCategoryId = Guid.NewGuid();
        var amount = 25m;
        var date = new DateTime(2024, 6, 15);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(string.Empty, command.Description);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIsOneShotToTrue_WhenExplicitlySetToTrue()
    {
        // Arrange
        var description = "One Shot Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 75m;
        var date = new DateTime(2024, 7, 20);
        var createdById = Guid.NewGuid();
        var isOneShot = true;

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById,
            isOneShot);

        // Assert
        Assert.True(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIsOneShotToFalse_WhenExplicitlySetToFalse()
    {
        // Arrange
        var description = "Recurring Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 125m;
        var date = new DateTime(2024, 8, 30);
        var createdById = Guid.NewGuid();
        var isOneShot = false;

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById,
            isOneShot);

        // Assert
        Assert.False(command.IsOneShot);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIdempotencyKey_WhenProvided()
    {
        // Arrange
        var description = "Payment with Key";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 200m;
        var date = new DateTime(2024, 9, 10);
        var createdById = Guid.NewGuid();
        var idempotencyKey = "unique-key-456";

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById,
            false,
            idempotencyKey);

        // Assert
        Assert.Equal(idempotencyKey, command.IdempotencyKey);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIdempotencyKeyToNull_WhenNotProvided()
    {
        // Arrange
        var description = "Payment without Key";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 150m;
        var date = new DateTime(2024, 10, 5);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById,
            false,
            null);

        // Assert
        Assert.Null(command.IdempotencyKey);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetForecastOccurrenceId_WhenProvided()
    {
        // Arrange
        var description = "Forecast Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 300m;
        var date = new DateTime(2024, 11, 15);
        var createdById = Guid.NewGuid();
        var forecastOccurrenceId = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById,
            false,
            null,
            forecastOccurrenceId);

        // Assert
        Assert.Equal(forecastOccurrenceId, command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetForecastOccurrenceIdToNull_WhenNotProvided()
    {
        // Arrange
        var description = "Non-Forecast Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 250m;
        var date = new DateTime(2024, 12, 25);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById,
            false,
            null,
            null);

        // Assert
        Assert.Null(command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetAllPropertiesCorrectly_WhenCalledWithMixedOptionalParameters()
    {
        // Arrange
        var description = "Mixed Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 175.50m;
        var date = new DateTime(2024, 1, 1);
        var createdById = Guid.NewGuid();
        var isOneShot = true;
        var forecastOccurrenceId = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById,
            isOneShot,
            null,
            forecastOccurrenceId);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(paymentCategoryId, command.PaymentCategoryId);
        Assert.Equal(amount, command.Amount);
        Assert.Equal(date, command.Date);
        Assert.Equal(createdById, command.CreatedById);
        Assert.True(command.IsOneShot);
        Assert.Null(command.IdempotencyKey);
        Assert.Equal(forecastOccurrenceId, command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithDateTimeMax()
    {
        // Arrange
        var description = "Future Payment";
        var paymentCategoryId = Guid.NewGuid();
        var amount = 500m;
        var date = DateTime.MaxValue;
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(DateTime.MaxValue, command.Date);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithSpecificGuid()
    {
        // Arrange
        var description = "Specific GUID Payment";
        var paymentCategoryId = new Guid("12345678-1234-1234-1234-123456789012");
        var amount = 450m;
        var date = new DateTime(2024, 3, 15);
        var createdById = new Guid("87654321-4321-4321-4321-210987654321");

        // Act
        var command = new CreatePaymentCommand(
            description,
            paymentCategoryId,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(new Guid("12345678-1234-1234-1234-123456789012"), command.PaymentCategoryId);
        Assert.Equal(new Guid("87654321-4321-4321-4321-210987654321"), command.CreatedById);
    }
}
