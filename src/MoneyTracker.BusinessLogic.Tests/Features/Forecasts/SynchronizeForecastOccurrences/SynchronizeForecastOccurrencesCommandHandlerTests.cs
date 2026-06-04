using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.SynchronizeForecastOccurrences;

public class SynchronizeForecastOccurrencesCommandHandlerTests
{
    private static readonly DateOnly WindowStart = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private static readonly DateOnly WindowEnd = WindowStart.AddMonths(3).AddDays(-1);

    private static MoneyTrackerDbContext CreateDbContext()
    {
        return new MoneyTrackerDbContext(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            null);
    }

    private static async Task<(Guid ExpenseRuleId, Guid IncomeRuleId, Guid CategoryId)> SeedReferenceDataAsync(MoneyTrackerDbContext db)
    {
        var expenseRuleId = Guid.NewGuid();
        var incomeRuleId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        db.ForecastRecurrenceRuleTypes.AddRange(
            new ForecastRecurrenceRuleType { Id = expenseRuleId, Name = "Expense Monthly", Code = ForecastRecurrenceRuleType.Month },
            new ForecastRecurrenceRuleType { Id = incomeRuleId, Name = "Income Monthly", Code = ForecastRecurrenceRuleType.Month });

        db.PaymentCategories.Add(new PaymentCategory
        {
            Id = categoryId,
            Name = "Housing",
            Code = "HSG"
        });

        await db.SaveChangesAsync();
        return (expenseRuleId, incomeRuleId, categoryId);
    }

    [Fact]
    public async Task Handle_ShouldCreateMissingOccurrences_ForActiveForecasts()
    {
        using var db = CreateDbContext();
        var (expenseRuleId, incomeRuleId, categoryId) = await SeedReferenceDataAsync(db);

        var expenseId = Guid.NewGuid();
        var incomeId = Guid.NewGuid();

        db.ForecastExpenses.Add(new ForecastExpense
        {
            Id = expenseId,
            Description = "Rent",
            Amount = 1000m,
            RecurrenceStart = WindowStart,
            RecurrenceEnd = WindowStart,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = expenseRuleId,
            PaymentCategoryId = categoryId,
            IsActive = true
        });

        db.ForecastIncomes.Add(new ForecastIncome
        {
            Id = incomeId,
            Description = "Salary",
            Amount = 2500m,
            RecurrenceStart = WindowStart,
            RecurrenceEnd = WindowStart,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = incomeRuleId,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var handler = new SynchronizeForecastOccurrencesCommandHandler(db);

        await handler.Handle(new SynchronizeForecastOccurrencesCommand(), CancellationToken.None);

        var occurrences = await db.ForecastOccurrences.OrderBy(x => x.IsIncome).ToListAsync();
        Assert.Equal(2, occurrences.Count);

        var expenseOccurrence = Assert.Single(occurrences.Where(x => !x.IsIncome));
        Assert.Equal(expenseId, expenseOccurrence.ForecastDefinitionId);
        Assert.Equal("Rent", expenseOccurrence.Description);
        Assert.Equal(1000m, expenseOccurrence.Amount);
        Assert.Equal(categoryId, expenseOccurrence.PaymentCategoryId);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, expenseOccurrence.ForecastOccurrenceStatusId);

        var incomeOccurrence = Assert.Single(occurrences.Where(x => x.IsIncome));
        Assert.Equal(incomeId, incomeOccurrence.ForecastDefinitionId);
        Assert.Equal("Salary", incomeOccurrence.Description);
        Assert.Equal(2500m, incomeOccurrence.Amount);
        Assert.Null(incomeOccurrence.PaymentCategoryId);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, incomeOccurrence.ForecastOccurrenceStatusId);
    }

