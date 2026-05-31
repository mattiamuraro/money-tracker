using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;
using MoneyTracker.BusinessLogic.Tests.TestUtilities;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;

public class GetPendingForecastIncomeOccurrencesQueryHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    // ────────────────────────────────────────────────── Empty database ──

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Occurrences_Exist()
    {
        using var db = CreateDbContext();
        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 1), CancellationToken.None);

        Assert.Empty(result);
    }

    // ────────────────────────────────────── Status / expense filtering ──

    [Fact]
    public async Task Handle_Should_Not_Return_Expense_Occurrences()
    {
        using var db = CreateDbContext();
        var expense = new ForecastOccurrenceBuilder()
            .WithIsIncome(false)
            .WithExpectedDate(new DateOnly(2024, 1, 10))
            .WithStatus(ForecastOccurrenceStatus.PendingId)
            .Build();
        db.ForecastOccurrences.Add(expense);
        await db.SaveChangesAsync();

        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 1), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Non_Pending_Occurrences()
    {
        using var db = CreateDbContext();
        var discardedStatusId = Guid.NewGuid();
        var occurrence = new ForecastOccurrenceBuilder()
            .WithIsIncome(true)
            .WithExpectedDate(new DateOnly(2024, 1, 10))
            .WithStatus(discardedStatusId)
            .Build();
        db.ForecastOccurrences.Add(occurrence);
        await db.SaveChangesAsync();

        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 1), CancellationToken.None);

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────── Month boundary ──

    [Fact]
    public async Task Handle_Should_Return_Occurrences_In_Requested_Month()
    {
        using var db = CreateDbContext();
        var inMonth = new ForecastOccurrenceBuilder()
            .WithIsIncome(true)
            .WithDescription("Salary")
            .WithExpectedDate(new DateOnly(2024, 3, 25))
            .WithStatus(ForecastOccurrenceStatus.PendingId)
            .Build();
        db.ForecastOccurrences.Add(inMonth);
        await db.SaveChangesAsync();

        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 3), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Salary", result[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Include_First_Day_Of_Month()
    {
        using var db = CreateDbContext();
        var firstDay = new ForecastOccurrenceBuilder()
            .WithIsIncome(true)
            .WithExpectedDate(new DateOnly(2024, 6, 1))
            .WithStatus(ForecastOccurrenceStatus.PendingId)
            .Build();
        db.ForecastOccurrences.Add(firstDay);
        await db.SaveChangesAsync();

        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 6), CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task Handle_Should_Include_Last_Day_Of_Month()
    {
        using var db = CreateDbContext();
        var lastDay = new ForecastOccurrenceBuilder()
            .WithIsIncome(true)
            .WithExpectedDate(new DateOnly(2024, 6, 30))
            .WithStatus(ForecastOccurrenceStatus.PendingId)
            .Build();
        db.ForecastOccurrences.Add(lastDay);
        await db.SaveChangesAsync();

        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 6), CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Occurrences_From_Previous_Month()
    {
        using var db = CreateDbContext();
        var previousMonth = new ForecastOccurrenceBuilder()
            .WithIsIncome(true)
            .WithExpectedDate(new DateOnly(2024, 2, 28))
            .WithStatus(ForecastOccurrenceStatus.PendingId)
            .Build();
        db.ForecastOccurrences.Add(previousMonth);
        await db.SaveChangesAsync();

        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 3), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Occurrences_From_Next_Month()
    {
        using var db = CreateDbContext();
        var nextMonth = new ForecastOccurrenceBuilder()
            .WithIsIncome(true)
            .WithExpectedDate(new DateOnly(2024, 4, 1))
            .WithStatus(ForecastOccurrenceStatus.PendingId)
            .Build();
        db.ForecastOccurrences.Add(nextMonth);
        await db.SaveChangesAsync();

        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 3), CancellationToken.None);

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────── Ordering ──

    [Fact]
    public async Task Handle_Should_Order_By_ExpectedDate_Then_Description()
    {
        using var db = CreateDbContext();
        db.ForecastOccurrences.AddRange(
            new ForecastOccurrenceBuilder().WithIsIncome(true).WithDescription("Zebra").WithExpectedDate(new DateOnly(2024, 5, 10)).WithStatus(ForecastOccurrenceStatus.PendingId).Build(),
            new ForecastOccurrenceBuilder().WithIsIncome(true).WithDescription("Apple").WithExpectedDate(new DateOnly(2024, 5, 10)).WithStatus(ForecastOccurrenceStatus.PendingId).Build(),
            new ForecastOccurrenceBuilder().WithIsIncome(true).WithDescription("Middle").WithExpectedDate(new DateOnly(2024, 5, 5)).WithStatus(ForecastOccurrenceStatus.PendingId).Build()
        );
        await db.SaveChangesAsync();

        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 5), CancellationToken.None);

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
        var definitionId = Guid.NewGuid();

        var occurrence = new ForecastOccurrenceBuilder()
            .WithIsIncome(true)
            .WithForecastDefinitionId(definitionId)
            .WithDescription("Freelance payment")
            .WithAmount(800m)
            .WithExpectedDate(new DateOnly(2024, 7, 15))
            .WithStatus(ForecastOccurrenceStatus.PendingId)
            .Build();
        db.ForecastOccurrences.Add(occurrence);
        await db.SaveChangesAsync();

        var handler = new GetPendingForecastIncomeOccurrencesQueryHandler(db);

        var result = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(2024, 7), CancellationToken.None);

        Assert.Single(result);
        var dto = result[0];
        Assert.Equal(occurrence.Id, dto.Id);
        Assert.Equal(definitionId, dto.ForecastDefinitionId);
        Assert.Equal("Freelance payment", dto.Description);
        Assert.Equal(800m, dto.Amount);
        Assert.Equal(new DateOnly(2024, 7, 15), dto.ExpectedDate);
    }
}
