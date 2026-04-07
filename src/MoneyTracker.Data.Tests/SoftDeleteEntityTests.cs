namespace MoneyTracker.Data.Tests;

[TestFixture]
public class SoftDeleteEntityTests
{
    [Test]
    public void Delete_Should_Set_Deleted_Fields()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "To delete",
            Amount = 30m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        payment.Delete("admin");

        Assert.That(payment.IsDeleted, Is.True);
        Assert.That(payment.DeletedBy, Is.EqualTo("admin"));
        Assert.That(payment.DeletedAt, Is.Not.Null);
    }

    [Test]
    public void Restore_Should_Clear_Deleted_Fields()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "To restore",
            Amount = 30m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        payment.Delete("admin");
        payment.Restore();

        Assert.That(payment.DeletedAt, Is.Null);
        Assert.That(payment.DeletedBy, Is.Null);
    }
}
