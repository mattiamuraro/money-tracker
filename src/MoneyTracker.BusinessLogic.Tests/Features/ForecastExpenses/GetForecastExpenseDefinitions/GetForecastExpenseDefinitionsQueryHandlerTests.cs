using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Tests.TestUtilities;
using MoneyTracker.Data.EntityFramework;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.GetForecastExpenseDefinitions;

public class GetForecastExpenseDefinitionsQueryHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static GetForecastExpenseDefinitionsQueryHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(db);

    // ────────────────────────────────────────────────── Empty database ──

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Definitions_Exist()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastExpenseDefinitionsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────── IsActive filtering ──

    [Fact]
    public async Task Handle_Should_Return_Only_Active_Definitions()
    {
        using var db = CreateDbContext();
        var ruleType = new ForecastRecurrenceRuleTypeBuilder().Build();
        db.ForecastRecurrenceRuleTypes.Add(ruleType);

        var category = new PaymentCategoryBuilder().WithName("Bills").Build();
        db.PaymentCategories.Add(category);

        var active = new ForecastExpenseBuilder()
            .WithDescription("Active")
            .WithIsActive(true)
            .WithRecurrenceRuleTypeId(ruleType.Id)
            .WithPaymentCategoryId(category.Id)
            .Build();

        var inactive = new ForecastExpenseBuilder()
            .WithDescription("Inactive")
            .WithIsActive(false)
            .WithRecurrenceRuleTypeId(ruleType.Id)
            .WithPaymentCategoryId(category.Id)
            .Build();

        db.ForecastExpenses.AddRange(active, inactive);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastExpenseDefinitionsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Active", result[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_All_Definitions_Are_Inactive()
    {
        using var db = CreateDbContext();
        var ruleType = new ForecastRecurrenceRuleTypeBuilder().Build();
        db.ForecastRecurrenceRuleTypes.Add(ruleType);

        var category = new PaymentCategoryBuilder().Build();
        db.PaymentCategories.Add(category);

        var inactive = new ForecastExpenseBuilder()
            .WithIsActive(false)
            .WithRecurrenceRuleTypeId(ruleType.Id)
            .WithPaymentCategoryId(category.Id)
            .Build();
        db.ForecastExpenses.Add(inactive);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastExpenseDefinitionsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────── Ordering ──

    [Fact]
    public async Task Handle_Should_Order_By_RecurrenceStart_Then_Description()
    {
        using var db = CreateDbContext();
        var ruleType = new ForecastRecurrenceRuleTypeBuilder().Build();
        db.ForecastRecurrenceRuleTypes.Add(ruleType);

        var category = new PaymentCategoryBuilder().Build();
        db.PaymentCategories.Add(category);

        db.ForecastExpenses.AddRange(
            new ForecastExpenseBuilder()
                .WithDescription("Zebra")
                .WithRecurrence(new DateOnly(2024, 1, 1), null, 1)
                .WithRecurrenceRuleTypeId(ruleType.Id)
                .WithPaymentCategoryId(category.Id)
                .Build(),
            new ForecastExpenseBuilder()
                .WithDescription("Apple")
                .WithRecurrence(new DateOnly(2024, 1, 1), null, 1)
                .WithRecurrenceRuleTypeId(ruleType.Id)
                .WithPaymentCategoryId(category.Id)
                .Build(),
            new ForecastExpenseBuilder()
                .WithDescription("Middle")
                .WithRecurrence(new DateOnly(2023, 6, 1), null, 1)
                .WithRecurrenceRuleTypeId(ruleType.Id)
                .WithPaymentCategoryId(category.Id)
                .Build()
        );
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastExpenseDefinitionsQuery(), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("Middle", result[0].Description);
        Assert.Equal("Apple", result[1].Description);
        Assert.Equal("Zebra", result[2].Description);
    }

    // ──────────────────────────────────────────────── DTO projection ──

    [Fact]
    public async Task Handle_Should_Project_All_Dto_Fields_Correctly()
    {
        using var db = CreateDbContext();
        var ruleType = new ForecastRecurrenceRuleTypeBuilder().Build();
        db.ForecastRecurrenceRuleTypes.Add(ruleType);

        var category = new PaymentCategoryBuilder().WithName("Transport").Build();
        db.PaymentCategories.Add(category);

        var expense = new ForecastExpenseBuilder()
            .WithDescription("Monthly train pass")
            .WithAmount(120m)
            .WithRecurrence(new DateOnly(2023, 1, 1), new DateOnly(2025, 12, 31), 2)
            .WithRecurrenceRuleTypeId(ruleType.Id)
            .WithPaymentCategoryId(category.Id)
            .WithIsActive(true)
            .Build();
        db.ForecastExpenses.Add(expense);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastExpenseDefinitionsQuery(), CancellationToken.None);

        Assert.Single(result);
        var dto = result[0];
        Assert.Equal(expense.Id, dto.Id);
        Assert.Equal(ruleType.Id, dto.ForecastRecurrenceRuleTypeId);
        Assert.Equal("Monthly train pass", dto.Description);
        Assert.Equal(120m, dto.Amount);
        Assert.Equal(new DateOnly(2023, 1, 1), dto.RecurrenceStart);
        Assert.Equal(new DateOnly(2025, 12, 31), dto.RecurrenceEnd);
        Assert.Equal(2, dto.Interval);
        Assert.Equal(category.Id, dto.PaymentCategoryId);
        Assert.Equal("Transport", dto.Category);
    }
}
