using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.DeleteIncome;

public class DeleteIncomeCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    [Fact]
    public void Constructor_Should_InitializeHandler_When_ValidDbContextProvided()
    {
        // Arrange
        using var db = CreateDbContext();

        // Act
        var handler = new DeleteIncomeCommandHandler(db);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_InvalidOccurrenceAction()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new DeleteIncomeCommand(Guid.NewGuid(), "InvalidAction");
        var handler = new DeleteIncomeCommandHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Equal("Occurrence action must be Auto, Reopen, or Skip.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_ThrowEntityNotFoundException_When_IncomeNotFound()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var command = new DeleteIncomeCommand(incomeId, "Auto");
        var handler = new DeleteIncomeCommandHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Contains(incomeId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_MarkIncomeAsDeleted_When_IncomeHasNoForecastOccurrence()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = null });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, "Auto");
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.True(updated!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_MarkIncomeAsDeleted_When_ForecastOccurrenceNotFound()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = Guid.NewGuid() });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, "Auto");
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.True(updated!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsReopen()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId, Description = "Test Occurrence", Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now, IsIncome = true, ForecastDefinitionId = Guid.NewGuid()
        });
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = occurrenceId });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, "Reopen");
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedIncome = await db.Incomes.FindAsync(incomeId);
        var updatedOccurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.True(updatedIncome!.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, updatedOccurrence!.ForecastOccurrenceStatusId);
        Assert.Null(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToSkipped_When_OccurrenceActionIsSkip()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId, Description = "Test Occurrence", Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now, IsIncome = true, ForecastDefinitionId = Guid.NewGuid()
        });
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = occurrenceId });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, "Skip");
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedIncome = await db.Incomes.FindAsync(incomeId);
        var updatedOccurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.True(updatedIncome!.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, updatedOccurrence!.ForecastOccurrenceStatusId);
        Assert.Null(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsAutoAndFutureDate()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId, Description = "Test Occurrence", Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now, IsIncome = true, ForecastDefinitionId = Guid.NewGuid()
        });
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = occurrenceId });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, "Auto");
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedIncome = await db.Incomes.FindAsync(incomeId);
        var updatedOccurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.True(updatedIncome!.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, updatedOccurrence!.ForecastOccurrenceStatusId);
        Assert.Null(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToSkipped_When_OccurrenceActionIsAutoAndPastDate()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId, Description = "Test Occurrence", Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now, IsIncome = true, ForecastDefinitionId = Guid.NewGuid()
        });
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = occurrenceId });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, "Auto");
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedIncome = await db.Incomes.FindAsync(incomeId);
        var updatedOccurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.True(updatedIncome!.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, updatedOccurrence!.ForecastOccurrenceStatusId);
        Assert.Null(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsAutoAndTodayDate()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId, Description = "Test Occurrence", Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now, IsIncome = true, ForecastDefinitionId = Guid.NewGuid()
        });
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = occurrenceId });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, "Auto");
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedIncome = await db.Incomes.FindAsync(incomeId);
        var updatedOccurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.True(updatedIncome!.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, updatedOccurrence!.ForecastOccurrenceStatusId);
        Assert.Null(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_When_Called()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = null });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, "Auto");
        var cancellationToken = new CancellationToken();
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.True(updated!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_HandleNullOccurrenceAction_When_OccurrenceActionIsNull()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = null });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, null);
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.True(updated!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_HandleEmptyOccurrenceAction_When_OccurrenceActionIsEmpty()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Test Income", Amount = 100m, Date = DateTime.Now, ForecastOccurrenceId = null });
        await db.SaveChangesAsync();

        var command = new DeleteIncomeCommand(incomeId, string.Empty);
        var handler = new DeleteIncomeCommandHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.True(updated!.IsDeleted);
    }
}