using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.DeleteForecastIncomeDefinition;

public class DeleteForecastIncomeDefinitionCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static DeleteForecastIncomeDefinitionCommandHandler CreateHandler(MoneyTrackerDbContext db) => new(db);

    private static async Task<ForecastIncome> SeedActiveForecastIncomeAsync(MoneyTrackerDbContext db)
    {
        var income = new ForecastIncome
        {
            Id = Guid.NewGuid(),
            Description = "Salary",
            Amount = 1000m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1,
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            IsActive = true
        };
        db.ForecastIncomes.Add(income);
        await db.SaveChangesAsync();
        return income;
    }

    [Fact]
    public async Task Handle_ActiveIncome_SetsIsActiveFalse()
    {
        // Arrange
        using var db = CreateDbContext();
        var income = await SeedActiveForecastIncomeAsync(db);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(new DeleteForecastIncomeDefinitionCommand(income.Id), CancellationToken.None);

        // Assert
        var result = await db.ForecastIncomes.FindAsync(income.Id);
        Assert.False(result!.IsActive);
    }

    [Fact]
    public async Task Handle_ActiveIncome_CancelsPendingOccurrences()
    {
        // Arrange
        using var db = CreateDbContext();
        var income = await SeedActiveForecastIncomeAsync(db);
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = income.Id,
            IsIncome = true,
            Description = "Salary",
            Amount = 1000m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        });
        await db.SaveChangesAsync();
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(new DeleteForecastIncomeDefinitionCommand(income.Id), CancellationToken.None);

        // Assert
        var occurrences = await db.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == income.Id)
            .ToListAsync();
        Assert.All(occurrences, o => Assert.Equal(ForecastOccurrenceStatus.CancelledId, o.ForecastOccurrenceStatusId));
    }

    [Fact]
    public async Task Handle_NotFoundIncome_ThrowsEntityNotFoundException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(new DeleteForecastIncomeDefinitionCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
