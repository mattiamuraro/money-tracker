using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.CreateForecastIncomeDefinition;

public class CreateForecastIncomeDefinitionCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static CreateForecastIncomeDefinitionCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new CreateForecastIncomeDefinitionCommandValidator(), db);

    private static async Task<Guid> SeedRecurrenceRuleTypeAsync(MoneyTrackerDbContext db)
    {
        var id = Guid.NewGuid();
        db.ForecastRecurrenceRuleTypes.Add(new ForecastRecurrenceRuleType { Id = id, Name = "Monthly", Code = "Month" });
        await db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesForecastIncome()
    {
        // Arrange
        using var db = CreateDbContext();
        var ruleTypeId = await SeedRecurrenceRuleTypeAsync(db);
        var handler = CreateHandler(db);
        var command = new CreateForecastIncomeDefinitionCommand
        {
            Description = "Salary",
            Amount = 2000m,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today)
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var income = await db.ForecastIncomes.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(income);
        Assert.Equal("Salary", income.Description);
        Assert.Equal(2000m, income.Amount);
        Assert.True(income.IsActive);
    }

    [Fact]
    public async Task Handle_ValidCommand_GeneratesPendingIncomeOccurrences()
    {
        // Arrange
        using var db = CreateDbContext();
        var ruleTypeId = await SeedRecurrenceRuleTypeAsync(db);
        var handler = CreateHandler(db);
        var command = new CreateForecastIncomeDefinitionCommand
        {
            Description = "Salary",
            Amount = 2000m,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today)
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var occurrences = await db.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == result)
            .ToListAsync();
        Assert.All(occurrences, o =>
        {
            Assert.True(o.IsIncome);
            Assert.Equal(ForecastOccurrenceStatus.PendingId, o.ForecastOccurrenceStatusId);
        });
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        var command = new CreateForecastIncomeDefinitionCommand
        {
            Description = "",
            Amount = 0m,
            Interval = 0,
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
