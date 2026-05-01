using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.DeleteForecastDefinition;

public class DeleteForecastDefinitionCommandHandlerTests
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
        var handler = new DeleteForecastDefinitionCommandHandler(db);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ActiveExpenseFound_SoftDeletesExpenseAndDeletesOccurrences()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TC"
        };
        dbContext.PaymentCategories.Add(category);

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "Test Expense",
            Amount = 100.00m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            PaymentCategoryId = categoryId
        };
        dbContext.ForecastExpenses.Add(expense);

        var occurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = expenseId,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 100.00m,
            Description = "Test Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new DeleteForecastDefinitionCommandHandler(dbContext);
        var command = new DeleteForecastDefinitionCommand(expenseId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedExpense = await dbContext.ForecastExpenses.FirstOrDefaultAsync(x => x.Id == expenseId);
        Assert.NotNull(updatedExpense);
        Assert.False(updatedExpense.IsActive);

        var updatedOccurrence = await dbContext.ForecastOccurrences.FirstOrDefaultAsync(x => x.Id == occurrence.Id);
        Assert.NotNull(updatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.CancelledId, updatedOccurrence.ForecastOccurrenceStatusId);
    }

    [Fact]
    public async Task Handle_ActiveIncomeFound_SoftDeletesIncomeAndDeletesOccurrences()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var incomeId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        var income = new ForecastIncome
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 200.00m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId
        };
        dbContext.ForecastIncomes.Add(income);

        var occurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = incomeId,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 200.00m,
            Description = "Test Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new DeleteForecastDefinitionCommandHandler(dbContext);
        var command = new DeleteForecastDefinitionCommand(incomeId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedIncome = await dbContext.ForecastIncomes.FirstOrDefaultAsync(x => x.Id == incomeId);
        Assert.NotNull(updatedIncome);
        Assert.False(updatedIncome.IsActive);

        var updatedOccurrence = await dbContext.ForecastOccurrences.FirstOrDefaultAsync(x => x.Id == occurrence.Id);
        Assert.NotNull(updatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.CancelledId, updatedOccurrence.ForecastOccurrenceStatusId);
    }

    [Fact]
    public async Task Handle_NoActiveForecastFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var handler = new DeleteForecastDefinitionCommandHandler(dbContext);
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteForecastDefinitionCommand(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"No active forecast found with ID '{nonExistentId}'.", exception.Message);
    }

    [Fact]
    public async Task Handle_InactiveExpense_ThrowsEntityNotFoundException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = ForecastRecurrenceRuleType.Day
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TC"
        };
        dbContext.PaymentCategories.Add(category);

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "Test Expense",
            Amount = 100.00m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsActive = false,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            PaymentCategoryId = categoryId
        };
        dbContext.ForecastExpenses.Add(expense);

        await dbContext.SaveChangesAsync();

        var handler = new DeleteForecastDefinitionCommandHandler(dbContext);
        var command = new DeleteForecastDefinitionCommand(expenseId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"No active forecast found with ID '{expenseId}'.", exception.Message);
    }

    [Fact]
    public async Task Handle_InactiveIncome_ThrowsEntityNotFoundException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var incomeId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        var income = new ForecastIncome
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 200.00m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsActive = false,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId
        };
        dbContext.ForecastIncomes.Add(income);

        await dbContext.SaveChangesAsync();

        var handler = new DeleteForecastDefinitionCommandHandler(dbContext);
        var command = new DeleteForecastDefinitionCommand(incomeId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"No active forecast found with ID '{incomeId}'.", exception.Message);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var handler = new DeleteForecastDefinitionCommandHandler(dbContext);
        var command = new DeleteForecastDefinitionCommand(Guid.NewGuid());

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_MultiplePendingOccurrences_CancelsAllPendingOccurrences()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TC"
        };
        dbContext.PaymentCategories.Add(category);

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "Test Expense",
            Amount = 100.00m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            PaymentCategoryId = categoryId
        };
        dbContext.ForecastExpenses.Add(expense);

        var occurrence1 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = expenseId,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 100.00m,
            Description = "Test Occurrence 1",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence1);

        var occurrence2 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = expenseId,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Amount = 100.00m,
            Description = "Test Occurrence 2",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence2);

        var occurrence3 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = expenseId,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            Amount = 100.00m,
            Description = "Test Occurrence 3",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence3);

        await dbContext.SaveChangesAsync();

        var handler = new DeleteForecastDefinitionCommandHandler(dbContext);
        var command = new DeleteForecastDefinitionCommand(expenseId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var allOccurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == expenseId)
            .ToListAsync();

        Assert.Equal(3, allOccurrences.Count);
        Assert.All(allOccurrences, occ =>
            Assert.Equal(ForecastOccurrenceStatus.CancelledId, occ.ForecastOccurrenceStatusId));
    }

    [Fact]
    public async Task Handle_NonPendingOccurrences_DoesNotUpdateNonPendingOccurrences()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TC"
        };
        dbContext.PaymentCategories.Add(category);

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "Test Expense",
            Amount = 100.00m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            PaymentCategoryId = categoryId
        };
        dbContext.ForecastExpenses.Add(expense);

        var pendingOccurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = expenseId,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 100.00m,
            Description = "Pending Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(pendingOccurrence);

        var cancelledOccurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = expenseId,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Amount = 100.00m,
            Description = "Already Cancelled Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId
        };
        dbContext.ForecastOccurrences.Add(cancelledOccurrence);

        var validatedOccurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = expenseId,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            Amount = 100.00m,
            Description = "Confirmed Occurrence",
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId
        };
        dbContext.ForecastOccurrences.Add(validatedOccurrence);

        await dbContext.SaveChangesAsync();

        var handler = new DeleteForecastDefinitionCommandHandler(dbContext);
        var command = new DeleteForecastDefinitionCommand(expenseId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedPendingOccurrence = await dbContext.ForecastOccurrences
            .FirstOrDefaultAsync(x => x.Id == pendingOccurrence.Id);
        Assert.NotNull(updatedPendingOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.CancelledId, updatedPendingOccurrence.ForecastOccurrenceStatusId);

        var updatedCancelledOccurrence = await dbContext.ForecastOccurrences
            .FirstOrDefaultAsync(x => x.Id == cancelledOccurrence.Id);
        Assert.NotNull(updatedCancelledOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.CancelledId, updatedCancelledOccurrence.ForecastOccurrenceStatusId);

        var updatedValidatedOccurrence = await dbContext.ForecastOccurrences
            .FirstOrDefaultAsync(x => x.Id == validatedOccurrence.Id);
        Assert.NotNull(updatedValidatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.ConfirmedId, updatedValidatedOccurrence.ForecastOccurrenceStatusId);
    }
}
