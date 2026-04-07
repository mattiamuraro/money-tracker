using Xunit;
using MoneyTracker.BusinessLogic.ExtensionMethods.Mapping;
using MoneyTracker.Data;

namespace MoneyTracker.Tests.ExtensionMethods.Mapping;

public class PaymentRowMappingMethodsTests
{
    [Fact]
    public void ToPaymentRow_Should_Map_All_Properties()
    {
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

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 45.5m,
            Date = new DateTime(2026, 4, 5),
            IsOneShot = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var row = payment.ToPaymentRow();

        Xunit.Assert.Equal(payment.Id, row.Id);
        Xunit.Assert.Equal("Groceries", row.Description);
        Xunit.Assert.Equal(category.Id, row.PaymentCategoryId);
        Xunit.Assert.Equal("Food", row.Category);
        Xunit.Assert.Equal(45.5m, row.Amount);
        Xunit.Assert.Equal(new DateTime(2026, 4, 5), row.Date);
        Xunit.Assert.True(row.IsOneShot);
    }

    [Fact]
    public void ToPaymentRows_Should_Map_Collection()
    {
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Transport",
            Code = "TRNS",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var payments = new List<Payment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Bus",
                PaymentCategoryId = category.Id,
                PaymentCategory = category,
                Amount = 2.5m,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "tester",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "tester"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Train",
                PaymentCategoryId = category.Id,
                PaymentCategory = category,
                Amount = 9m,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "tester",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "tester"
            }
        };

        var rows = payments.ToPaymentRows();

        Xunit.Assert.Equal(2, rows.Count);
        Xunit.Assert.Equal("Bus", rows[0].Description);
        Xunit.Assert.Equal("Train", rows[1].Description);
    }
}
