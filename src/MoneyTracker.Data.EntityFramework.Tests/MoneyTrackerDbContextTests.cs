using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.Data.EntityFramework.Tests.TestFixtures;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker.Data.EntityFramework.Tests;

[TestFixture]
public class MoneyTrackerDbContextTests
{
    private InMemoryDbContextFixture _fixture = null!;
    private MoneyTrackerDbContext _dbContext = null!;

    [SetUp]
    public void Setup()
    {
        _fixture = new InMemoryDbContextFixture();
        _dbContext = _fixture.CreateDbContext();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
        _fixture?.Dispose();
    }

    [Test]
    public void DbContext_CanCreateDatabase()
    {
        // Assert
        Assert.That(_dbContext.Database.ProviderName, Is.EqualTo("Microsoft.EntityFrameworkCore.InMemory"));
    }

    [Test]
    public async Task DbContext_CanAddPaymentCategory()
    {
        // Arrange
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD",
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedCategory = _dbContext.PaymentCategories.FirstOrDefault(c => c.Id == category.Id);
        Assert.That(savedCategory, Is.Not.Null);
        Assert.That(savedCategory!.Name, Is.EqualTo("Food"));
    }

    [Test]
    public async Task DbContext_CanAddPayment()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD",
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = categoryId,
            IsOneShot = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedPayment = _dbContext.Payments
            .Where(p => p.Id == payment.Id)
            .Select(p => new { p.Description, p.Amount, Category = p.PaymentCategory.Name })
            .FirstOrDefault();

        Assert.That(savedPayment, Is.Not.Null);
        Assert.That(savedPayment!.Description, Is.EqualTo("Groceries"));
        Assert.That(savedPayment.Amount, Is.EqualTo(50.00m));
    }

    [Test]
    public async Task DbContext_CanUpdatePayment()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD",
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Groceries",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = categoryId,
            IsOneShot = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        // Act
        var paymentToUpdate = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.That(paymentToUpdate, Is.Not.Null);

        paymentToUpdate!.Description = "Updated Groceries";
        paymentToUpdate.Amount = 75.50m;
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedPayment = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.That(updatedPayment!.Description, Is.EqualTo("Updated Groceries"));
        Assert.That(updatedPayment.Amount, Is.EqualTo(75.50m));
    }

    [Test]
    public async Task DbContext_CanDeletePayment()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD",
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Groceries",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = categoryId,
            IsOneShot = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        // Act
        var paymentToDelete = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.That(paymentToDelete, Is.Not.Null);

        _dbContext.Payments.Remove(paymentToDelete!);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedPayment = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.That(deletedPayment, Is.Null);
    }

    [Test]
    public async Task DbContext_CanQueryPaymentsByCategory()
    {
        // Arrange
        var category1 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD",
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        var category2 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Transport",
            Code = "TRANS",
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        _dbContext.PaymentCategories.Add(category1);
        _dbContext.PaymentCategories.Add(category2);

        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category1.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Bus Ticket",
            Amount = 10.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category2.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        _dbContext.Payments.Add(payment1);
        _dbContext.Payments.Add(payment2);
        await _dbContext.SaveChangesAsync();

        // Act
        var foodPayments = _dbContext.Payments
            .Where(p => p.PaymentCategoryId == category1.Id)
            .ToList();

        // Assert
        Assert.That(foodPayments, Has.Count.EqualTo(1));
        Assert.That(foodPayments.First().Description, Is.EqualTo("Groceries"));
    }

    [Test]
    public async Task DbContext_CanQueryPaymentsByDateRange()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD",
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        var today = DateTime.UtcNow.Date;

        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Old Payment",
            Amount = 50.00m,
            Date = today.AddDays(-5),
            PaymentCategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Recent Payment",
            Amount = 30.00m,
            Date = today.AddDays(1),
            PaymentCategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment1);
        _dbContext.Payments.Add(payment2);
        await _dbContext.SaveChangesAsync();

        // Act
        var paymentsInRange = _dbContext.Payments
            .Where(p => p.Date >= today && p.Date < today.AddDays(2))
            .ToList();

        // Assert
        Assert.That(paymentsInRange, Has.Count.EqualTo(1));
        Assert.That(paymentsInRange.First().Description, Is.EqualTo("Recent Payment"));
    }
}
