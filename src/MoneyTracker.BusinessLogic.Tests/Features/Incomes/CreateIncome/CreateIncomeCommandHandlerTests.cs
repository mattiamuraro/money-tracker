using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.CreateIncome;

public class CreateIncomeCommandHandlerTests
{
    private readonly Mock<IValidator<CreateIncomeCommand>> _mockValidator;
    private readonly MoneyTrackerDbContext _dbContext;

    public CreateIncomeCommandHandlerTests()
    {
        _mockValidator = new Mock<IValidator<CreateIncomeCommand>>();

        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new MoneyTrackerDbContext(options, null!);
    }

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        // Act
        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldCreateIncome_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1500.75m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var income = await _dbContext.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Equal("Test Income", income.Description);
        Assert.Equal(1500.75m, income.Amount);
        Assert.Equal(command.Date, income.Date);
        Assert.Equal(command.CreatedById, income.CreatedById);
        Assert.Equal(command.CreatedById, income.ModifiedById);
        Assert.Null(income.IdempotencyKey);
        Assert.Null(income.ForecastOccurrenceId);
    }

    [Fact]
    public async Task Handle_ShouldTrimDescription_WhenCreatingIncome()
    {
        // Arrange
        var command = new CreateIncomeCommand(
            description: "  Test Income With Spaces  ",
            amount: 1000m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await _dbContext.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Equal("Test Income With Spaces", income.Description);
    }

    [Fact]
    public async Task Handle_ShouldStoreIdempotencyKey_WhenProvided()
    {
        // Arrange
        var idempotencyKey = "test-key-123";
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: idempotencyKey,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await _dbContext.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Equal(idempotencyKey, income.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingIncomeId_WhenIdempotencyKeyExists()
    {
        // Arrange
        var idempotencyKey = "duplicate-key";
        var existingIncomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = existingIncomeId,
            Description = "Existing Income",
            Amount = 500m,
            Date = DateTime.UtcNow.AddDays(-1),
            IdempotencyKey = idempotencyKey,
            CreatedById = Guid.NewGuid(),
            ModifiedById = Guid.NewGuid()
        };
        _dbContext.Incomes.Add(existingIncome);
        await _dbContext.SaveChangesAsync();

        var command = new CreateIncomeCommand(
            description: "New Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: idempotencyKey,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(existingIncomeId, result);
        var incomeCount = await _dbContext.Incomes.CountAsync();
        Assert.Equal(1, incomeCount); // Should not create a new income
    }

    [Fact]
    public async Task Handle_ShouldTreatWhitespaceIdempotencyKeyAsNull()
    {
        // Arrange
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: "   ",
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await _dbContext.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Null(income.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldCreateIncomeAndUpdateOccurrence_WhenForecastOccurrenceExists()
    {
        // Arrange
        var occurrenceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = 2000m,
            Description = "Expected Income",
            IsIncome = true,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            CreatedById = userId,
            ModifiedById = userId
        };
        _dbContext.ForecastOccurrences.Add(occurrence);
        await _dbContext.SaveChangesAsync();

        var command = new CreateIncomeCommand(
            description: "Actual Income",
            amount: 2000m,
            date: DateTime.UtcNow,
            createdById: userId,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await _dbContext.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Equal(occurrenceId, income.ForecastOccurrenceId);

        var updatedOccurrence = await _dbContext.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.NotNull(updatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.ConfirmedId, updatedOccurrence.ForecastOccurrenceStatusId);
        Assert.NotNull(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenForecastOccurrenceNotFound()
    {
        // Arrange
        var nonExistentOccurrenceId = Guid.NewGuid();
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: null,
            forecastOccurrenceId: nonExistentOccurrenceId
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains(nonExistentOccurrenceId.ToString(), exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenForecastOccurrenceIsNotIncome()
    {
        // Arrange
        var occurrenceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = 2000m,
            Description = "Expected Payment",
            IsIncome = false, // This is a payment, not an income
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            CreatedById = userId,
            ModifiedById = userId
        };
        _dbContext.ForecastOccurrences.Add(occurrence);
        await _dbContext.SaveChangesAsync();

        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains(occurrenceId.ToString(), exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenForecastOccurrenceIsNotPending()
    {
        // Arrange
        var occurrenceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = 2000m,
            Description = "Already Confirmed",
            IsIncome = true,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId, // Already confirmed
            CreatedById = userId,
            ModifiedById = userId
        };
        _dbContext.ForecastOccurrences.Add(occurrence);
        await _dbContext.SaveChangesAsync();

        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains(occurrenceId.ToString(), exception.Message);
        Assert.Contains("not pending", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        var command = new CreateIncomeCommand(
            description: "",
            amount: 0,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Description", "Description is required")
        };

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationFailures));

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCheckIdempotencyKey_WhenItIsNull()
    {
        // Arrange
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var income = await _dbContext.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Null(income.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldNotUpdateOccurrence_WhenForecastOccurrenceIdIsNull()
    {
        // Arrange
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateIncomeCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await _dbContext.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Null(income.ForecastOccurrenceId);
    }
}
