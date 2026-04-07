using Xunit;
using MoneyTracker.Api.Endpoints.Payments.Contracts;

namespace MoneyTracker.Tests.DTOs;

public class PaymentFilterQueryTests
{
    [Fact]
    public void Should_Inherit_Pagination_Defaults()
    {
        var query = new PaymentFilterQuery();

        Xunit.Assert.Equal(1, query.PageNumber);
        Xunit.Assert.Equal(20, query.PageSize);
        Xunit.Assert.Equal("desc", query.SortOrder);
    }

    [Fact]
    public void Validate_Should_Apply_Pagination_Validation_From_Base_Class()
    {
        var query = new PaymentFilterQuery
        {
            PageNumber = 0,
            PageSize = 500,
            SortOrder = "invalid"
        };

        query.Validate();

        Xunit.Assert.Equal(1, query.PageNumber);
        Xunit.Assert.Equal(100, query.PageSize);
        Xunit.Assert.Equal("desc", query.SortOrder);
    }

    [Fact]
    public void Should_Store_Filter_Properties()
    {
        var categoryId = Guid.NewGuid();
        var query = new PaymentFilterQuery
        {
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            CategoryFilter = "Food",
            MinAmount = 10.5m,
            MaxAmount = 120m,
            CategoryId = categoryId
        };

        Xunit.Assert.Equal(new DateTime(2026, 1, 1), query.StartDate);
        Xunit.Assert.Equal(new DateTime(2026, 1, 31), query.EndDate);
        Xunit.Assert.Equal("Food", query.CategoryFilter);
        Xunit.Assert.Equal(10.5m, query.MinAmount);
        Xunit.Assert.Equal(120m, query.MaxAmount);
        Xunit.Assert.Equal(categoryId, query.CategoryId);
    }
}
