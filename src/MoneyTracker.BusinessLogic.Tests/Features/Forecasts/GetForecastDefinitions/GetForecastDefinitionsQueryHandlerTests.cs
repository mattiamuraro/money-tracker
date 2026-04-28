using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitions;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.GetForecastDefinitions;

public class GetForecastDefinitionsQueryHandlerTests
{
    [Fact]
    public void Constructor_Should_Set_DbContext()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();

        // Act
        var handler = new GetForecastDefinitionsQueryHandler(dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Collection_When_No_Forecasts_Exist()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Active_Expenses()
    {
        // Arrange
        var expenseId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Monthly",
            Code = "M",
            OrderIndex = 1
        };

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Groceries",
            Code = "GRC"
        };

        var activeExpense = new ForecastExpense
        {
            Id = expenseId,
            Description = "Active Expense",
            Amount = 100.50m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            RecurrenceEnd = new DateOnly(2024, 12, 31),
            Interval = 2,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };

        var inactiveExpense = new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "Inactive Expense",
            Amount = 50.25m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            Interval = 1,
            IsActive = false,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };

        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);
        dbContext.PaymentCategories.Add(category);
        dbContext.ForecastExpenses.AddRange(activeExpense, inactiveExpense);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(expenseId, result[0].Id);
        Assert.Equal("Active Expense", result[0].Description);
        Assert.Equal(100.50m, result[0].Amount);
        Assert.Equal(new DateOnly(2024, 1, 1), result[0].RecurrenceStart);
        Assert.Equal(new DateOnly(2024, 12, 31), result[0].RecurrenceEnd);
        Assert.Equal(2, result[0].Interval);
        Assert.False(result[0].IsIncome);
        Assert.Equal(categoryId, result[0].PaymentCategoryId);
        Assert.Equal("Groceries", result[0].Category);
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Active_Incomes()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Weekly",
            Code = "W",
            OrderIndex = 1
        };

        var activeIncome = new ForecastIncome
        {
            Id = incomeId,
            Description = "Active Income",
            Amount = 2000.00m,
            RecurrenceStart = new DateOnly(2024, 2, 1),
            RecurrenceEnd = new DateOnly(2024, 12, 31),
            Interval = 3,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType
        };

        var inactiveIncome = new ForecastIncome
        {
            Id = Guid.NewGuid(),
            Description = "Inactive Income",
            Amount = 1000.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            Interval = 1,
            IsActive = false,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType
        };

        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);
        dbContext.ForecastIncomes.AddRange(activeIncome, inactiveIncome);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(incomeId, result[0].Id);
        Assert.Equal("Active Income", result[0].Description);
        Assert.Equal(2000.00m, result[0].Amount);
        Assert.Equal(new DateOnly(2024, 2, 1), result[0].RecurrenceStart);
        Assert.Equal(new DateOnly(2024, 12, 31), result[0].RecurrenceEnd);
        Assert.Equal(3, result[0].Interval);
        Assert.True(result[0].IsIncome);
        Assert.Null(result[0].PaymentCategoryId);
        Assert.Null(result[0].Category);
    }

    [Fact]
    public async Task Handle_Should_Return_Both_Expenses_And_Incomes()
    {
        // Arrange
        var expenseId = Guid.NewGuid();
        var incomeId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Monthly",
            Code = "M",
            OrderIndex = 1
        };

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Utilities",
            Code = "UTL"
        };

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "Expense",
            Amount = 100.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            Interval = 1,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };

        var income = new ForecastIncome
        {
            Id = incomeId,
            Description = "Income",
            Amount = 2000.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            Interval = 1,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType
        };

        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);
        dbContext.PaymentCategories.Add(category);
        dbContext.ForecastExpenses.Add(expense);
        dbContext.ForecastIncomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == expenseId && !r.IsIncome);
        Assert.Contains(result, r => r.Id == incomeId && r.IsIncome);
    }

    [Fact]
    public async Task Handle_Should_Order_By_RecurrenceStart_Then_Description()
    {
        // Arrange
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Monthly",
            Code = "M",
            OrderIndex = 1
        };

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "General",
            Code = "GEN"
        };

        // Different dates and descriptions to test ordering
        var expense1 = new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "Z Description",
            Amount = 100.00m,
            RecurrenceStart = new DateOnly(2024, 2, 1),
            Interval = 1,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };

        var expense2 = new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "A Description",
            Amount = 200.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            Interval = 1,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };

        var income1 = new ForecastIncome
        {
            Id = Guid.NewGuid(),
            Description = "B Income",
            Amount = 1000.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            Interval = 1,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType
        };

        var income2 = new ForecastIncome
        {
            Id = Guid.NewGuid(),
            Description = "A Income",
            Amount = 2000.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            Interval = 1,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType
        };

        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);
        dbContext.PaymentCategories.Add(category);
        dbContext.ForecastExpenses.AddRange(expense1, expense2);
        dbContext.ForecastIncomes.AddRange(income1, income2);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        // First by RecurrenceStart, then by Description
        Assert.Equal("A Description", result[0].Description); // 2024-01-01, A
        Assert.Equal("A Income", result[1].Description);     // 2024-01-01, A
        Assert.Equal("B Income", result[2].Description);     // 2024-01-01, B
        Assert.Equal("Z Description", result[3].Description); // 2024-02-01, Z
    }

    [Fact]
    public async Task Handle_Should_Handle_Null_Interval()
    {
        // Arrange
        var expenseId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "OneTime",
            Code = "O",
            OrderIndex = 1
        };

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "OneTime",
            Code = "ONE"
        };

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "No Interval",
            Amount = 100.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            Interval = null,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };

        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);
        dbContext.PaymentCategories.Add(category);
        dbContext.ForecastExpenses.Add(expense);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].Interval); // Should default to 1
    }

    [Fact]
    public async Task Handle_Should_Handle_Null_RecurrenceEnd()
    {
        // Arrange
        var expenseId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Monthly",
            Code = "M",
            OrderIndex = 1
        };

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Recurring",
            Code = "REC"
        };

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "No End Date",
            Amount = 100.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            RecurrenceEnd = null,
            Interval = 1,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };

        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);
        dbContext.PaymentCategories.Add(category);
        dbContext.ForecastExpenses.Add(expense);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Null(result[0].RecurrenceEnd);
    }

    [Fact]
    public async Task Handle_Should_Respect_Cancellation_Token()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_Should_Include_ForecastRecurrenceRuleTypeId()
    {
        // Arrange
        var expenseId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Daily",
            Code = "D",
            OrderIndex = 1
        };

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD"
        };

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "Daily Expense",
            Amount = 10.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            Interval = 1,
            IsActive = true,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };

        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);
        dbContext.PaymentCategories.Add(category);
        dbContext.ForecastExpenses.Add(expense);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(ruleTypeId, result[0].ForecastRecurrenceRuleTypeId);
    }

    [Fact]
    public async Task Handle_Should_Return_Multiple_Expenses_And_Incomes()
    {
        // Arrange
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Monthly",
            Code = "M",
            OrderIndex = 1
        };

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Various",
            Code = "VAR"
        };

        var expenses = new[]
        {
            new ForecastExpense
            {
                Id = Guid.NewGuid(),
                Description = "Expense 1",
                Amount = 100.00m,
                RecurrenceStart = new DateOnly(2024, 1, 1),
                Interval = 1,
                IsActive = true,
                ForecastRecurrenceRuleTypeId = ruleTypeId,
                ForecastRecurrenceRuleType = ruleType,
                PaymentCategoryId = categoryId,
                PaymentCategory = category
            },
            new ForecastExpense
            {
                Id = Guid.NewGuid(),
                Description = "Expense 2",
                Amount = 200.00m,
                RecurrenceStart = new DateOnly(2024, 1, 1),
                Interval = 1,
                IsActive = true,
                ForecastRecurrenceRuleTypeId = ruleTypeId,
                ForecastRecurrenceRuleType = ruleType,
                PaymentCategoryId = categoryId,
                PaymentCategory = category
            },
            new ForecastExpense
            {
                Id = Guid.NewGuid(),
                Description = "Expense 3",
                Amount = 300.00m,
                RecurrenceStart = new DateOnly(2024, 1, 1),
                Interval = 1,
                IsActive = true,
                ForecastRecurrenceRuleTypeId = ruleTypeId,
                ForecastRecurrenceRuleType = ruleType,
                PaymentCategoryId = categoryId,
                PaymentCategory = category
            }
        };

        var incomes = new[]
        {
            new ForecastIncome
            {
                Id = Guid.NewGuid(),
                Description = "Income 1",
                Amount = 1000.00m,
                RecurrenceStart = new DateOnly(2024, 1, 1),
                Interval = 1,
                IsActive = true,
                ForecastRecurrenceRuleTypeId = ruleTypeId,
                ForecastRecurrenceRuleType = ruleType
            },
            new ForecastIncome
            {
                Id = Guid.NewGuid(),
                Description = "Income 2",
                Amount = 2000.00m,
                RecurrenceStart = new DateOnly(2024, 1, 1),
                Interval = 1,
                IsActive = true,
                ForecastRecurrenceRuleTypeId = ruleTypeId,
                ForecastRecurrenceRuleType = ruleType
            }
        };

        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);
        dbContext.PaymentCategories.Add(category);
        dbContext.ForecastExpenses.AddRange(expenses);
        dbContext.ForecastIncomes.AddRange(incomes);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionsQueryHandler(dbContext);
        var query = new GetForecastDefinitionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Equal(3, result.Count(r => !r.IsIncome));
        Assert.Equal(2, result.Count(r => r.IsIncome));
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
