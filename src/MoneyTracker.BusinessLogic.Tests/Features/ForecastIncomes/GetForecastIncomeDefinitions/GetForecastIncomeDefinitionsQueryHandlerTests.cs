using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Tests.TestUtilities;
using MoneyTracker.Data.EntityFramework;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.GetForecastIncomeDefinitions;

public class GetForecastIncomeDefinitionsQueryHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static GetForecastIncomeDefinitionsQueryHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(db);

    // ────────────────────────────────────────────────── Empty database ──

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Definitions_Exist()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastIncomeDefinitionsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────── IsActive filtering ──

    [Fact]
    public async Task Handle_Should_Return_Only_Active_Definitions()
    {
        using var db = CreateDbContext();
        var ruleType = new ForecastRecurrenceRuleTypeBuilder().Build();
        db.ForecastRecurrenceRuleTypes.Add(ruleType);

        var active = new ForecastIncomeBuilder()
            .WithDescription("Active salary")
            .WithIsActive(true)
            .WithRecurrenceRuleTypeId(ruleType.Id)
            .Build();

        var inactive = new ForecastIncomeBuilder()
            .WithDescription("Old bonus")
            .WithIsActive(false)
            .WithRecurrenceRuleTypeId(ruleType.Id)
            .Build();

        db.ForecastIncomes.AddRange(active, inactive);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastIncomeDefinitionsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Active salary", result[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_All_Definitions_Are_Inactive()
    {
        using var db = CreateDbContext();
        var ruleType = new ForecastRecurrenceRuleTypeBuilder().Build();
        db.ForecastRecurrenceRuleTypes.Add(ruleType);

        var inactive = new ForecastIncomeBuilder()
            .WithIsActive(false)
            .WithRecurrenceRuleTypeId(ruleType.Id)
            .Build();
        db.ForecastIncomes.Add(inactive);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastIncomeDefinitionsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────── Ordering ──

    [Fact]
    public async Task Handle_Should_Order_By_RecurrenceStart_Then_Description()
    {
        using var db = CreateDbContext();
        var ruleType = new ForecastRecurrenceRuleTypeBuilder().Build();
        db.ForecastRecurrenceRuleTypes.Add(ruleType);

        db.ForecastIncomes.AddRange(
            new ForecastIncomeBuilder()
                .WithDescription("Zebra")
                .WithRecurrence(new DateOnly(2024, 1, 1), null, 1)
                .WithRecurrenceRuleTypeId(ruleType.Id)
                .Build(),
            new ForecastIncomeBuilder()
                .WithDescription("Apple")
                .WithRecurrence(new DateOnly(2024, 1, 1), null, 1)
                .WithRecurrenceRuleTypeId(ruleType.Id)
                .Build(),
            new ForecastIncomeBuilder()
                .WithDescription("Middle")
                .WithRecurrence(new DateOnly(2023, 6, 1), null, 1)
                .WithRecurrenceRuleTypeId(ruleType.Id)
                .Build()
        );
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastIncomeDefinitionsQuery(), CancellationToken.None);

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

        var income = new ForecastIncomeBuilder()
            .WithDescription("Monthly salary")
            .WithAmount(3000m)
            .WithRecurrence(new DateOnly(2022, 1, 1), new DateOnly(2026, 12, 31), 3)
            .WithRecurrenceRuleTypeId(ruleType.Id)
            .WithIsActive(true)
            .Build();
        db.ForecastIncomes.Add(income);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastIncomeDefinitionsQuery(), CancellationToken.None);

        Assert.Single(result);
        var dto = result[0];
        Assert.Equal(income.Id, dto.Id);
        Assert.Equal(ruleType.Id, dto.ForecastRecurrenceRuleTypeId);
        Assert.Equal("Monthly salary", dto.Description);
        Assert.Equal(3000m, dto.Amount);
        Assert.Equal(new DateOnly(2022, 1, 1), dto.RecurrenceStart);
        Assert.Equal(new DateOnly(2026, 12, 31), dto.RecurrenceEnd);
        Assert.Equal(3, dto.Interval);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_RecurrenceEnd_When_Open_Ended()
    {
        using var db = CreateDbContext();
        var ruleType = new ForecastRecurrenceRuleTypeBuilder().Build();
        db.ForecastRecurrenceRuleTypes.Add(ruleType);

        var income = new ForecastIncomeBuilder()
            .WithRecurrence(new DateOnly(2024, 1, 1), null, 1)
            .WithRecurrenceRuleTypeId(ruleType.Id)
            .Build();
        db.ForecastIncomes.Add(income);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new GetForecastIncomeDefinitionsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Null(result[0].RecurrenceEnd);
    }
}
