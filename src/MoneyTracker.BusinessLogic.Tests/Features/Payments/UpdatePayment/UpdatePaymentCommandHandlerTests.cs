using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.UpdatePayment;

public class UpdatePaymentCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static UpdatePaymentCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new UpdatePaymentCommandValidator(), db);

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_When_ValidationFails()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new UpdatePaymentCommand(Guid.NewGuid(), amount: 0m); // Amount must be > 0
        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_ThrowEntityNotFoundException_When_PaymentNotFound()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var command = new UpdatePaymentCommand(paymentId);
        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Contains(paymentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_When_PaymentCategoryNotFound()
    {
        // Arrange
        using var db = CreateDbContext();
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
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var command = new UpdatePaymentCommand(paymentId, paymentCategoryId: categoryId);
        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Contains(categoryId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_UpdateAllProperties_When_AllPropertiesProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var category = new PaymentCategory { Id = categoryId, Name = "Test", Code = "TST" };
        db.PaymentCategories.Add(category);
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Old Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var newDescription = "New Description";
        var newAmount = 100m;
        var newDate = DateTime.UtcNow;
        var command = new UpdatePaymentCommand(paymentId, newDescription, categoryId, newAmount, newDate, true);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal(newDescription, updated!.Description);
        Assert.Equal(categoryId, updated.PaymentCategoryId);
        Assert.Equal(newAmount, updated.Amount);
        Assert.Equal(newDate, updated.Date);
        Assert.True(updated.IsOneShot);
        Assert.True((DateTime.UtcNow - updated.ModifiedAt).TotalSeconds < 5);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyDescription_When_OnlyDescriptionProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Old Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var originalAmount = existingPayment.Amount;
        var originalDate = existingPayment.Date;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var originalIsOneShot = existingPayment.IsOneShot;
        var command = new UpdatePaymentCommand(paymentId, "New Description");
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal("New Description", updated!.Description);
        Assert.Equal(originalAmount, updated.Amount);
        Assert.Equal(originalDate, updated.Date);
        Assert.Equal(originalCategoryId, updated.PaymentCategoryId);
        Assert.Equal(originalIsOneShot, updated.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyPaymentCategoryId_When_OnlyPaymentCategoryIdProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var category = new PaymentCategory { Id = categoryId, Name = "Test", Code = "TST" };
        db.PaymentCategories.Add(category);
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var originalDescription = existingPayment.Description;
        var originalAmount = existingPayment.Amount;
        var originalDate = existingPayment.Date;
        var originalIsOneShot = existingPayment.IsOneShot;
        var command = new UpdatePaymentCommand(paymentId, paymentCategoryId: categoryId);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal(originalDescription, updated!.Description);
        Assert.Equal(categoryId, updated.PaymentCategoryId);
        Assert.Equal(originalAmount, updated.Amount);
        Assert.Equal(originalDate, updated.Date);
        Assert.Equal(originalIsOneShot, updated.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyAmount_When_OnlyAmountProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var originalDescription = existingPayment.Description;
        var originalDate = existingPayment.Date;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var originalIsOneShot = existingPayment.IsOneShot;
        var newAmount = 200m;
        var command = new UpdatePaymentCommand(paymentId, amount: newAmount);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal(originalDescription, updated!.Description);
        Assert.Equal(originalCategoryId, updated.PaymentCategoryId);
        Assert.Equal(newAmount, updated.Amount);
        Assert.Equal(originalDate, updated.Date);
        Assert.Equal(originalIsOneShot, updated.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyDate_When_OnlyDateProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var originalDescription = existingPayment.Description;
        var originalAmount = existingPayment.Amount;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var originalIsOneShot = existingPayment.IsOneShot;
        var newDate = DateTime.UtcNow.AddDays(5);
        var command = new UpdatePaymentCommand(paymentId, date: newDate);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal(originalDescription, updated!.Description);
        Assert.Equal(originalCategoryId, updated.PaymentCategoryId);
        Assert.Equal(originalAmount, updated.Amount);
        Assert.Equal(newDate, updated.Date);
        Assert.Equal(originalIsOneShot, updated.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyIsOneShot_When_OnlyIsOneShotProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var originalDescription = existingPayment.Description;
        var originalAmount = existingPayment.Amount;
        var originalDate = existingPayment.Date;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var command = new UpdatePaymentCommand(paymentId, isOneShot: true);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal(originalDescription, updated!.Description);
        Assert.Equal(originalCategoryId, updated.PaymentCategoryId);
        Assert.Equal(originalAmount, updated.Amount);
        Assert.Equal(originalDate, updated.Date);
        Assert.True(updated.IsOneShot);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsNull()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var command = new UpdatePaymentCommand(paymentId, null, null, 100m, DateTime.UtcNow);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal("Original Description", updated!.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsEmpty()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var command = new UpdatePaymentCommand(paymentId, "", null, 100m, DateTime.UtcNow);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal("Original Description", updated!.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsWhitespace()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var command = new UpdatePaymentCommand(paymentId, "   ", null, 100m, DateTime.UtcNow);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal("Original Description", updated!.Description);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_When_Called()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var command = new UpdatePaymentCommand(paymentId, "Updated", null, 100m, DateTime.UtcNow);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var result = await db.Payments.FindAsync(paymentId);
        Assert.Equal("Updated", result!.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateAnyProperty_When_AllPropertiesAreNull()
    {
        // Arrange
        using var db = CreateDbContext();
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.UtcNow.AddDays(-10),
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = false
        };
        db.Payments.Add(existingPayment);
        await db.SaveChangesAsync();

        var originalDescription = existingPayment.Description;
        var originalAmount = existingPayment.Amount;
        var originalDate = existingPayment.Date;
        var originalCategoryId = existingPayment.PaymentCategoryId;
        var originalIsOneShot = existingPayment.IsOneShot;
        var command = new UpdatePaymentCommand(paymentId);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Payments.FindAsync(paymentId);
        Assert.Equal(originalDescription, updated!.Description);
        Assert.Equal(originalAmount, updated.Amount);
        Assert.Equal(originalDate, updated.Date);
        Assert.Equal(originalCategoryId, updated.PaymentCategoryId);
        Assert.Equal(originalIsOneShot, updated.IsOneShot);
    }
}
