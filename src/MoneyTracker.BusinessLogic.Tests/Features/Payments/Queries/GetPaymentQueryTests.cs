using Xunit;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.Queries;

public class GetPaymentQueryTests
{
    [Fact]
    public void Constructor_Should_Clamp_PageNumber_And_PageSize()
    {
        var query = new GetPaymentQuery(pageNumber: 0, pageSize: 500);

        Assert.Equal(1, query.PageNumber);
        Assert.Equal(100, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Default_SortOrder_When_Null()
    {
        var query = new GetPaymentQuery(sortOrder: null);

        Assert.Equal("desc", query.SortOrder);
    }

    [Fact]
    public void Constructor_Should_Map_Properties()
    {
        var categoryId = Guid.NewGuid();

        var query = new GetPaymentQuery(
            categoryFilter: "Food",
            categoryId: categoryId,
            minAmount: 10,
            maxAmount: 300,
            year: 2026,
            month: 1,
            pageNumber: 2,
            pageSize: 25,
            sortBy: "Amount",
            sortOrder: "asc");

        Assert.Equal("Food", query.CategoryFilter);
        Assert.Equal(categoryId, query.CategoryId);
        Assert.Equal(10, query.MinAmount);
        Assert.Equal(300, query.MaxAmount);
        Assert.Equal(2026, query.Year);
        Assert.Equal(1, query.Month);
        Assert.Equal(2, query.PageNumber);
        Assert.Equal(25, query.PageSize);
        Assert.Equal("Amount", query.SortBy);
        Assert.Equal("asc", query.SortOrder);
    }

    [Fact]
    public void DefaultConstructor_Should_CreateInstance()
    {
        var query = new GetPaymentQuery();

        Assert.NotNull(query);
    }

    [Fact]
    public void Constructor_Should_Clamp_Negative_PageNumber_To_One()
    {
        var query = new GetPaymentQuery(pageNumber: -5);

        Assert.Equal(1, query.PageNumber);
    }

    [Fact]
    public void Constructor_Should_Keep_PageNumber_When_Valid()
    {
        var query = new GetPaymentQuery(pageNumber: 10);

        Assert.Equal(10, query.PageNumber);
    }

    [Fact]
    public void Constructor_Should_Clamp_Zero_PageSize_To_One()
    {
        var query = new GetPaymentQuery(pageSize: 0);

        Assert.Equal(1, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Clamp_Negative_PageSize_To_One()
    {
        var query = new GetPaymentQuery(pageSize: -10);

        Assert.Equal(1, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Keep_PageSize_One()
    {
        var query = new GetPaymentQuery(pageSize: 1);

        Assert.Equal(1, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Keep_PageSize_OneHundred()
    {
        var query = new GetPaymentQuery(pageSize: 100);

        Assert.Equal(100, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Keep_Valid_PageSize()
    {
        var query = new GetPaymentQuery(pageSize: 50);

        Assert.Equal(50, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Set_All_Nullable_Parameters_To_Null()
    {
        var query = new GetPaymentQuery(
            categoryFilter: null,
            descriptionFilter: null,
            categoryId: null,
            minAmount: null,
            maxAmount: null,
            year: null,
            month: null);

        Assert.Null(query.CategoryFilter);
        Assert.Null(query.DescriptionFilter);
        Assert.Null(query.CategoryId);
        Assert.Null(query.MinAmount);
        Assert.Null(query.MaxAmount);
        Assert.Null(query.Year);
        Assert.Null(query.Month);
    }

    [Fact]
    public void Constructor_Should_Map_DescriptionFilter()
    {
        var query = new GetPaymentQuery(descriptionFilter: "test description");

        Assert.Equal("test description", query.DescriptionFilter);
    }

    [Fact]
    public void Constructor_Should_Set_Default_Values_When_No_Parameters()
    {
        var query = new GetPaymentQuery();

        Assert.Equal(1, query.PageNumber);
        Assert.Equal(20, query.PageSize);
        Assert.Equal("desc", query.SortOrder);
    }

    [Fact]
    public void Constructor_Should_Set_SortBy_To_Null_When_Not_Provided()
    {
        var query = new GetPaymentQuery();

        Assert.Null(query.SortBy);
    }

    [Fact]
    public void Constructor_Should_Clamp_PageSize_Above_Maximum()
    {
        var query = new GetPaymentQuery(pageSize: 101);

        Assert.Equal(100, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Clamp_Very_Large_PageSize()
    {
        var query = new GetPaymentQuery(pageSize: 1000);

        Assert.Equal(100, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Keep_PageNumber_One()
    {
        var query = new GetPaymentQuery(pageNumber: 1);

        Assert.Equal(1, query.PageNumber);
    }

    [Fact]
    public void Constructor_Should_Set_Empty_String_CategoryFilter()
    {
        var query = new GetPaymentQuery(categoryFilter: "");

        Assert.Equal("", query.CategoryFilter);
    }

    [Fact]
    public void Constructor_Should_Set_Empty_String_DescriptionFilter()
    {
        var query = new GetPaymentQuery(descriptionFilter: "");

        Assert.Equal("", query.DescriptionFilter);
    }

    [Fact]
    public void Constructor_Should_Set_Empty_String_SortBy()
    {
        var query = new GetPaymentQuery(sortBy: "");

        Assert.Equal("", query.SortBy);
    }

    [Fact]
    public void Constructor_Should_Set_Empty_String_SortOrder()
    {
        var query = new GetPaymentQuery(sortOrder: "");

        Assert.Equal("", query.SortOrder);
    }

    [Fact]
    public void Constructor_Should_Set_Guid_Empty_CategoryId()
    {
        var query = new GetPaymentQuery(categoryId: Guid.Empty);

        Assert.Equal(Guid.Empty, query.CategoryId);
    }

    [Fact]
    public void Constructor_Should_Set_Zero_MinAmount()
    {
        var query = new GetPaymentQuery(minAmount: 0m);

        Assert.Equal(0m, query.MinAmount);
    }

    [Fact]
    public void Constructor_Should_Set_Negative_MinAmount()
    {
        var query = new GetPaymentQuery(minAmount: -100m);

        Assert.Equal(-100m, query.MinAmount);
    }

    [Fact]
    public void Constructor_Should_Set_Zero_MaxAmount()
    {
        var query = new GetPaymentQuery(maxAmount: 0m);

        Assert.Equal(0m, query.MaxAmount);
    }

    [Fact]
    public void Constructor_Should_Set_Negative_MaxAmount()
    {
        var query = new GetPaymentQuery(maxAmount: -50m);

        Assert.Equal(-50m, query.MaxAmount);
    }

    [Fact]
    public void Constructor_Should_Set_Very_Large_Decimal_Values()
    {
        var query = new GetPaymentQuery(minAmount: 999999999.99m, maxAmount: 9999999999.99m);

        Assert.Equal(999999999.99m, query.MinAmount);
        Assert.Equal(9999999999.99m, query.MaxAmount);
    }

    [Fact]
    public void Constructor_Should_Set_Zero_Year()
    {
        var query = new GetPaymentQuery(year: 0);

        Assert.Equal(0, query.Year);
    }

    [Fact]
    public void Constructor_Should_Set_Negative_Year()
    {
        var query = new GetPaymentQuery(year: -1);

        Assert.Equal(-1, query.Year);
    }

    [Fact]
    public void Constructor_Should_Set_Zero_Month()
    {
        var query = new GetPaymentQuery(month: 0);

        Assert.Equal(0, query.Month);
    }

    [Fact]
    public void Constructor_Should_Set_Negative_Month()
    {
        var query = new GetPaymentQuery(month: -1);

        Assert.Equal(-1, query.Month);
    }

    [Fact]
    public void Constructor_Should_Set_Month_Thirteen()
    {
        var query = new GetPaymentQuery(month: 13);

        Assert.Equal(13, query.Month);
    }

    [Fact]
    public void DefaultConstructor_Should_Have_Default_Property_Values()
    {
        var query = new GetPaymentQuery();

        Assert.Null(query.Id);
        Assert.Null(query.CategoryFilter);
        Assert.Null(query.DescriptionFilter);
        Assert.Null(query.CategoryId);
        Assert.Null(query.MinAmount);
        Assert.Null(query.MaxAmount);
        Assert.Null(query.Year);
        Assert.Null(query.Month);
        Assert.Equal(1, query.PageNumber);
        Assert.Equal(20, query.PageSize);
        Assert.Null(query.SortBy);
        Assert.Equal("desc", query.SortOrder);
    }

    [Fact]
    public void Constructor_Should_Handle_Combination_Of_Edge_Values()
    {
        var query = new GetPaymentQuery(
            categoryFilter: "",
            descriptionFilter: "",
            categoryId: Guid.Empty,
            minAmount: 0m,
            maxAmount: 0m,
            year: 0,
            month: 0,
            pageNumber: -10,
            pageSize: 1000,
            sortBy: "",
            sortOrder: "");

        Assert.Equal("", query.CategoryFilter);
        Assert.Equal("", query.DescriptionFilter);
        Assert.Equal(Guid.Empty, query.CategoryId);
        Assert.Equal(0m, query.MinAmount);
        Assert.Equal(0m, query.MaxAmount);
        Assert.Equal(0, query.Year);
        Assert.Equal(0, query.Month);
        Assert.Equal(1, query.PageNumber);
        Assert.Equal(100, query.PageSize);
        Assert.Equal("", query.SortBy);
        Assert.Equal("", query.SortOrder);
    }
}
