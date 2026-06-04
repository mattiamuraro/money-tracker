using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Tests.TestUtilities;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.UpdateForecastExpenseDefinition;

public class UpdateForecastExpenseDefinitionCommandHandlerTests
{
    private static UpdateForecastExpenseDefinitionCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new UpdateForecastExpenseDefinitionCommandValidator(), db);

    [Fact]
    public async Task Handle_InvalidCommand_ShouldThrowValidationException()
    {
        using var db = InMemoryDbContextFactory.Create();
        var sut = CreateHandler(db);

        var command = new UpdateForecastExpenseDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = string.Empty,
            Amount = 0,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            RecurrenceEnd = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            Interval = 0,
            PaymentCategoryId = Guid.Empty
        };

        await Assert.ThrowsAsync<ValidationException>(() => sut.Handle(command, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Handle_NotFoundForecast_ShouldThrowEntityNotFoundException()
    {
        using var db = InMemoryDbContextFactory.Create();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        db.ForecastRecurrenceRuleTypes.Add(new ForecastRecurrenceRuleTypeBuilder().WithId(ruleTypeId).Build());
        db.PaymentCategories.Add(new PaymentCategoryBuilder().WithId(categoryId).Build());
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = CreateHandler(db);
        var command = new UpdateForecastExpenseDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            Description = "Updated",
            Amount = 50,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            RecurrenceEnd = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1,
            PaymentCategoryId = categoryId
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.Handle(command, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Handle_NotFoundCategory_ShouldThrowEntityNotFoundException()
    {
        using var db = InMemoryDbContextFactory.Create();
        var ruleTypeId = Guid.NewGuid();
        db.ForecastRecurrenceRuleTypes.Add(new ForecastRecurrenceRuleTypeBuilder().WithId(ruleTypeId).Build());

        var forecastId = Guid.NewGuid();
        db.ForecastExpenses.Add(
            new ForecastExpenseBuilder()
                .WithId(forecastId)
                .WithDescription("Old")
                .WithAmount(20)
                .WithRecurrence(DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today), 1)
                .WithRecurrenceRuleTypeId(ruleTypeId)
                .WithPaymentCategoryId(Guid.NewGuid())
                .Build());

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = CreateHandler(db);
        var command = new UpdateForecastExpenseDefinitionCommand
        {
            Id = forecastId,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            Description = "Updated",
            Amount = 100,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            RecurrenceEnd = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1,
            PaymentCategoryId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.Handle(command, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Handle_ShouldSynchronizeOccurrences_Branches()
    {
        using var db = InMemoryDbContextFactory.Create();
        var ruleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var otherCategoryId = Guid.NewGuid();

        db.ForecastRecurrenceRuleTypes.Add(new ForecastRecurrenceRuleTypeBuilder().WithId(ruleTypeId).Build());
        db.PaymentCategories.AddRange(
            new PaymentCategoryBuilder().WithId(categoryId).Build(),
            new PaymentCategoryBuilder().WithId(otherCategoryId).WithName("Food").WithCode("FOOD").Build());

        var start = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var second = start.AddMonths(1);

        var forecastId = Guid.NewGuid();
        db.ForecastExpenses.Add(
            new ForecastExpenseBuilder()
                .WithId(forecastId)
                .WithDescription("Old")
                .WithAmount(10)
                .WithRecurrence(start, start, 1)
                .WithRecurrenceRuleTypeId(ruleTypeId)
                .WithPaymentCategoryId(categoryId)
                .Build());

        var unrelatedForecastId = Guid.NewGuid();
        db.ForecastOccurrences.AddRange(
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(forecastId)
                .WithIsIncome(false)
                .WithDescription("cancel me")
                .WithAmount(1)
                .WithExpectedDate(start.AddDays(1))
                .WithPaymentCategoryId(categoryId)
                .WithStatus(ForecastOccurrenceStatus.PendingId)
                .WithValidatedAt(DateTime.UtcNow)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(forecastId)
                .WithIsIncome(false)
                .WithDescription("keep confirmed")
                .WithAmount(2)
                .WithExpectedDate(start.AddDays(2))
                .WithPaymentCategoryId(categoryId)
                .WithStatus(ForecastOccurrenceStatus.ConfirmedId)
                .WithValidatedAt(DateTime.UtcNow)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(forecastId)
                .WithIsIncome(false)
                .WithDescription("refresh")
                .WithAmount(3)
                .WithExpectedDate(start)
                .WithPaymentCategoryId(categoryId)
                .WithStatus(ForecastOccurrenceStatus.CancelledId)
                .WithValidatedAt(DateTime.UtcNow)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(unrelatedForecastId)
                .WithIsIncome(false)
                .WithDescription("unrelated")
                .WithExpectedDate(start)
                .WithPaymentCategoryId(categoryId)
                .Build());

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = CreateHandler(db);
        var command = new UpdateForecastExpenseDefinitionCommand
        {
            Id = forecastId,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            Description = "New",
            Amount = 999,
            RecurrenceStart = start,
            RecurrenceEnd = second,
            Interval = 1,
            PaymentCategoryId = otherCategoryId
        };

        await sut.Handle(command, TestContext.Current.CancellationToken);

        var updatedForecast = await db.ForecastExpenses.SingleAsync(x => x.Id == forecastId, TestContext.Current.CancellationToken);
        Assert.Equal("New", updatedForecast.Description);
        Assert.Equal(999, updatedForecast.Amount);
        Assert.Equal(otherCategoryId, updatedForecast.PaymentCategoryId);
        Assert.Equal(second, updatedForecast.RecurrenceEnd);

        var canceled = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == forecastId && x.ExpectedDate == start.AddDays(1), TestContext.Current.CancellationToken);
        Assert.Equal(ForecastOccurrenceStatus.CancelledId, canceled.ForecastOccurrenceStatusId);
        Assert.Null(canceled.ValidatedAt);

        var confirmed = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == forecastId && x.ExpectedDate == start.AddDays(2), TestContext.Current.CancellationToken);
        Assert.Equal(ForecastOccurrenceStatus.ConfirmedId, confirmed.ForecastOccurrenceStatusId);
        Assert.Equal("keep confirmed", confirmed.Description);
        Assert.Equal(2, confirmed.Amount);
        Assert.NotNull(confirmed.ValidatedAt);

        var refreshed = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == forecastId && x.ExpectedDate == start, TestContext.Current.CancellationToken);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, refreshed.ForecastOccurrenceStatusId);
        Assert.Equal("New", refreshed.Description);
        Assert.Equal(999, refreshed.Amount);
        Assert.Equal(otherCategoryId, refreshed.PaymentCategoryId);
        Assert.Null(refreshed.ValidatedAt);

        var added = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == forecastId && x.ExpectedDate == second, TestContext.Current.CancellationToken);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, added.ForecastOccurrenceStatusId);
        Assert.Equal("New", added.Description);
        Assert.Equal(999, added.Amount);
        Assert.Equal(otherCategoryId, added.PaymentCategoryId);

        var unrelated = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == unrelatedForecastId, TestContext.Current.CancellationToken);
        Assert.Equal("unrelated", unrelated.Description);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, unrelated.ForecastOccurrenceStatusId);
    }
}
