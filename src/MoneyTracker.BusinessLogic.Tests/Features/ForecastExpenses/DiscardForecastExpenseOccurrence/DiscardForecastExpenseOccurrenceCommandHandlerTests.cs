using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;

public class DiscardForecastExpenseOccurrenceCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static DiscardForecastExpenseOccurrenceCommandHandler CreateHandler(MoneyTrackerDbContext db) => new(db);

    private static async Task<ForecastOccurrence> SeedPendingExpenseOccurrenceAsync(MoneyTrackerDbContext db)
    {
        var occurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = false,
            Description = "Rent",
            Amount = 800m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        db.ForecastOccurrences.Add(occurrence);
        await db.SaveChangesAsync();
        return occurrence;
    }

    [Fact]
    public async Task Handle_PendingExpenseOccurrence_SetsStatusToSkipped()
    {
        // Arrange
        using var db = CreateDbContext();
        var occurrence = await SeedPendingExpenseOccurrenceAsync(db);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(new DiscardForecastExpenseOccurrenceCommand(occurrence.Id), CancellationToken.None);

        // Assert
        var result = await db.ForecastOccurrences.FindAsync(occurrence.Id);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, result!.ForecastOccurrenceStatusId);
    }

    [Fact]
    public async Task Handle_NotFoundOccurrence_ThrowsEntityNotFoundException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(new DiscardForecastExpenseOccurrenceCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonPendingOccurrence_ThrowsConflictException()
    {
        // Arrange
        using var db = CreateDbContext();
        var occurrence = await SeedPendingExpenseOccurrenceAsync(db);
        occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.SkippedId;
        await db.SaveChangesAsync();
        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new DiscardForecastExpenseOccurrenceCommand(occurrence.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_IncomeOccurrence_ThrowsEntityNotFoundException()
    {
        // Arrange
        using var db = CreateDbContext();
        var occurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = true,
            Description = "Salary",
            Amount = 1000m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        db.ForecastOccurrences.Add(occurrence);
        await db.SaveChangesAsync();
        var handler = CreateHandler(db);

        // Act & Assert — income occurrence must not be found by expense handler
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(new DiscardForecastExpenseOccurrenceCommand(occurrence.Id), CancellationToken.None));
    }
}
