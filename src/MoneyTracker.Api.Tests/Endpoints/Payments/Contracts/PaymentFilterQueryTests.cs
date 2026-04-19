using Xunit;
using MoneyTracker.Api.Endpoints.Payments.Contracts;

namespace MoneyTracker.Api.Tests.Endpoints.Payments.Contracts;

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
            Month = "2026-01",
            CategoryFilter = "Food",
            MinAmount = 10.5m,
            MaxAmount = 120m,
            CategoryId = categoryId
        };

        Xunit.Assert.Equal("2026-01", query.Month);
        Xunit.Assert.Equal("Food", query.CategoryFilter);
        Xunit.Assert.Equal(10.5m, query.MinAmount);
        Xunit.Assert.Equal(120m, query.MaxAmount);
        Xunit.Assert.Equal(categoryId, query.CategoryId);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Return_Parsed_Values()
    {
        var query = new PaymentFilterQuery { Month = "2026-07" };

        var (year, month) = query.GetRequiredYearMonth();

        Xunit.Assert.Equal(2026, year);
        Xunit.Assert.Equal(7, month);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Missing_Or_Invalid()
    {
        var missing = new PaymentFilterQuery();
        var invalid = new PaymentFilterQuery { Month = "07-2026" };

        Xunit.Assert.Throws<ArgumentException>(() => missing.GetRequiredYearMonth());
        Xunit.Assert.Throws<ArgumentException>(() => invalid.GetRequiredYearMonth());
    }
}
