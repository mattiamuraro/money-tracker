using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Forecasts.DiscardPendingForecastOccurrence;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.DiscardPendingForecastOccurrence;

public class DiscardPendingForecastOccurrenceCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    [Fact]
    public void Constructor_ShouldInitialize_AllDependencies()
    {
        // Arrange
        using var db = CreateDbContext();

        // Act
        var handler = new DiscardPendingForecastOccurrenceCommandHandler(db);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_PendingOccurrenceFound_DiscardsForecastOccurrence()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 100.00m,
            Description = "Test Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            ValidatedAt = DateTime.UtcNow
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new DiscardPendingForecastOccurrenceCommandHandler(dbContext);
        var command = new DiscardPendingForecastOccurrenceCommand(occurrenceId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOccurrence = await dbContext.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.NotNull(updatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, updatedOccurrence.ForecastOccurrenceStatusId);
        Assert.Null(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_OccurrenceNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var handler = new DiscardPendingForecastOccurrenceCommandHandler(dbContext);
        var nonExistentId = Guid.NewGuid();
        var command = new DiscardPendingForecastOccurrenceCommand(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal($"Forecast Occurrence with id {nonExistentId} not found", exception.Message);
    }

    [Fact]
    public async Task Handle_OccurrenceNotPending_ThrowsConflictException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 100.00m,
            Description = "Test Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new DiscardPendingForecastOccurrenceCommandHandler(dbContext);
        var command = new DiscardPendingForecastOccurrenceCommand(occurrenceId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Only pending forecast occurrences can be discarded.", exception.Message);
    }

    [Fact]
    public async Task Handle_OccurrenceAlreadySkipped_ThrowsConflictException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 100.00m,
            Description = "Test Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.SkippedId
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new DiscardPendingForecastOccurrenceCommandHandler(dbContext);
        var command = new DiscardPendingForecastOccurrenceCommand(occurrenceId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Only pending forecast occurrences can be discarded.", exception.Message);
    }

    [Fact]
    public async Task Handle_OccurrenceCancelled_ThrowsConflictException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 100.00m,
            Description = "Test Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new DiscardPendingForecastOccurrenceCommandHandler(dbContext);
        var command = new DiscardPendingForecastOccurrenceCommand(occurrenceId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Only pending forecast occurrences can be discarded.", exception.Message);
    }

    [Fact]
    public async Task Handle_PendingOccurrenceWithoutValidatedAt_DiscardsForecastOccurrence()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 100.00m,
            Description = "Test Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            ValidatedAt = null
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new DiscardPendingForecastOccurrenceCommandHandler(dbContext);
        var command = new DiscardPendingForecastOccurrenceCommand(occurrenceId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOccurrence = await dbContext.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.NotNull(updatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, updatedOccurrence.ForecastOccurrenceStatusId);
        Assert.Null(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_DiscardsSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 100.00m,
            Description = "Test Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            ValidatedAt = DateTime.UtcNow
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new DiscardPendingForecastOccurrenceCommandHandler(dbContext);
        var command = new DiscardPendingForecastOccurrenceCommand(occurrenceId);
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await handler.Handle(command, cancellationTokenSource.Token);

        // Assert
        var updatedOccurrence = await dbContext.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.NotNull(updatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, updatedOccurrence.ForecastOccurrenceStatusId);
        Assert.Null(updatedOccurrence.ValidatedAt);
    }
}
