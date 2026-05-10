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

        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };

        var activePayment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Active",
            Amount = 10,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category.Id,
            PaymentCategory = category
        };

        var deletedPayment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Deleted",
            Amount = 20,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
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

    [Fact]
    public async Task Incomes_Query_Should_Exclude_SoftDeleted_Records()
    {
        using var dbContext = _fixture.CreateDbContext();

        var activeIncome = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Salary",
            Amount = 1000,
            Date = DateTime.UtcNow
        };

        var deletedIncome = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Old salary",
            Amount = 500,
            Date = DateTime.UtcNow,
            IsDeleted = true
        };

        dbContext.Incomes.Add(activeIncome);
        dbContext.Incomes.Add(deletedIncome);
        await dbContext.SaveChangesAsync();

        var visibleIncomes = dbContext.Incomes.ToList();
        var allIncomes = dbContext.Incomes.IgnoreQueryFilters().ToList();

        Assert.Single(visibleIncomes);
        Assert.Equal(activeIncome.Id, visibleIncomes[0].Id);
        Assert.Equal(2, allIncomes.Count);
    }

    [Fact]
    public async Task SaveChanges_Should_SetDeletedAuditFields_WhenSoftDeletedEntityIsSaved()
    {
        using var dbContext = _fixture.CreateDbContext();

        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Bills",
            Code = "BILL"
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Internet",
            Amount = 30,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category.Id,
            PaymentCategory = category
        };

        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        payment.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        var deletedPayment = await dbContext.Payments.IgnoreQueryFilters().SingleAsync(p => p.Id == payment.Id);

        Assert.NotNull(deletedPayment.DeletedAt);
        Assert.Equal(SystemUsers.SystemUserId, deletedPayment.DeletedBy);
    }
}
