using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.Data.EntityFramework.Tests.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MoneyTracker.Data.EntityFramework.Tests;

public class MoneyTrackerDbContextTests : IDisposable
{
    private readonly InMemoryDbContextFixture _fixture;
    private readonly MoneyTrackerDbContext _dbContext;

    public MoneyTrackerDbContextTests()
    {
        _fixture = new InMemoryDbContextFixture();
        _dbContext = _fixture.CreateDbContext();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _fixture.Dispose();
    }

    [Fact]
    public void DbContext_CanCreateDatabase()
    {
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", _dbContext.Database.ProviderName);
    }

    [Fact]
    public async Task DbContext_CanAddPaymentCategory()
    {
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

        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var savedCategory = _dbContext.PaymentCategories.FirstOrDefault(c => c.Id == category.Id);
        Assert.NotNull(savedCategory);
        Assert.Equal("Food", savedCategory!.Name);
    }

    [Fact]
    public async Task DbContext_CanAddPayment()
    {
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

        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        var savedPayment = _dbContext.Payments
            .Where(p => p.Id == payment.Id)
            .Select(p => new { p.Description, p.Amount, Category = p.PaymentCategory.Name })
            .FirstOrDefault();

        Assert.NotNull(savedPayment);
        Assert.Equal("Groceries", savedPayment!.Description);
        Assert.Equal(50.00m, savedPayment.Amount);
    }

    [Fact]
    public async Task DbContext_CanUpdatePayment()
    {
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

        var paymentToUpdate = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.NotNull(paymentToUpdate);

        paymentToUpdate!.Description = "Updated Groceries";
        paymentToUpdate.Amount = 75.50m;
        await _dbContext.SaveChangesAsync();

        var updatedPayment = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.NotNull(updatedPayment);
        Assert.Equal("Updated Groceries", updatedPayment!.Description);
        Assert.Equal(75.50m, updatedPayment.Amount);
    }

    [Fact]
    public async Task DbContext_CanDeletePayment()
    {
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

        var paymentToDelete = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.NotNull(paymentToDelete);

        _dbContext.Payments.Remove(paymentToDelete!);
        await _dbContext.SaveChangesAsync();

        var deletedPayment = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.Null(deletedPayment);
    }

    [Fact]
    public async Task DbContext_CanQueryPaymentsByCategory()
    {
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

        var foodPayments = _dbContext.Payments
            .Where(p => p.PaymentCategoryId == category1.Id)
            .ToList();

        Assert.Single(foodPayments);
        Assert.Equal("Groceries", foodPayments.First().Description);
    }

    [Fact]
    public async Task DbContext_CanQueryPaymentsByDateRange()
    {
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

        var paymentsInRange = _dbContext.Payments
            .Where(p => p.Date >= today && p.Date < today.AddDays(2))
            .ToList();

        Assert.Single(paymentsInRange);
        Assert.Equal("Recent Payment", paymentsInRange.First().Description);
    }
}
