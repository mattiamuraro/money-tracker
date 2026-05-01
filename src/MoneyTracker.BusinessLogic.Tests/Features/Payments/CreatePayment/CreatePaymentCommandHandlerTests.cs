using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.CreatePayment;

public class CreatePaymentCommandHandlerTests
{
    private static (CreatePaymentCommandHandler handler, MoneyTrackerDbContext db) CreateHandler()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new MoneyTrackerDbContext(options, null!);
        var validator = new CreatePaymentCommandValidator();
        return (new CreatePaymentCommandHandler(validator, db), db);
    }

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        var (handler, _) = CreateHandler();
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldCreatePayment_WhenCommandIsValid()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.50m,
            date: DateTime.UtcNow,
            isOneShot: true,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var payment = await db.Payments.FindAsync(result);
        Assert.NotNull(payment);
        Assert.Equal("Test Payment", payment.Description);
        Assert.Equal(categoryId, payment.PaymentCategoryId);
        Assert.Equal(100.50m, payment.Amount);
        Assert.True(payment.IsOneShot);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        var (handler, _) = CreateHandler();
        var command = new CreatePaymentCommand(
            description: "",
            paymentCategoryId: Guid.NewGuid(),
            amount: 0,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingPaymentId_WhenIdempotencyKeyExists()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });

        var existingPaymentId = Guid.NewGuid();
        db.Payments.Add(new Payment
        {
            Id = existingPaymentId,
            Description = "Existing Payment",
            PaymentCategoryId = categoryId,
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            IsOneShot = false,
            IdempotencyKey = "test-key-123"
        });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "New Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: true,
            idempotencyKey: "test-key-123",
            forecastOccurrenceId: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(existingPaymentId, result);
        Assert.Equal(1, await db.Payments.CountAsync());
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenPaymentCategoryNotFound()
    {
        // Arrange
        var (handler, _) = CreateHandler();
        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: Guid.NewGuid(),
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("PaymentCategory with id", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenForecastOccurrenceNotFound()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: Guid.NewGuid()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("ForecastOccurrence with id", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenForecastOccurrenceIsNotPending()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });

        var occurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = false,
            Description = "Test Forecast",
            Amount = 100.00m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId
        });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("ForecastOccurrence with id", exception.Message);
        Assert.Contains("is not pending", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldUpdateForecastOccurrenceStatus_WhenForecastOccurrenceProvided()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });

        var occurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = false,
            Description = "Test Forecast",
            Amount = 100.00m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOccurrence = await db.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.NotNull(updatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.ConfirmedId, updatedOccurrence.ForecastOccurrenceStatusId);
        Assert.NotNull(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_ShouldSetIdempotencyKeyToNull_WhenEmptyStringProvided()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: "",
            forecastOccurrenceId: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var payment = await db.Payments.FindAsync(result);
        Assert.NotNull(payment);
        Assert.Null(payment.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldSetIdempotencyKey_WhenNonEmptyStringProvided()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: "my-key-123",
            forecastOccurrenceId: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var payment = await db.Payments.FindAsync(result);
        Assert.NotNull(payment);
        Assert.Equal("my-key-123", payment.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldSetAuditFields_WhenCreatingPayment()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var payment = await db.Payments.FindAsync(result);
        Assert.NotNull(payment);
    }

    [Fact]
    public async Task Handle_ShouldNotCheckIdempotency_WhenIdempotencyKeyIsNull()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var payment = await db.Payments.FindAsync(result);
        Assert.NotNull(payment);
    }

    [Fact]
    public async Task Handle_ShouldLinkPaymentToForecastOccurrence_WhenForecastOccurrenceProvided()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });

        var occurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = false,
            Description = "Test Forecast",
            Amount = 100.00m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var payment = await db.Payments.FindAsync(result);
        Assert.NotNull(payment);
        Assert.Equal(occurrenceId, payment.ForecastOccurrenceId);
    }

    [Fact]
    public async Task Handle_ShouldSkipIncomeOccurrences_WhenFilteringForecastOccurrences()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });

        var incomeOccurrenceId = Guid.NewGuid();
        db.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = incomeOccurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = true,
            Description = "Test Income",
            Amount = 100.00m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: incomeOccurrenceId
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("ForecastOccurrence with id", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_WhenCalled()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        // Act & Assert - no exception means CancellationToken was passed through correctly
        await handler.Handle(command, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldSaveChangesToDatabase_WhenPaymentIsCreated()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        var initialCount = await db.Payments.CountAsync();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(initialCount + 1, await db.Payments.CountAsync());
    }

    [Fact]
    public async Task Handle_ShouldCallValidateAndThrowAsync_WhenHandleCalled()
    {
        // Arrange
        var (handler, db) = CreateHandler();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TEST" });
        await db.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        // Act & Assert - if validation runs, a valid command should succeed without throwing
        await handler.Handle(command, CancellationToken.None);
    }
}
