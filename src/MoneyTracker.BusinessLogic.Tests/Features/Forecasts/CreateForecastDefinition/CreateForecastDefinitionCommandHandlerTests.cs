using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.CreateForecastDefinition;

public class CreateForecastDefinitionCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static CreateForecastDefinitionCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new CreateForecastDefinitionCommandValidator(), db);

    private static async Task<(Guid UserId, Guid RecurrenceRuleTypeId)> SeedCoreDataAsync(MoneyTrackerDbContext db)
    {
        var userId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        db.Users.Add(new User { Id = userId, Username = "testuser", PasswordHash = "hash" });
        db.ForecastRecurrenceRuleTypes.Add(new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        });

        await db.SaveChangesAsync();
        return (userId, recurrenceRuleTypeId);
    }

    [Fact]
    public void Constructor_ShouldInitialize_AllDependencies()
    {
        // Arrange
        using var db = CreateDbContext();

        // Act
        var handler = CreateHandler(db);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);

        var command = new CreateForecastDefinitionCommand
        {
            Description = "",
            Amount = 100.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_IncomeCommand_CreatesIncome()
    {
        // Arrange
        using var db = CreateDbContext();
        var (_, recurrenceRuleTypeId) = await SeedCoreDataAsync(db);
        var handler = CreateHandler(db);

        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Income",
            Amount = 1000.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        var createdIncome = await db.ForecastIncomes.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(createdIncome);
        Assert.Equal("Test Income", createdIncome.Description);
        Assert.Equal(1000.00m, createdIncome.Amount);
        Assert.True(createdIncome.IsActive);
    }

    [Fact]
    public async Task Handle_ExpenseCommand_CreatesExpense()
    {
        // Arrange
        using var db = CreateDbContext();
        var (_, recurrenceRuleTypeId) = await SeedCoreDataAsync(db);
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TC" });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Expense",
            Amount = 500.00m,
            IsIncome = false,
            PaymentCategoryId = categoryId,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        var createdExpense = await db.ForecastExpenses.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(createdExpense);
        Assert.Equal("Test Expense", createdExpense.Description);
        Assert.Equal(500.00m, createdExpense.Amount);
        Assert.Equal(categoryId, createdExpense.PaymentCategoryId);
        Assert.True(createdExpense.IsActive);
    }

    [Fact]
    public async Task Handle_ExpenseCommandWithInvalidCategory_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = CreateDbContext();
        var (_, recurrenceRuleTypeId) = await SeedCoreDataAsync(db);
        var handler = CreateHandler(db);

        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Expense",
            Amount = 500.00m,
            IsIncome = false,
            PaymentCategoryId = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("The requested payment category does not exist.", exception.Message);
    }

    [Fact]
    public async Task Handle_IncomeCommand_CreatesForecastOccurrences()
    {
        // Arrange
        using var db = CreateDbContext();
        var (_, recurrenceRuleTypeId) = await SeedCoreDataAsync(db);
        var handler = CreateHandler(db);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Income",
            Amount = 1000.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = today,
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdOccurrences = await db.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == result)
            .ToListAsync();

        Assert.NotEmpty(createdOccurrences);
        Assert.All(createdOccurrences, occ =>
        {
            Assert.Equal("Test Income", occ.Description);
            Assert.Equal(1000.00m, occ.Amount);
            Assert.True(occ.IsIncome);
            Assert.Equal(ForecastOccurrenceStatus.PendingId, occ.ForecastOccurrenceStatusId);
        });
    }

    [Fact]
    public async Task Handle_ExpenseCommand_CreatesForecastOccurrences()
    {
        // Arrange
        using var db = CreateDbContext();
        var (_, recurrenceRuleTypeId) = await SeedCoreDataAsync(db);
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TC" });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Expense",
            Amount = 500.00m,
            IsIncome = false,
            PaymentCategoryId = categoryId,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = today,
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdOccurrences = await db.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == result)
            .ToListAsync();

        Assert.NotEmpty(createdOccurrences);
        Assert.All(createdOccurrences, occ =>
        {
            Assert.Equal("Test Expense", occ.Description);
            Assert.Equal(500.00m, occ.Amount);
            Assert.False(occ.IsIncome);
            Assert.Equal(categoryId, occ.PaymentCategoryId);
            Assert.Equal(ForecastOccurrenceStatus.PendingId, occ.ForecastOccurrenceStatusId);
        });
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);

        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cts.Token));
    }

    [Fact]
    public async Task Handle_IncomeCommandWithRecurrenceEnd_CreatesIncome()
    {
        // Arrange
        using var db = CreateDbContext();
        var (_, recurrenceRuleTypeId) = await SeedCoreDataAsync(db);
        var handler = CreateHandler(db);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Income",
            Amount = 1000.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = today,
            RecurrenceEnd = today.AddDays(30),
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        var createdIncome = await db.ForecastIncomes.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(createdIncome);
        Assert.Equal(today.AddDays(30), createdIncome.RecurrenceEnd);
    }

    [Fact]
    public async Task Handle_ExpenseCommandWithRecurrenceEnd_CreatesExpense()
    {
        // Arrange
        using var db = CreateDbContext();
        var (_, recurrenceRuleTypeId) = await SeedCoreDataAsync(db);
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TC" });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Expense",
            Amount = 500.00m,
            IsIncome = false,
            PaymentCategoryId = categoryId,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = today,
            RecurrenceEnd = today.AddDays(30),
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        var createdExpense = await db.ForecastExpenses.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(createdExpense);
        Assert.Equal(today.AddDays(30), createdExpense.RecurrenceEnd);
    }
}
