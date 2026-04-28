using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.CreateIncome;

public class CreateIncomeCommandTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateInstance_WithDefaultValues()
    {
        // Arrange & Act
        var command = new CreateIncomeCommand();

        // Assert
        Assert.Equal(string.Empty, command.Description);
        Assert.Null(command.ForecastOccurrenceId);
        Assert.Equal(0m, command.Amount);
        Assert.Equal(default(DateTime), command.Date);
        Assert.Equal(Guid.Empty, command.CreatedById);
        Assert.Null(command.IdempotencyKey);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetAllProperties_WhenCalledWithValidParameters()
    {
        // Arrange
        var description = "Test Income";
        var amount = 100.50m;
        var date = new DateTime(2024, 1, 15);
        var createdById = Guid.NewGuid();
        var idempotencyKey = "test-key-123";
        var forecastOccurrenceId = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById,
            idempotencyKey,
            forecastOccurrenceId);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(amount, command.Amount);
        Assert.Equal(date, command.Date);
        Assert.Equal(createdById, command.CreatedById);
        Assert.Equal(idempotencyKey, command.IdempotencyKey);
        Assert.Equal(forecastOccurrenceId, command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetDefaultValues_WhenOptionalParametersNotProvided()
    {
        // Arrange
        var description = "Test Income";
        var amount = 50.25m;
        var date = new DateTime(2024, 2, 20);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(amount, command.Amount);
        Assert.Equal(date, command.Date);
        Assert.Equal(createdById, command.CreatedById);
        Assert.Null(command.IdempotencyKey);
        Assert.Null(command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldThrowArgumentNullException_WhenDescriptionIsNull()
    {
        // Arrange
        string? description = null;
        var amount = 100m;
        var date = DateTime.Now;
        var createdById = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CreateIncomeCommand(
                description!,
                amount,
                date,
                createdById));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithEmptyGuids()
    {
        // Arrange
        var description = "Test Income";
        var amount = 0m;
        var date = DateTime.MinValue;
        var createdById = Guid.Empty;

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(0m, command.Amount);
        Assert.Equal(DateTime.MinValue, command.Date);
        Assert.Equal(Guid.Empty, command.CreatedById);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithNegativeAmount()
    {
        // Arrange
        var description = "Refund Income";
        var amount = -50.75m;
        var date = new DateTime(2024, 3, 10);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
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
        var description = "Zero Income";
        var amount = 0m;
        var date = new DateTime(2024, 4, 5);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
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
        var description = "Large Income";
        var amount = 999999999.99m;
        var date = new DateTime(2024, 5, 25);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
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
        var amount = 25m;
        var date = new DateTime(2024, 6, 15);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(string.Empty, command.Description);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIdempotencyKey_WhenProvided()
    {
        // Arrange
        var description = "Income with Key";
        var amount = 200m;
        var date = new DateTime(2024, 9, 10);
        var createdById = Guid.NewGuid();
        var idempotencyKey = "unique-key-456";

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById,
            idempotencyKey);

        // Assert
        Assert.Equal(idempotencyKey, command.IdempotencyKey);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetIdempotencyKeyToNull_WhenNotProvided()
    {
        // Arrange
        var description = "Income without Key";
        var amount = 150m;
        var date = new DateTime(2024, 10, 5);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById,
            null);

        // Assert
        Assert.Null(command.IdempotencyKey);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetForecastOccurrenceId_WhenProvided()
    {
        // Arrange
        var description = "Forecast Income";
        var amount = 300m;
        var date = new DateTime(2024, 11, 15);
        var createdById = Guid.NewGuid();
        var forecastOccurrenceId = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById,
            null,
            forecastOccurrenceId);

        // Assert
        Assert.Equal(forecastOccurrenceId, command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetForecastOccurrenceIdToNull_WhenNotProvided()
    {
        // Arrange
        var description = "Non-Forecast Income";
        var amount = 250m;
        var date = new DateTime(2024, 12, 25);
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById,
            null,
            null);

        // Assert
        Assert.Null(command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetAllPropertiesCorrectly_WhenCalledWithMixedOptionalParameters()
    {
        // Arrange
        var description = "Mixed Income";
        var amount = 175.50m;
        var date = new DateTime(2024, 1, 1);
        var createdById = Guid.NewGuid();
        var forecastOccurrenceId = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById,
            null,
            forecastOccurrenceId);

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(amount, command.Amount);
        Assert.Equal(date, command.Date);
        Assert.Equal(createdById, command.CreatedById);
        Assert.Null(command.IdempotencyKey);
        Assert.Equal(forecastOccurrenceId, command.ForecastOccurrenceId);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetProperties_WhenCalledWithDateTimeMax()
    {
        // Arrange
        var description = "Future Income";
        var amount = 500m;
        var date = DateTime.MaxValue;
        var createdById = Guid.NewGuid();

        // Act
        var command = new CreateIncomeCommand(
            description,
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
        var description = "Specific GUID Income";
        var amount = 450m;
        var date = new DateTime(2024, 3, 15);
        var createdById = new Guid("87654321-4321-4321-4321-210987654321");

        // Act
        var command = new CreateIncomeCommand(
            description,
            amount,
            date,
            createdById);

        // Assert
        Assert.Equal(new Guid("87654321-4321-4321-4321-210987654321"), command.CreatedById);
    }
}
