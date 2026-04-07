using Xunit;
using MoneyTracker.BusinessLogic.Features.Payments.Queries.GetPaymentHistory;

namespace MoneyTracker.Tests.Features.Payments.Queries;

public class GetPaymentQueryTests
{
    [Fact]
    public void Constructor_Should_Clamp_PageNumber_And_PageSize()
    {
        var query = new GetPaymentQuery(pageNumber: 0, pageSize: 500);

        Xunit.Assert.Equal(1, query.PageNumber);
        Xunit.Assert.Equal(100, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Default_SortOrder_When_Null()
    {
        var query = new GetPaymentQuery(sortOrder: null);

        Xunit.Assert.Equal("desc", query.SortOrder);
    }

    [Fact]
    public void Constructor_Should_Map_Properties()
    {
        var categoryId = Guid.NewGuid();

        var query = new GetPaymentQuery(
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 1, 31),
            categoryFilter: "Food",
            categoryId: categoryId,
            minAmount: 10,
            maxAmount: 300,
            pageNumber: 2,
            pageSize: 25,
            sortBy: "Amount",
            sortOrder: "asc");

        Xunit.Assert.Equal(new DateTime(2026, 1, 1), query.StartDate);
        Xunit.Assert.Equal(new DateTime(2026, 1, 31), query.EndDate);
        Xunit.Assert.Equal("Food", query.CategoryFilter);
        Xunit.Assert.Equal(categoryId, query.CategoryId);
        Xunit.Assert.Equal(10, query.MinAmount);
        Xunit.Assert.Equal(300, query.MaxAmount);
        Xunit.Assert.Equal(2, query.PageNumber);
        Xunit.Assert.Equal(25, query.PageSize);
        Xunit.Assert.Equal("Amount", query.SortBy);
        Xunit.Assert.Equal("asc", query.SortOrder);
    }
}
