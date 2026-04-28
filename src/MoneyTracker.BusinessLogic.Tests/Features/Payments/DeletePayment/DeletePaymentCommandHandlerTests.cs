using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.DeletePayment;

public class DeletePaymentCommandHandlerTests
{
    private readonly Mock<MoneyTrackerDbContext> _mockDbContext;
    private readonly Mock<DbSet<Payment>> _mockPaymentDbSet;
    private readonly Mock<DbSet<ForecastOccurrence>> _mockForecastOccurrenceDbSet;

    public DeletePaymentCommandHandlerTests()
    {
        _mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>().Options,
            null!);
        _mockPaymentDbSet = new Mock<DbSet<Payment>>();
        _mockForecastOccurrenceDbSet = new Mock<DbSet<ForecastOccurrence>>();
    }

    [Fact]
    public void Constructor_Should_InitializeHandler_When_ValidDbContextProvided()
    {
        // Act
        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_InvalidOccurrenceAction()
    {
        // Arrange
        var command = new DeletePaymentCommand(Guid.NewGuid(), "InvalidAction");
        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Equal("Occurrence action must be Auto, Reopen, or Skip.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_ThrowEntityNotFoundException_When_PaymentNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new DeletePaymentCommand(paymentId, "Auto");

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Contains(paymentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_MarkPaymentAsDeleted_When_PaymentHasNoForecastOccurrence()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = null,
            IsDeleted = false
        };
        var command = new DeletePaymentCommand(paymentId, "Auto");

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == paymentId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(payment.IsDeleted);
        _mockDbContext.Verify(db => db.Payments.Update(payment), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_MarkPaymentAsDeleted_When_ForecastOccurrenceNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var command = new DeletePaymentCommand(paymentId, "Auto");

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((ForecastOccurrence?)null);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(payment.IsDeleted);
        _mockDbContext.Verify(db => db.Payments.Update(payment), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsReopen()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = false,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeletePaymentCommand(paymentId, "Reopen");

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(payment.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToSkipped_When_OccurrenceActionIsSkip()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = false,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeletePaymentCommand(paymentId, "Skip");

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(payment.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsAutoAndFutureDate()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = false,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeletePaymentCommand(paymentId, "Auto");

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(payment.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToSkipped_When_OccurrenceActionIsAutoAndPastDate()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = false,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeletePaymentCommand(paymentId, "Auto");

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(payment.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsAutoAndTodayDate()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = false,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeletePaymentCommand(paymentId, "Auto");

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(payment.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_When_Called()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = null,
            IsDeleted = false
        };
        var command = new DeletePaymentCommand(paymentId, "Auto");
        var cancellationToken = new CancellationToken();

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            cancellationToken))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        _mockDbContext.Verify(db => db.Payments.FindAsync(It.IsAny<object[]>(), cancellationToken), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_HandleNullOccurrenceAction_When_OccurrenceActionIsNull()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = null,
            IsDeleted = false
        };
        var command = new DeletePaymentCommand(paymentId, null);

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(payment.IsDeleted);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_HandleEmptyOccurrenceAction_When_OccurrenceActionIsEmpty()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = null,
            IsDeleted = false
        };
        var command = new DeletePaymentCommand(paymentId, string.Empty);

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePaymentCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(payment.IsDeleted);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
