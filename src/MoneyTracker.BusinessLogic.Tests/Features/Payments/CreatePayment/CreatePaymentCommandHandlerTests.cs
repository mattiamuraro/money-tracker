using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.CreatePayment;

public class CreatePaymentCommandHandlerTests
{
    private readonly Mock<IValidator<CreatePaymentCommand>> _mockValidator;
    private readonly MoneyTrackerDbContext _dbContext;

    public CreatePaymentCommandHandlerTests()
    {
        _mockValidator = new Mock<IValidator<CreatePaymentCommand>>();

        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new MoneyTrackerDbContext(options, null!);
    }

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        // Act
        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldCreatePayment_WhenCommandIsValid()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.50m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: true,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var payment = await _dbContext.Payments.FindAsync(result);
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
        var command = new CreatePaymentCommand(
            description: "",
            paymentCategoryId: Guid.NewGuid(),
            amount: 0,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Description", "Description is required")
        };

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationFailures));

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingPaymentId_WhenIdempotencyKeyExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);

        var existingPaymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = existingPaymentId,
            Description = "Existing Payment",
            PaymentCategoryId = categoryId,
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            IsOneShot = false,
            IdempotencyKey = "test-key-123",
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };
        _dbContext.Payments.Add(existingPayment);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "New Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: true,
            idempotencyKey: "test-key-123",
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(existingPaymentId, result);
        var paymentsCount = await _dbContext.Payments.CountAsync();
        Assert.Equal(1, paymentsCount); // Should not create a new payment
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenPaymentCategoryNotFound()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: Guid.NewGuid(),
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("PaymentCategory with id", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenForecastOccurrenceNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: Guid.NewGuid()
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("ForecastOccurrence with id", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenForecastOccurrenceIsNotPending()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);

        var occurrenceId = Guid.NewGuid();
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = false,
            Description = "Test Forecast",
            Amount = 100.00m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId, // Not pending
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };
        _dbContext.ForecastOccurrences.Add(occurrence);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("ForecastOccurrence with id", exception.Message);
        Assert.Contains("is not pending", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldUpdateForecastOccurrenceStatus_WhenForecastOccurrenceProvided()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);

        var occurrenceId = Guid.NewGuid();
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = false,
            Description = "Test Forecast",
            Amount = 100.00m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };
        _dbContext.ForecastOccurrences.Add(occurrence);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOccurrence = await _dbContext.ForecastOccurrences.FindAsync(occurrenceId);
        Assert.NotNull(updatedOccurrence);
        Assert.Equal(ForecastOccurrenceStatus.ConfirmedId, updatedOccurrence.ForecastOccurrenceStatusId);
        Assert.NotNull(updatedOccurrence.ValidatedAt);
    }

    [Fact]
    public async Task Handle_ShouldSetIdempotencyKeyToNull_WhenEmptyStringProvided()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: "",
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var payment = await _dbContext.Payments.FindAsync(result);
        Assert.NotNull(payment);
        Assert.Null(payment.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldSetIdempotencyKey_WhenNonEmptyStringProvided()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: "my-key-123",
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var payment = await _dbContext.Payments.FindAsync(result);
        Assert.NotNull(payment);
        Assert.Equal("my-key-123", payment.IdempotencyKey);
    }

    [Fact]
    public async Task Handle_ShouldSetAuditFields_WhenCreatingPayment()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var createdById = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: createdById,
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var payment = await _dbContext.Payments.FindAsync(result);
        Assert.NotNull(payment);
        Assert.Equal(createdById, payment.CreatedById);
        Assert.Equal(createdById, payment.ModifiedById);
        Assert.NotEqual(default, payment.CreatedAt);
        Assert.NotEqual(default, payment.ModifiedAt);
    }

    [Fact]
    public async Task Handle_ShouldNotCheckIdempotency_WhenIdempotencyKeyIsNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var payment = await _dbContext.Payments.FindAsync(result);
        Assert.NotNull(payment);
    }

    [Fact]
    public async Task Handle_ShouldLinkPaymentToForecastOccurrence_WhenForecastOccurrenceProvided()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);

        var occurrenceId = Guid.NewGuid();
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = false,
            Description = "Test Forecast",
            Amount = 100.00m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };
        _dbContext.ForecastOccurrences.Add(occurrence);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: occurrenceId
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var payment = await _dbContext.Payments.FindAsync(result);
        Assert.NotNull(payment);
        Assert.Equal(occurrenceId, payment.ForecastOccurrenceId);
    }

    [Fact]
    public async Task Handle_ShouldSkipIncomeOccurrences_WhenFilteringForecastOccurrences()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);

        var incomeOccurrenceId = Guid.NewGuid();
        var incomeOccurrence = new ForecastOccurrence
        {
            Id = incomeOccurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            IsIncome = true, // This is an income
            Description = "Test Income",
            Amount = 100.00m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };
        _dbContext.ForecastOccurrences.Add(incomeOccurrence);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: incomeOccurrenceId
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("ForecastOccurrence with id", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_WhenCalled()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        var cancellationToken = new CancellationToken();

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        _mockValidator.Verify(v => v.ValidateAndThrowAsync(command, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChangesToDatabase_WhenPaymentIsCreated()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);
        var initialCount = await _dbContext.Payments.CountAsync();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var finalCount = await _dbContext.Payments.CountAsync();
        Assert.Equal(initialCount + 1, finalCount);
    }

    [Fact]
    public async Task Handle_ShouldCallValidateAndThrowAsync_WhenHandleCalled()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TEST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePaymentCommand(
            description: "Test Payment",
            paymentCategoryId: categoryId,
            amount: 100.00m,
            date: DateTime.UtcNow,
            createdById: Guid.NewGuid(),
            isOneShot: false,
            idempotencyKey: null,
            forecastOccurrenceId: null
        );

        _mockValidator
            .Setup(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockValidator.Verify(v => v.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
