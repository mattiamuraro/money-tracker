using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Dashboard.GetDashboardSummary;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Dashboard.GetDashboardSummary;

public class GetDashboardSummaryQueryHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static Payment MakePayment(string description, decimal amount, DateTime date, Guid categoryId) => new()
    {
        Id = Guid.NewGuid(),
        Description = description,
        PaymentCategoryId = categoryId,
        Amount = amount,
        Date = date
    };

    private static ForecastOccurrence MakePendingOccurrence(string description, decimal amount, DateOnly date, bool isIncome) => new()
    {
        Id = Guid.NewGuid(),
        ForecastDefinitionId = Guid.NewGuid(),
        Description = description,
        Amount = amount,
        ExpectedDate = date,
        IsIncome = isIncome,
        ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
    };

    // ────────────────────────────────────────────────────────── Constructor ──

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        using var db = CreateDbContext();

        var handler = new GetDashboardSummaryQueryHandler(db);

        Assert.NotNull(handler);
    }

    // ───────────────────────────────────────────── Empty database behaviour ──

    [Fact]
    public async Task Handle_Should_Return_Zero_Values_When_No_Data_Exists()
    {
        using var db = CreateDbContext();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 1 }, CancellationToken.None);

        Assert.Equal(0, result.PaymentsCount);
        Assert.Equal(0m, result.PaymentTotal);
        Assert.Equal(0m, result.ForecastIncomeTotal);
        Assert.Equal(0m, result.ForecastExpenseTotal);
        Assert.Equal(0m, result.ForecastBalance);
        Assert.Null(result.LatestPaymentDescription);
        Assert.Null(result.LatestPaymentDate);
        Assert.Null(result.NextUpcomingExpenseDescription);
    }

    // ────────────────────────────────────────────── Payment count and total ──

    [Fact]
    public async Task Handle_Should_Count_Only_Payments_Within_Requested_Month()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Payments.AddRange(
            MakePayment("In month", 10m, new DateTime(2024, 3, 15), categoryId),
            MakePayment("Also in month", 20m, new DateTime(2024, 3, 1), categoryId),
            MakePayment("Previous month", 5m, new DateTime(2024, 2, 28), categoryId),
            MakePayment("Next month", 5m, new DateTime(2024, 4, 1), categoryId));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(2, result.PaymentsCount);
    }

    [Fact]
    public async Task Handle_Should_Sum_Payment_Amounts_Only_Within_Requested_Month()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Payments.AddRange(
            MakePayment("A", 100m, new DateTime(2024, 3, 10), categoryId),
            MakePayment("B", 50m, new DateTime(2024, 3, 20), categoryId),
            MakePayment("Outside", 999m, new DateTime(2024, 2, 1), categoryId));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(150m, result.PaymentTotal);
    }

    // ──────────────────────────────────────────────────── Latest payment ─────

    [Fact]
    public async Task Handle_Should_Return_Latest_Payment_Description_By_Date()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Payments.AddRange(
            MakePayment("Older", 10m, new DateTime(2024, 3, 5), categoryId),
            MakePayment("Latest", 20m, new DateTime(2024, 3, 25), categoryId));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal("Latest", result.LatestPaymentDescription);
    }

    [Fact]
    public async Task Handle_Should_Return_Latest_Payment_Date()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        var expectedDate = new DateTime(2024, 3, 25);
        db.Payments.Add(MakePayment("Only payment", 10m, expectedDate, categoryId));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(expectedDate, result.LatestPaymentDate);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_Latest_Payment_When_No_Payments_In_Month()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Payments.Add(MakePayment("Outside", 10m, new DateTime(2024, 2, 10), categoryId));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Null(result.LatestPaymentDescription);
        Assert.Null(result.LatestPaymentDate);
    }

    // ─────────────────────────────────── Pending forecast occurrence totals ──

    [Fact]
    public async Task Handle_Should_Sum_Pending_Forecast_Income_Only_Within_Month()
    {
        using var db = CreateDbContext();
        db.ForecastOccurrences.AddRange(
            MakePendingOccurrence("Salary", 3000m, new DateOnly(2024, 3, 1), isIncome: true),
            MakePendingOccurrence("Bonus", 500m, new DateOnly(2024, 3, 15), isIncome: true),
            MakePendingOccurrence("Other month income", 1000m, new DateOnly(2024, 2, 1), isIncome: true));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(3500m, result.ForecastIncomeTotal);
    }

    [Fact]
    public async Task Handle_Should_Sum_Pending_Forecast_Expense_Only_Within_Month()
    {
        using var db = CreateDbContext();
        db.ForecastOccurrences.AddRange(
            MakePendingOccurrence("Rent", 800m, new DateOnly(2024, 3, 1), isIncome: false),
            MakePendingOccurrence("Utilities", 100m, new DateOnly(2024, 3, 10), isIncome: false),
            MakePendingOccurrence("Outside", 500m, new DateOnly(2024, 4, 1), isIncome: false));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(900m, result.ForecastExpenseTotal);
    }

    [Fact]
    public async Task Handle_Should_Exclude_Non_Pending_Forecast_Occurrences()
    {
        using var db = CreateDbContext();
        db.ForecastOccurrences.AddRange(
            MakePendingOccurrence("Pending", 500m, new DateOnly(2024, 3, 1), isIncome: true),
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Confirmed",
                Amount = 999m,
                ExpectedDate = new DateOnly(2024, 3, 5),
                IsIncome = true,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId
            });
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(500m, result.ForecastIncomeTotal);
    }

    // ───────────────────────────────────────────────────── Forecast balance ──

    [Fact]
    public async Task Handle_Should_Compute_Forecast_Balance_As_Income_Minus_Expense()
    {
        using var db = CreateDbContext();
        db.ForecastOccurrences.AddRange(
            MakePendingOccurrence("Income", 3000m, new DateOnly(2024, 3, 1), isIncome: true),
            MakePendingOccurrence("Expense", 1200m, new DateOnly(2024, 3, 5), isIncome: false));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(1800m, result.ForecastBalance);
    }

    [Fact]
    public async Task Handle_Should_Return_Negative_Forecast_Balance_When_Expenses_Exceed_Income()
    {
        using var db = CreateDbContext();
        db.ForecastOccurrences.AddRange(
            MakePendingOccurrence("Income", 500m, new DateOnly(2024, 3, 1), isIncome: true),
            MakePendingOccurrence("Expense", 1200m, new DateOnly(2024, 3, 5), isIncome: false));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(-700m, result.ForecastBalance);
    }

    // ──────────────────────────────────────── Next upcoming expense ──────────

    [Fact]
    public async Task Handle_Should_Return_Earliest_Pending_Expense_As_Next_Upcoming()
    {
        using var db = CreateDbContext();
        db.ForecastOccurrences.AddRange(
            MakePendingOccurrence("Later expense", 200m, new DateOnly(2024, 3, 20), isIncome: false),
            MakePendingOccurrence("Earliest expense", 100m, new DateOnly(2024, 3, 5), isIncome: false));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal("Earliest expense", result.NextUpcomingExpenseDescription);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Income_As_Next_Upcoming_Expense()
    {
        using var db = CreateDbContext();
        db.ForecastOccurrences.AddRange(
            MakePendingOccurrence("Income occurrence", 3000m, new DateOnly(2024, 3, 1), isIncome: true));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Null(result.NextUpcomingExpenseDescription);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_Next_Upcoming_Expense_When_No_Pending_Expenses()
    {
        using var db = CreateDbContext();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Null(result.NextUpcomingExpenseDescription);
    }

    // ──────────────────────────────────────── Month boundary edge cases ──────

    [Fact]
    public async Task Handle_Should_Include_Payments_On_First_Day_Of_Month()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Payments.Add(MakePayment("First of month", 50m, new DateTime(2024, 3, 1, 0, 0, 0), categoryId));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(1, result.PaymentsCount);
    }

    [Fact]
    public async Task Handle_Should_Exclude_Payments_On_First_Day_Of_Next_Month()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Payments.Add(MakePayment("First of April", 50m, new DateTime(2024, 4, 1, 0, 0, 0), categoryId));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 3 }, CancellationToken.None);

        Assert.Equal(0, result.PaymentsCount);
    }

    [Fact]
    public async Task Handle_Should_Work_Correctly_For_December()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Payments.AddRange(
            MakePayment("Dec payment", 100m, new DateTime(2024, 12, 15), categoryId),
            MakePayment("Jan next year", 100m, new DateTime(2025, 1, 1), categoryId));
        await db.SaveChangesAsync();
        var handler = new GetDashboardSummaryQueryHandler(db);

        var result = await handler.Handle(new GetDashboardSummaryQuery { Year = 2024, Month = 12 }, CancellationToken.None);

        Assert.Equal(1, result.PaymentsCount);
        Assert.Equal(100m, result.PaymentTotal);
    }
}
