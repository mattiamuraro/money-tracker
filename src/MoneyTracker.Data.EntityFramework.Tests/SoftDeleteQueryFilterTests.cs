using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework.Tests.TestFixtures;

namespace MoneyTracker.Data.EntityFramework.Tests;

[TestFixture]
public class SoftDeleteQueryFilterTests
{
    private InMemoryDbContextFixture _fixture = null!;

    [SetUp]
    public void Setup()
    {
        _fixture = new InMemoryDbContextFixture();
    }

    [TearDown]
    public void TearDown()
    {
        _fixture.Dispose();
    }

    [Test]
    public async Task Payments_Query_Should_Exclude_SoftDeleted_Records()
    {
        using var dbContext = _fixture.CreateDbContext();

        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
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
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
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
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        deletedPayment.Delete("tester");

        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.Add(activePayment);
        dbContext.Payments.Add(deletedPayment);
        await dbContext.SaveChangesAsync();

        var visiblePayments = dbContext.Payments.ToList();
        var allPayments = dbContext.Payments.IgnoreQueryFilters().ToList();

        Assert.That(visiblePayments, Has.Count.EqualTo(1));
        Assert.That(visiblePayments[0].Description, Is.EqualTo("Active"));
        Assert.That(allPayments, Has.Count.EqualTo(2));
    }
}
