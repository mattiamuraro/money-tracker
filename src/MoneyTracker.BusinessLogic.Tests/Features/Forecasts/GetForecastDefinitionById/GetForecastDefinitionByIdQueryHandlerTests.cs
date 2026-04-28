using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;
using Moq;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.GetForecastDefinitionById;

public class GetForecastDefinitionByIdQueryHandlerTests
{
    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        // Arrange
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>().Options,
            null!);

        // Act
        var handler = new GetForecastDefinitionByIdQueryHandler(mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_Expense_DTO_When_Expense_Exists_And_IsActive()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var expenseId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Monthly",
            Code = ForecastRecurrenceRuleType.Month
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Groceries",
            Code = "GRC"
        };
        dbContext.PaymentCategories.Add(category);

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "Monthly Groceries",
            Amount = 500.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            RecurrenceEnd = new DateOnly(2024, 12, 31),
            IsActive = true,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };
        dbContext.ForecastExpenses.Add(expense);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionByIdQueryHandler(dbContext);
        var query = new GetForecastDefinitionByIdQuery(expenseId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expenseId, result.Id);
        Assert.Equal(ruleTypeId, result.ForecastRecurrenceRuleTypeId);
        Assert.Equal("Monthly Groceries", result.Description);
        Assert.Equal(500.00m, result.Amount);
        Assert.Equal(new DateOnly(2024, 1, 1), result.RecurrenceStart);
        Assert.Equal(new DateOnly(2024, 12, 31), result.RecurrenceEnd);
        Assert.Equal(1, result.Interval);
        Assert.False(result.IsIncome);
        Assert.Equal(categoryId, result.PaymentCategoryId);
    }

    [Fact]
    public async Task Handle_Should_Return_Income_DTO_When_Expense_Not_Found_And_Income_Exists_And_IsActive()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var incomeId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Weekly",
            Code = ForecastRecurrenceRuleType.Week
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);

        var income = new ForecastIncome
        {
            Id = incomeId,
            Description = "Salary",
            Amount = 5000.00m,
            RecurrenceStart = new DateOnly(2024, 1, 15),
            RecurrenceEnd = null,
            IsActive = true,
            Interval = 2,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType
        };
        dbContext.ForecastIncomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionByIdQueryHandler(dbContext);
        var query = new GetForecastDefinitionByIdQuery(incomeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(incomeId, result.Id);
        Assert.Equal(ruleTypeId, result.ForecastRecurrenceRuleTypeId);
        Assert.Equal("Salary", result.Description);
        Assert.Equal(5000.00m, result.Amount);
        Assert.Equal(new DateOnly(2024, 1, 15), result.RecurrenceStart);
        Assert.Null(result.RecurrenceEnd);
        Assert.Equal(2, result.Interval);
        Assert.True(result.IsIncome);
        Assert.Null(result.PaymentCategoryId);
    }

    [Fact]
    public async Task Handle_Should_Throw_EntityNotFoundException_When_Neither_Expense_Nor_Income_Found()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastDefinitionByIdQueryHandler(dbContext);
        var nonExistentId = Guid.NewGuid();
        var query = new GetForecastDefinitionByIdQuery(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(query, CancellationToken.None));

        Assert.Equal($"Forecast with id {nonExistentId} not found.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Expense_When_IsActive_Is_False()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var expenseId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Monthly",
            Code = ForecastRecurrenceRuleType.Month
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Groceries",
            Code = "GRC"
        };
        dbContext.PaymentCategories.Add(category);

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "Inactive Expense",
            Amount = 100.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            RecurrenceEnd = new DateOnly(2024, 12, 31),
            IsActive = false,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };
        dbContext.ForecastExpenses.Add(expense);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionByIdQueryHandler(dbContext);
        var query = new GetForecastDefinitionByIdQuery(expenseId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(query, CancellationToken.None));

        Assert.Equal($"Forecast with id {expenseId} not found.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Income_When_IsActive_Is_False()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var incomeId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "Weekly",
            Code = ForecastRecurrenceRuleType.Week
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);

        var income = new ForecastIncome
        {
            Id = incomeId,
            Description = "Inactive Income",
            Amount = 5000.00m,
            RecurrenceStart = new DateOnly(2024, 1, 15),
            RecurrenceEnd = null,
            IsActive = false,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType
        };
        dbContext.ForecastIncomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionByIdQueryHandler(dbContext);
        var query = new GetForecastDefinitionByIdQuery(incomeId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(query, CancellationToken.None));

        Assert.Equal($"Forecast with id {incomeId} not found.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_Return_Expense_With_Null_Interval_As_1()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var expenseId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "One Time",
            Code = ForecastRecurrenceRuleType.OneTime
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Shopping",
            Code = "SHP"
        };
        dbContext.PaymentCategories.Add(category);

        var expense = new ForecastExpense
        {
            Id = expenseId,
            Description = "One Time Purchase",
            Amount = 250.00m,
            RecurrenceStart = new DateOnly(2024, 6, 1),
            RecurrenceEnd = null,
            IsActive = true,
            Interval = null,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };
        dbContext.ForecastExpenses.Add(expense);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionByIdQueryHandler(dbContext);
        var query = new GetForecastDefinitionByIdQuery(expenseId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Interval);
    }

    [Fact]
    public async Task Handle_Should_Return_Income_With_Null_Interval_As_1()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var incomeId = Guid.NewGuid();
        var ruleTypeId = Guid.NewGuid();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = ruleTypeId,
            Name = "One Time",
            Code = ForecastRecurrenceRuleType.OneTime
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(ruleType);

        var income = new ForecastIncome
        {
            Id = incomeId,
            Description = "Bonus",
            Amount = 1000.00m,
            RecurrenceStart = new DateOnly(2024, 12, 31),
            RecurrenceEnd = null,
            IsActive = true,
            Interval = null,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            ForecastRecurrenceRuleType = ruleType
        };
        dbContext.ForecastIncomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionByIdQueryHandler(dbContext);
        var query = new GetForecastDefinitionByIdQuery(incomeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Interval);
    }

    [Fact]
    public async Task Handle_Should_Respect_Cancellation_Token()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastDefinitionByIdQueryHandler(dbContext);
        var query = new GetForecastDefinitionByIdQuery(Guid.NewGuid());

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_Should_Prioritize_Expense_When_Both_Expense_And_Income_Exist_With_Same_Id()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var sharedId = Guid.NewGuid();
        var expenseRuleTypeId = Guid.NewGuid();
        var incomeRuleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var expenseRuleType = new ForecastRecurrenceRuleType
        {
            Id = expenseRuleTypeId,
            Name = "Monthly",
            Code = ForecastRecurrenceRuleType.Month
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(expenseRuleType);

        var incomeRuleType = new ForecastRecurrenceRuleType
        {
            Id = incomeRuleTypeId,
            Name = "Weekly",
            Code = ForecastRecurrenceRuleType.Week
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(incomeRuleType);

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Entertainment",
            Code = "ENT"
        };
        dbContext.PaymentCategories.Add(category);

        var expense = new ForecastExpense
        {
            Id = sharedId,
            Description = "Expense Record",
            Amount = 100.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            RecurrenceEnd = null,
            IsActive = true,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = expenseRuleTypeId,
            ForecastRecurrenceRuleType = expenseRuleType,
            PaymentCategoryId = categoryId,
            PaymentCategory = category
        };
        dbContext.ForecastExpenses.Add(expense);

        var income = new ForecastIncome
        {
            Id = sharedId,
            Description = "Income Record",
            Amount = 5000.00m,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            RecurrenceEnd = null,
            IsActive = true,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = incomeRuleTypeId,
            ForecastRecurrenceRuleType = incomeRuleType
        };
        dbContext.ForecastIncomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastDefinitionByIdQueryHandler(dbContext);
        var query = new GetForecastDefinitionByIdQuery(sharedId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Expense Record", result.Description);
        Assert.False(result.IsIncome);
        Assert.Equal(categoryId, result.PaymentCategoryId);
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
