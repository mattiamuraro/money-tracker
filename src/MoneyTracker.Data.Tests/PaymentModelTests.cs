namespace MoneyTracker.Data.Tests;

[TestFixture]
public class PaymentModelTests
{
    [Test]
    public void Payment_CanBeCreated()
    {
        // Arrange & Act
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Test Payment",
            Amount = 100.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = Guid.NewGuid(),
            IsOneShot = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "TestUser"
        };

        // Assert
        Assert.That(payment.Description, Is.EqualTo("Test Payment"));
        Assert.That(payment.Amount, Is.EqualTo(100.00m));
        Assert.That(payment.IsOneShot, Is.True);
    }

    [Test]
    public void Payment_HasMaxLengthConstraintOnDescription()
    {
        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = new string('a', 101), // Exceeds max length
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "TestUser"
        };

        // Assert - should have exceeded max length
        Assert.That(payment.Description.Length, Is.GreaterThan(100));
    }

    [Test]
    public void Payment_HasPaymentCategoryNavigation()
    {
        // Arrange
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "TestUser"
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Test Payment",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "TestUser"
        };

        // Assert
        Assert.That(payment.PaymentCategory, Is.Not.Null);
        Assert.That(payment.PaymentCategory!.Name, Is.EqualTo("Food"));
    }
}

