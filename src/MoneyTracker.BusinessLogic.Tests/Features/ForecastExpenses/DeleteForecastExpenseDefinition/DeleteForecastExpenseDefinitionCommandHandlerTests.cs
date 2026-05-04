using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.DeleteForecastExpenseDefinition;

public class DeleteForecastExpenseDefinitionCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static DeleteForecastExpenseDefinitionCommandHandler CreateHandler(MoneyTrackerDbContext db) => new(db);

    private static async Task<ForecastExpense> SeedActiveForecastExpenseAsync(MoneyTrackerDbContext db)
    {
        var expense = new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 800m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1,
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            PaymentCategoryId = Guid.NewGuid(),
            IsActive = true
        };
        db.ForecastExpenses.Add(expense);
        await db.SaveChangesAsync();
        return expense;
    }

    [Fact]
    public async Task Handle_ActiveExpense_SetsIsActiveFalse()
    {
        // Arrange
        using var db = CreateDbContext();
        var expense = await SeedActiveForecastExpenseAsync(db);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(new DeleteForecastExpenseDefinitionCommand(expense.Id), CancellationToken.None);

        // Assert
        var result = await db.ForecastExpenses.FindAsync(expense.Id);
        Assert.False(result!.IsActive);
    }

    [Fact]
    public async Task Handle_ActiveExpense_CancelsPendingOccurrences()
    {
        // Arrange
        using var db = CreateDbContext();
        var expense = await SeedActiveForecastExpenseAsync(db);
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = expense.Id,
            IsIncome = false,
            Description = "Rent",
            Amount = 800m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        });
        await db.SaveChangesAsync();
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(new DeleteForecastExpenseDefinitionCommand(expense.Id), CancellationToken.None);

        // Assert
        var occurrences = await db.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == expense.Id)
            .ToListAsync();
        Assert.All(occurrences, o => Assert.Equal(ForecastOccurrenceStatus.CancelledId, o.ForecastOccurrenceStatusId));
    }

    [Fact]
    public async Task Handle_NotFoundExpense_ThrowsEntityNotFoundException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(new DeleteForecastExpenseDefinitionCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
