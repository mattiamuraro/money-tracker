using Xunit;

namespace MoneyTracker.Data.Tests;

public class SoftDeleteEntityTests
{
    [Fact]
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

        Assert.True(payment.IsDeleted);
        Assert.Equal(deletedBy, payment.DeletedBy);
        Assert.NotNull(payment.DeletedAt);
    }

    [Fact]
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

        Assert.Null(payment.DeletedAt);
        Assert.Null(payment.DeletedBy);
    }
}
