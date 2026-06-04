using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.CreateIncome;

public class CreateIncomeCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static CreateIncomeCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new CreateIncomeCommandValidator(), db);

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldCreateIncome_WhenCommandIsValid()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1500.75m,
            date: DateTime.UtcNow,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var income = await db.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Equal("Test Income", income.Description);
        Assert.Equal(1500.75m, income.Amount);
        Assert.Equal(command.Date, income.Date);
        Assert.Null(income.IdempotencyKey);
        Assert.Null(income.ForecastOccurrenceId);
    }

    [Fact]
    public async Task Handle_ShouldTrimDescription_WhenCreatingIncome()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateIncomeCommand(
            description: "  Test Income With Spaces  ",
            amount: 1000m,
            date: DateTime.UtcNow,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await db.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Equal("Test Income With Spaces", income.Description);
    }

    [Fact]
    public async Task Handle_ShouldStoreIdempotencyKey_WhenProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var idempotencyKey = "test-key-123";
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            idempotencyKey: idempotencyKey,
            forecastOccurrenceId: null
        );

        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await db.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Equal(idempotencyKey, income.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingIncomeId_WhenIdempotencyKeyExists()
    {
        // Arrange
        using var db = CreateDbContext();
        var idempotencyKey = "duplicate-key";
        var existingIncomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = existingIncomeId,
            Description = "Existing Income",
            Amount = 500m,
            Date = DateTime.UtcNow.AddDays(-1),
            IdempotencyKey = idempotencyKey
        };
        db.Incomes.Add(existingIncome);
        await db.SaveChangesAsync();

        var command = new CreateIncomeCommand(
            description: "New Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            idempotencyKey: idempotencyKey,
            forecastOccurrenceId: null
        );

        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(existingIncomeId, result);
        var incomeCount = await db.Incomes.CountAsync();
        Assert.Equal(1, incomeCount); // Should not create a new income
    }

    [Fact]
    public async Task Handle_ShouldTreatWhitespaceIdempotencyKeyAsNull()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            idempotencyKey: "   ",
            forecastOccurrenceId: null
        );

        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await db.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Null(income.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldCreateIncomeAndUpdateOccurrence_WhenForecastOccurrenceExists()
    {
        // Arrange
        using var db = CreateDbContext();
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
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        db.ForecastOccurrences.Add(occurrence);
        await db.SaveChangesAsync();

        var command = new CreateIncomeCommand(
            description: "Actual Income",
            amount: 2000m,
            date: DateTime.UtcNow,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await db.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Equal(occurrenceId, income.ForecastOccurrenceId);

        var updatedOccurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.NotNull(updatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.ConfirmedId, updatedOccurrence.ForecastOccurrenceStatusId);
        Assert.NotNull(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenForecastOccurrenceNotFound()
    {
        // Arrange
        using var db = CreateDbContext();
        var nonExistentOccurrenceId = Guid.NewGuid();
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            idempotencyKey: null,
            forecastOccurrenceId: nonExistentOccurrenceId
        );

        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains(nonExistentOccurrenceId.ToString(), exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenForecastOccurrenceIsNotIncome()
    {
        // Arrange
        using var db = CreateDbContext();
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
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        db.ForecastOccurrences.Add(occurrence);
        await db.SaveChangesAsync();

        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains(occurrenceId.ToString(), exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenForecastOccurrenceIsNotPending()
    {
        // Arrange
        using var db = CreateDbContext();
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
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId // Already confirmed
        };
        db.ForecastOccurrences.Add(occurrence);
        await db.SaveChangesAsync();

        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains(occurrenceId.ToString(), exception.Message);
        Assert.Contains("not pending", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateIncomeCommand(
            description: "",
            amount: 0,
            date: DateTime.UtcNow,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Description", "Description is required")
        };

        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCheckIdempotencyKey_WhenItIsNull()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var income = await db.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Null(income.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldNotUpdateOccurrence_WhenForecastOccurrenceIdIsNull()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateIncomeCommand(
            description: "Test Income",
            amount: 1000m,
            date: DateTime.UtcNow,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var income = await db.Incomes.FindAsync(result);
        Assert.NotNull(income);
        Assert.Null(income.ForecastOccurrenceId);
    }
}


