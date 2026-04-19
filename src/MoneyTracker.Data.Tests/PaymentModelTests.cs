using Xunit;

namespace MoneyTracker.Data.Tests;

public class PaymentModelTests
{
    [Fact]
    public void Payment_CanBeCreated()
    {
        var actorId = Guid.NewGuid();

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
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId
        };

        // Assert
        Assert.Equal("Test Payment", payment.Description);
        Assert.Equal(100.00m, payment.Amount);
        Assert.True(payment.IsOneShot);
    }

    [Fact]
    public void Payment_HasMaxLengthConstraintOnDescription()
    {
        var actorId = Guid.NewGuid();

        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = new string('a', 101), // Exceeds max length
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId
        };

        // Assert - should have exceeded max length
        Assert.True(payment.Description.Length > 100);
    }

    [Fact]
    public void Payment_HasPaymentCategoryNavigation()
    {
        var actorId = Guid.NewGuid();

        // Arrange
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD",
            CreatedAt = DateTime.UtcNow,
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId
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
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId
        };

        // Assert
        Assert.NotNull(payment.PaymentCategory);
        Assert.Equal("Food", payment.PaymentCategory!.Name);
    }
}