    [Fact]
    public async Task Handle_ShouldCancelPendingOccurrence_WhenNoLongerExpected()
    {
        using var db = CreateDbContext();
        var (expenseRuleId, _, categoryId) = await SeedReferenceDataAsync(db);

        var forecastId = Guid.NewGuid();
        var expectedDate = WindowStart.AddDays(2);

        db.ForecastExpenses.Add(new ForecastExpense
        {
            Id = forecastId,
            Description = "Old Rent",
            Amount = 500m,
            RecurrenceStart = WindowStart,
            RecurrenceEnd = WindowStart,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = expenseRuleId,
            PaymentCategoryId = categoryId,
            IsActive = false
        });

        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            IsIncome = false,
            Description = "Old occurrence",
            Amount = 500m,
            ExpectedDate = expectedDate,
            PaymentCategoryId = categoryId,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            ValidatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var handler = new SynchronizeForecastOccurrencesCommandHandler(db);
        await handler.Handle(new SynchronizeForecastOccurrencesCommand(), CancellationToken.None);

        var occurrence = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == forecastId && x.ExpectedDate == expectedDate);
        Assert.Equal(ForecastOccurrenceStatus.CancelledId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_ShouldRefreshPendingOrCancelledOccurrence_FromForecastSeed()
    {
        using var db = CreateDbContext();
        var (expenseRuleId, _, categoryId) = await SeedReferenceDataAsync(db);

        var forecastId = Guid.NewGuid();
        var occurrenceDate = WindowStart;

        db.ForecastExpenses.Add(new ForecastExpense
        {
            Id = forecastId,
            Description = "New Rent",
            Amount = 1200m,
            RecurrenceStart = WindowStart,
            RecurrenceEnd = WindowStart,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = expenseRuleId,
            PaymentCategoryId = categoryId,
            IsActive = true
        });

        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            IsIncome = false,
            Description = "Stale",
            Amount = 1m,
            ExpectedDate = occurrenceDate,
            PaymentCategoryId = null,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId,
            ValidatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var handler = new SynchronizeForecastOccurrencesCommandHandler(db);
        await handler.Handle(new SynchronizeForecastOccurrencesCommand(), CancellationToken.None);

        var occurrence = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == forecastId && x.ExpectedDate == occurrenceDate);
        Assert.Equal("New Rent", occurrence.Description);
        Assert.Equal(1200m, occurrence.Amount);
        Assert.Equal(categoryId, occurrence.PaymentCategoryId);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_ShouldNotChangeConfirmedOccurrence()
    {
        using var db = CreateDbContext();
        var (expenseRuleId, _, categoryId) = await SeedReferenceDataAsync(db);

        var forecastId = Guid.NewGuid();
        var occurrenceDate = WindowStart;

        db.ForecastExpenses.Add(new ForecastExpense
        {
            Id = forecastId,
            Description = "Rent",
            Amount = 2000m,
            RecurrenceStart = WindowStart,
            RecurrenceEnd = WindowStart,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = expenseRuleId,
            PaymentCategoryId = categoryId,
            IsActive = true
        });

        var confirmedDate = DateTime.UtcNow;
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            IsIncome = false,
            Description = "Confirmed Value",
            Amount = 777m,
            ExpectedDate = occurrenceDate,
            PaymentCategoryId = categoryId,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = confirmedDate
        });

        await db.SaveChangesAsync();

        var handler = new SynchronizeForecastOccurrencesCommandHandler(db);
        await handler.Handle(new SynchronizeForecastOccurrencesCommand(), CancellationToken.None);

        var occurrence = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == forecastId && x.ExpectedDate == occurrenceDate);
        Assert.Equal("Confirmed Value", occurrence.Description);
        Assert.Equal(777m, occurrence.Amount);
        Assert.Equal(ForecastOccurrenceStatus.ConfirmedId, occurrence.ForecastOccurrenceStatusId);
        Assert.Equal(confirmedDate, occurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_WithCanceledToken_ShouldThrowOperationCanceledException()
    {
        using var db = CreateDbContext();
        var (expenseRuleId, _, categoryId) = await SeedReferenceDataAsync(db);

        db.ForecastExpenses.Add(new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 100m,
            RecurrenceStart = WindowStart,
            RecurrenceEnd = WindowEnd,
            Interval = 1,
            ForecastRecurrenceRuleTypeId = expenseRuleId,
            PaymentCategoryId = categoryId,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var handler = new SynchronizeForecastOccurrencesCommandHandler(db);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => handler.Handle(new SynchronizeForecastOccurrencesCommand(), cts.Token));
    }
}
