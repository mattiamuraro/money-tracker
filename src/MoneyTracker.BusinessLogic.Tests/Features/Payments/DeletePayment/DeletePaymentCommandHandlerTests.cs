using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.DeletePayment;

public class DeletePaymentCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static async Task<(MoneyTrackerDbContext db, Guid paymentId)> SeedPaymentAsync(
        Guid? forecastOccurrenceId = null)
    {
        var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        db.Payments.Add(new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = forecastOccurrenceId,
            IsDeleted = false
        });
        await db.SaveChangesAsync();
        return (db, paymentId);
    }

    private static async Task<(MoneyTrackerDbContext db, Guid paymentId, Guid occurrenceId)> SeedPaymentWithOccurrenceAsync(
        DateOnly expectedDate)
    {
        var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = expectedDate,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = false,
            ForecastDefinitionId = Guid.NewGuid()
        });
        db.Payments.Add(new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        });
        await db.SaveChangesAsync();
        return (db, paymentId, occurrenceId);
    }

    [Fact]
    public void Constructor_Should_InitializeHandler_When_ValidDbContextProvided()
    {
        using var db = CreateDbContext();
        var handler = new DeletePaymentCommandHandler(db);
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_InvalidOccurrenceAction()
    {
        using var db = CreateDbContext();
        var command = new DeletePaymentCommand(Guid.NewGuid(), "InvalidAction");
        var handler = new DeletePaymentCommandHandler(db);

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Occurrence action must be Auto, Reopen, or Skip.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_ThrowEntityNotFoundException_When_PaymentNotFound()
    {
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var command = new DeletePaymentCommand(paymentId, "Auto");
        var handler = new DeletePaymentCommandHandler(db);

        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains(paymentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_MarkPaymentAsDeleted_When_PaymentHasNoForecastOccurrence()
    {
        var (db, paymentId) = await SeedPaymentAsync(forecastOccurrenceId: null);
        var command = new DeletePaymentCommand(paymentId, "Auto");
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        var payment = await db.Payments.FindAsync(paymentId);
        Assert.True(payment!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_MarkPaymentAsDeleted_When_ForecastOccurrenceNotFound()
    {
        var (db, paymentId) = await SeedPaymentAsync(forecastOccurrenceId: Guid.NewGuid());
        var command = new DeletePaymentCommand(paymentId, "Auto");
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        var payment = await db.Payments.FindAsync(paymentId);
        Assert.True(payment!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsReopen()
    {
        var (db, paymentId, occurrenceId) = await SeedPaymentWithOccurrenceAsync(DateOnly.FromDateTime(DateTime.Today.AddDays(5)));
        var command = new DeletePaymentCommand(paymentId, "Reopen");
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        var occurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence!.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        Assert.True((await db.Payments.FindAsync(paymentId))!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToSkipped_When_OccurrenceActionIsSkip()
    {
        var (db, paymentId, occurrenceId) = await SeedPaymentWithOccurrenceAsync(DateOnly.FromDateTime(DateTime.Today.AddDays(5)));
        var command = new DeletePaymentCommand(paymentId, "Skip");
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        var occurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, occurrence!.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        Assert.True((await db.Payments.FindAsync(paymentId))!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsAutoAndFutureDate()
    {
        var (db, paymentId, occurrenceId) = await SeedPaymentWithOccurrenceAsync(DateOnly.FromDateTime(DateTime.Today.AddDays(5)));
        var command = new DeletePaymentCommand(paymentId, "Auto");
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        var occurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence!.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        Assert.True((await db.Payments.FindAsync(paymentId))!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToSkipped_When_OccurrenceActionIsAutoAndPastDate()
    {
        var (db, paymentId, occurrenceId) = await SeedPaymentWithOccurrenceAsync(DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));
        var command = new DeletePaymentCommand(paymentId, "Auto");
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        var occurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, occurrence!.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        Assert.True((await db.Payments.FindAsync(paymentId))!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsAutoAndTodayDate()
    {
        var (db, paymentId, occurrenceId) = await SeedPaymentWithOccurrenceAsync(DateOnly.FromDateTime(DateTime.Today));
        var command = new DeletePaymentCommand(paymentId, "Auto");
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        var occurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence!.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        Assert.True((await db.Payments.FindAsync(paymentId))!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_When_Called()
    {
        var (db, paymentId) = await SeedPaymentAsync(forecastOccurrenceId: null);
        var command = new DeletePaymentCommand(paymentId, "Auto");
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        Assert.True((await db.Payments.FindAsync(paymentId))!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_HandleNullOccurrenceAction_When_OccurrenceActionIsNull()
    {
        var (db, paymentId) = await SeedPaymentAsync(forecastOccurrenceId: null);
        var command = new DeletePaymentCommand(paymentId, null);
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        Assert.True((await db.Payments.FindAsync(paymentId))!.IsDeleted);
    }

    [Fact]
    public async Task Handle_Should_HandleEmptyOccurrenceAction_When_OccurrenceActionIsEmpty()
    {
        var (db, paymentId) = await SeedPaymentAsync(forecastOccurrenceId: null);
        var command = new DeletePaymentCommand(paymentId, string.Empty);
        var handler = new DeletePaymentCommandHandler(db);

        await handler.Handle(command, CancellationToken.None);

        Assert.True((await db.Payments.FindAsync(paymentId))!.IsDeleted);
    }
}
