using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.CreateForecastExpenseDefinition;

public class CreateForecastExpenseDefinitionCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static CreateForecastExpenseDefinitionCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new CreateForecastExpenseDefinitionCommandValidator(), db);

    private static async Task<(Guid RuleTypeId, Guid CategoryId)> SeedCoreDataAsync(MoneyTrackerDbContext db)
    {
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        db.ForecastRecurrenceRuleTypes.Add(new ForecastRecurrenceRuleType { Id = ruleTypeId, Name = "Monthly", Code = "Month" });
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Housing", Code = "HSG" });
        await db.SaveChangesAsync();
        return (ruleTypeId, categoryId);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesForecastExpense()
    {
        // Arrange
        using var db = CreateDbContext();
        var (ruleTypeId, categoryId) = await SeedCoreDataAsync(db);
        var handler = CreateHandler(db);
        var command = new CreateForecastExpenseDefinitionCommand
        {
            Description = "Rent",
            Amount = 800m,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            PaymentCategoryId = categoryId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var expense = await db.ForecastExpenses.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(expense);
        Assert.Equal("Rent", expense.Description);
        Assert.Equal(800m, expense.Amount);
        Assert.Equal(categoryId, expense.PaymentCategoryId);
        Assert.True(expense.IsActive);
    }

    [Fact]
    public async Task Handle_ValidCommand_GeneratesPendingExpenseOccurrences()
    {
        // Arrange
        using var db = CreateDbContext();
        var (ruleTypeId, categoryId) = await SeedCoreDataAsync(db);
        var handler = CreateHandler(db);
        var command = new CreateForecastExpenseDefinitionCommand
        {
            Description = "Rent",
            Amount = 800m,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            PaymentCategoryId = categoryId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var occurrences = await db.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == result)
            .ToListAsync();
        Assert.All(occurrences, o =>
        {
            Assert.False(o.IsIncome);
            Assert.Equal(ForecastOccurrenceStatus.PendingId, o.ForecastOccurrenceStatusId);
        });
    }

    [Fact]
    public async Task Handle_InvalidCategory_ThrowsEntityNotFoundException()
    {
        // Arrange
        using var db = CreateDbContext();
        var (ruleTypeId, _) = await SeedCoreDataAsync(db);
        var handler = CreateHandler(db);
        var command = new CreateForecastExpenseDefinitionCommand
        {
            Description = "Rent",
            Amount = 800m,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            PaymentCategoryId = Guid.NewGuid()
        };

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        var command = new CreateForecastExpenseDefinitionCommand
        {
            Description = "",
            Amount = 0m,
            Interval = 0,
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            PaymentCategoryId = Guid.Empty
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
