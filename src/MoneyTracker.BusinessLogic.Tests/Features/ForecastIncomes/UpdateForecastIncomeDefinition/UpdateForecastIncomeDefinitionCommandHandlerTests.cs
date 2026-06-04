using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Tests.TestUtilities;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.UpdateForecastIncomeDefinition;

public class UpdateForecastIncomeDefinitionCommandHandlerTests
{
    private static UpdateForecastIncomeDefinitionCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new UpdateForecastIncomeDefinitionCommandValidator(), db);

    [Fact]
    public async Task Handle_InvalidCommand_ShouldThrowValidationException()
    {
        using var db = InMemoryDbContextFactory.Create();
        var sut = CreateHandler(db);

        var command = new UpdateForecastIncomeDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = string.Empty,
            Amount = 0,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            RecurrenceEnd = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            Interval = 0
        };

        await Assert.ThrowsAsync<ValidationException>(() => sut.Handle(command, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Handle_NotFoundForecast_ShouldThrowEntityNotFoundException()
    {
        using var db = InMemoryDbContextFactory.Create();
        var ruleTypeId = Guid.NewGuid();
        db.ForecastRecurrenceRuleTypes.Add(new ForecastRecurrenceRuleTypeBuilder().WithId(ruleTypeId).Build());
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = CreateHandler(db);
        var command = new UpdateForecastIncomeDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            Description = "Updated",
            Amount = 3000,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            RecurrenceEnd = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.Handle(command, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Handle_ShouldSynchronizeOccurrences_Branches()
    {
        using var db = InMemoryDbContextFactory.Create();
        var ruleTypeId = Guid.NewGuid();
        db.ForecastRecurrenceRuleTypes.Add(new ForecastRecurrenceRuleTypeBuilder().WithId(ruleTypeId).Build());

        var start = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var second = start.AddMonths(1);

        var forecastId = Guid.NewGuid();
        db.ForecastIncomes.Add(
            new ForecastIncomeBuilder()
                .WithId(forecastId)
                .WithDescription("Old salary")
                .WithAmount(1000)
                .WithRecurrence(start, start, 1)
                .WithRecurrenceRuleTypeId(ruleTypeId)
                .Build());

        var unrelatedForecastId = Guid.NewGuid();
        db.ForecastOccurrences.AddRange(
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(forecastId)
                .WithIsIncome(true)
                .WithDescription("cancel me")
                .WithAmount(1)
                .WithExpectedDate(start.AddDays(1))
                .WithStatus(ForecastOccurrenceStatus.PendingId)
                .WithValidatedAt(DateTime.UtcNow)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(forecastId)
                .WithIsIncome(true)
                .WithDescription("keep confirmed")
                .WithAmount(2)
                .WithExpectedDate(start.AddDays(2))
                .WithStatus(ForecastOccurrenceStatus.ConfirmedId)
                .WithValidatedAt(DateTime.UtcNow)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(forecastId)
                .WithIsIncome(true)
                .WithDescription("refresh")
                .WithAmount(3)
                .WithExpectedDate(start)
                .WithStatus(ForecastOccurrenceStatus.CancelledId)
                .WithValidatedAt(DateTime.UtcNow)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(unrelatedForecastId)
                .WithIsIncome(true)
                .WithDescription("unrelated")
                .WithAmount(77)
                .WithExpectedDate(start)
                .WithStatus(ForecastOccurrenceStatus.PendingId)
                .Build());

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = CreateHandler(db);
        var command = new UpdateForecastIncomeDefinitionCommand
        {
            Id = forecastId,
            ForecastRecurrenceRuleTypeId = ruleTypeId,
            Description = "New salary",
            Amount = 4500,
            RecurrenceStart = start,
            RecurrenceEnd = second,
            Interval = 1
        };

        await sut.Handle(command, TestContext.Current.CancellationToken);

        var updatedForecast = await db.ForecastIncomes.SingleAsync(x => x.Id == forecastId, TestContext.Current.CancellationToken);
        Assert.Equal("New salary", updatedForecast.Description);
        Assert.Equal(4500, updatedForecast.Amount);
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
        Assert.Equal("New salary", refreshed.Description);
        Assert.Equal(4500, refreshed.Amount);
        Assert.Null(refreshed.ValidatedAt);

        var added = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == forecastId && x.ExpectedDate == second, TestContext.Current.CancellationToken);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, added.ForecastOccurrenceStatusId);
        Assert.True(added.IsIncome);
        Assert.Equal("New salary", added.Description);
        Assert.Equal(4500, added.Amount);

        var unrelated = await db.ForecastOccurrences.SingleAsync(x => x.ForecastDefinitionId == unrelatedForecastId, TestContext.Current.CancellationToken);
        Assert.Equal("unrelated", unrelated.Description);
        Assert.Equal(77, unrelated.Amount);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, unrelated.ForecastOccurrenceStatusId);
    }
}
