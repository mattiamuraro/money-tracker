using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;

public class DiscardForecastIncomeOccurrenceCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static DiscardForecastIncomeOccurrenceCommandHandler CreateHandler(MoneyTrackerDbContext db) => new(db);

    private static async Task<ForecastOccurrence> SeedPendingIncomeOccurrenceAsync(MoneyTrackerDbContext db)
    {
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
        return occurrence;
    }

    [Fact]
    public async Task Handle_PendingIncomeOccurrence_SetsStatusToSkipped()
    {
        // Arrange
        using var db = CreateDbContext();
        var occurrence = await SeedPendingIncomeOccurrenceAsync(db);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(new DiscardForecastIncomeOccurrenceCommand(occurrence.Id), CancellationToken.None);

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
            () => handler.Handle(new DiscardForecastIncomeOccurrenceCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonPendingOccurrence_ThrowsConflictException()
    {
        // Arrange
        using var db = CreateDbContext();
        var occurrence = await SeedPendingIncomeOccurrenceAsync(db);
        occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.SkippedId;
        await db.SaveChangesAsync();
        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new DiscardForecastIncomeOccurrenceCommand(occurrence.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExpenseOccurrence_ThrowsEntityNotFoundException()
    {
        // Arrange
        using var db = CreateDbContext();
        var occurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = false,
            Description = "Bill",
            Amount = 50m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        db.ForecastOccurrences.Add(occurrence);
        await db.SaveChangesAsync();
        var handler = CreateHandler(db);

        // Act & Assert — expense occurrence must not be found by income handler
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(new DiscardForecastIncomeOccurrenceCommand(occurrence.Id), CancellationToken.None));
    }
}
