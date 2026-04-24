using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework.Tests.TestFixtures;
using Xunit;

namespace MoneyTracker.Data.EntityFramework.Tests;

public class SoftDeleteQueryFilterTests : IDisposable
{
    private readonly InMemoryDbContextFixture _fixture;

    public SoftDeleteQueryFilterTests()
    {
        _fixture = new InMemoryDbContextFixture();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task Payments_Query_Should_Exclude_SoftDeleted_Records()
    {
        using var dbContext = _fixture.CreateDbContext();
        var actorId = Guid.NewGuid();

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

        var activePayment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Active",
            Amount = 10,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            CreatedAt = DateTime.UtcNow,
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId
        };

        var deletedPayment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Deleted",
            Amount = 20,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            CreatedAt = DateTime.UtcNow,
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId,
            IsDeleted = true
        };

        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.Add(activePayment);
        dbContext.Payments.Add(deletedPayment);
        await dbContext.SaveChangesAsync();

        var visiblePayments = dbContext.Payments.ToList();
        var allPayments = dbContext.Payments.IgnoreQueryFilters().ToList();

        Assert.Single(visiblePayments);
        Assert.Equal("Active", visiblePayments[0].Description);
        Assert.Equal(2, allPayments.Count);
    }
}
