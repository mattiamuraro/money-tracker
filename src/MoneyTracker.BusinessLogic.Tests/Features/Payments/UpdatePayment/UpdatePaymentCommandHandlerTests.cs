using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.UpdatePayment;

public class UpdatePaymentCommandHandlerTests
{
    private readonly Mock<IValidator<UpdatePaymentCommand>> _mockValidator;
    private readonly Mock<MoneyTrackerDbContext> _mockDbContext;

    public UpdatePaymentCommandHandlerTests()
    {
        _mockValidator = new Mock<IValidator<UpdatePaymentCommand>>();
        _mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>().Options,
            null!);
    }

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        // Act
        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_When_ValidationFails()
    {
        // Arrange
        var command = new UpdatePaymentCommand(Guid.NewGuid(), Guid.NewGuid());
        var validationFailure = new ValidationFailure("PaymentId", "Invalid payment id");
        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_ThrowEntityNotFoundException_When_PaymentNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new UpdatePaymentCommand(paymentId, Guid.NewGuid());

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Contains(paymentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_When_PaymentCategoryNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Old Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var command = new UpdatePaymentCommand(
            paymentId,
            Guid.NewGuid(),
            paymentCategoryId: categoryId);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var mockPaymentCategoryDbSet = new Mock<DbSet<PaymentCategory>>();
        _mockDbContext.Setup(db => db.PaymentCategories)
            .Returns(mockPaymentCategoryDbSet.Object);

        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<PaymentCategory>(new List<PaymentCategory>().AsQueryable().Provider));
        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.Expression)
            .Returns(new List<PaymentCategory>().AsQueryable().Expression);
        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.ElementType)
            .Returns(new List<PaymentCategory>().AsQueryable().ElementType);
        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.GetEnumerator())
            .Returns(new List<PaymentCategory>().GetEnumerator());

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Contains(categoryId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_UpdateAllProperties_When_AllPropertiesProvided()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Old Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var newDescription = "New Description";
        var newAmount = 100m;
        var newDate = DateTime.UtcNow;
        var command = new UpdatePaymentCommand(
            paymentId,
            modifiedById,
            newDescription,
            categoryId,
            newAmount,
            newDate,
            true);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == paymentId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var mockPaymentCategoryDbSet = new Mock<DbSet<PaymentCategory>>();
        _mockDbContext.Setup(db => db.PaymentCategories)
            .Returns(mockPaymentCategoryDbSet.Object);

        var categories = new List<PaymentCategory>
        {
            new PaymentCategory { Id = categoryId, Name = "Test", Code = "TST" }
        };

        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<PaymentCategory>(categories.AsQueryable().Provider));
        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.Expression)
            .Returns(categories.AsQueryable().Expression);
        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.ElementType)
            .Returns(categories.AsQueryable().ElementType);
        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.GetEnumerator())
            .Returns(categories.GetEnumerator());

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newDescription, existingPayment.Description);
        Assert.Equal(categoryId, existingPayment.PaymentCategoryId);
        Assert.Equal(newAmount, existingPayment.Amount);
        Assert.Equal(newDate, existingPayment.Date);
        Assert.True(existingPayment.IsOneShot);
        Assert.Equal(modifiedById, existingPayment.ModifiedById);
        Assert.True((DateTime.UtcNow - existingPayment.ModifiedAt).TotalSeconds < 5);
        _mockDbContext.Verify(db => db.Payments.Update(existingPayment), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyDescription_When_OnlyDescriptionProvided()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Old Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var originalAmount = existingPayment.Amount;
        var originalDate = existingPayment.Date;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var originalIsOneShot = existingPayment.IsOneShot;
        var command = new UpdatePaymentCommand(paymentId, modifiedById, "New Description");

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Description", existingPayment.Description);
        Assert.Equal(originalAmount, existingPayment.Amount);
        Assert.Equal(originalDate, existingPayment.Date);
        Assert.Equal(originalCategoryId, existingPayment.PaymentCategoryId);
        Assert.Equal(originalIsOneShot, existingPayment.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyPaymentCategoryId_When_OnlyPaymentCategoryIdProvided()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var originalDescription = existingPayment.Description;
        var originalAmount = existingPayment.Amount;
        var originalDate = existingPayment.Date;
        var originalIsOneShot = existingPayment.IsOneShot;
        var command = new UpdatePaymentCommand(paymentId, modifiedById, paymentCategoryId: categoryId);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var mockPaymentCategoryDbSet = new Mock<DbSet<PaymentCategory>>();
        _mockDbContext.Setup(db => db.PaymentCategories)
            .Returns(mockPaymentCategoryDbSet.Object);

        var categories = new List<PaymentCategory>
        {
            new PaymentCategory { Id = categoryId, Name = "Test", Code = "TST" }
        };

        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<PaymentCategory>(categories.AsQueryable().Provider));
        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.Expression)
            .Returns(categories.AsQueryable().Expression);
        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.ElementType)
            .Returns(categories.AsQueryable().ElementType);
        mockPaymentCategoryDbSet.As<IQueryable<PaymentCategory>>()
            .Setup(m => m.GetEnumerator())
            .Returns(categories.GetEnumerator());

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingPayment.Description);
        Assert.Equal(categoryId, existingPayment.PaymentCategoryId);
        Assert.Equal(originalAmount, existingPayment.Amount);
        Assert.Equal(originalDate, existingPayment.Date);
        Assert.Equal(originalIsOneShot, existingPayment.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyAmount_When_OnlyAmountProvided()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var originalDescription = existingPayment.Description;
        var originalDate = existingPayment.Date;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var originalIsOneShot = existingPayment.IsOneShot;
        var newAmount = 200m;
        var command = new UpdatePaymentCommand(paymentId, modifiedById, amount: newAmount);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingPayment.Description);
        Assert.Equal(originalCategoryId, existingPayment.PaymentCategoryId);
        Assert.Equal(newAmount, existingPayment.Amount);
        Assert.Equal(originalDate, existingPayment.Date);
        Assert.Equal(originalIsOneShot, existingPayment.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyDate_When_OnlyDateProvided()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var originalDescription = existingPayment.Description;
        var originalAmount = existingPayment.Amount;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var originalIsOneShot = existingPayment.IsOneShot;
        var newDate = DateTime.UtcNow.AddDays(5);
        var command = new UpdatePaymentCommand(paymentId, modifiedById, date: newDate);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingPayment.Description);
        Assert.Equal(originalCategoryId, existingPayment.PaymentCategoryId);
        Assert.Equal(originalAmount, existingPayment.Amount);
        Assert.Equal(newDate, existingPayment.Date);
        Assert.Equal(originalIsOneShot, existingPayment.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyIsOneShot_When_OnlyIsOneShotProvided()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var originalDescription = existingPayment.Description;
        var originalAmount = existingPayment.Amount;
        var originalDate = existingPayment.Date;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var command = new UpdatePaymentCommand(paymentId, modifiedById, isOneShot: true);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingPayment.Description);
        Assert.Equal(originalCategoryId, existingPayment.PaymentCategoryId);
        Assert.Equal(originalAmount, existingPayment.Amount);
        Assert.Equal(originalDate, existingPayment.Date);
        Assert.True(existingPayment.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsNull()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var originalDescription = existingPayment.Description;
        var command = new UpdatePaymentCommand(paymentId, modifiedById, null, null, 100m, DateTime.UtcNow);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingPayment.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsEmpty()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var originalDescription = existingPayment.Description;
        var command = new UpdatePaymentCommand(paymentId, modifiedById, "", null, 100m, DateTime.UtcNow);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingPayment.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsWhitespace()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var originalDescription = existingPayment.Description;
        var command = new UpdatePaymentCommand(paymentId, modifiedById, "   ", null, 100m, DateTime.UtcNow);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingPayment.Description);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_When_Called()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var command = new UpdatePaymentCommand(paymentId, modifiedById, "Updated", null, 100m, DateTime.UtcNow);
        var cancellationToken = new CancellationToken();

        _mockValidator.Setup(v => v.ValidateAsync(command, cancellationToken))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            cancellationToken))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        _mockValidator.Verify(v => v.ValidateAsync(command, cancellationToken), Times.Once);
        _mockDbContext.Verify(db => db.Payments.FindAsync(It.IsAny<object[]>(), cancellationToken), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateAnyProperty_When_AllPropertiesAreNull()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };

        var originalDescription = existingPayment.Description;
        var originalAmount = existingPayment.Amount;
        var originalDate = existingPayment.Date;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var originalIsOneShot = existingPayment.IsOneShot;
        var command = new UpdatePaymentCommand(paymentId, modifiedById);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingPayment.Description);
        Assert.Equal(originalAmount, existingPayment.Amount);
        Assert.Equal(originalDate, existingPayment.Date);
        Assert.Equal(originalCategoryId, existingPayment.PaymentCategoryId);
        Assert.Equal(originalIsOneShot, existingPayment.IsOneShot);
        _mockDbContext.Verify(db => db.Payments.Update(existingPayment), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_SetModifiedAtAndModifiedById_When_Called()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var modifiedById = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false,
            ModifiedAt = DateTime.MinValue,
            ModifiedById = Guid.Empty
        };

        var command = new UpdatePaymentCommand(paymentId, modifiedById, "Updated");

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Payments.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockDbContext.Setup(db => db.Payments.Update(It.IsAny<Payment>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdatePaymentCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(modifiedById, existingPayment.ModifiedById);
        Assert.True((DateTime.UtcNow - existingPayment.ModifiedAt).TotalSeconds < 5);
        Assert.NotEqual(DateTime.MinValue, existingPayment.ModifiedAt);
    }

    // Helper class for async queryable support
    private class TestAsyncQueryProvider<TEntity> : IQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }
    }

    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
            : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_inner.MoveNext());
        }

        public T Current => _inner.Current;
    }
}
