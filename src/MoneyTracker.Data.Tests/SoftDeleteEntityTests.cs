namespace MoneyTracker.Data.Tests;

[TestFixture]
public class SoftDeleteEntityTests
{
    [Test]
    public void Delete_Should_Set_Deleted_Fields()
    {
        var actorId = Guid.NewGuid();
        var deletedBy = Guid.NewGuid();

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "To delete",
            Amount = 30m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId
        };

        payment.Delete(deletedBy);

        Assert.That(payment.IsDeleted, Is.True);
        Assert.That(payment.DeletedBy, Is.EqualTo(deletedBy));
        Assert.That(payment.DeletedAt, Is.Not.Null);
    }

    [Test]
    public void Restore_Should_Clear_Deleted_Fields()
    {
        var actorId = Guid.NewGuid();

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "To restore",
            Amount = 30m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId
        };

        payment.Delete(Guid.NewGuid());
        payment.Restore();

        Assert.That(payment.DeletedAt, Is.Null);
        Assert.That(payment.DeletedBy, Is.Null);
    }
}
