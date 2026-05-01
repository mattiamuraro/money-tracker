using Xunit;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.Queries;

public class GetIncomeQueryTests
{
    [Fact]
    public void Constructor_Should_Clamp_PageNumber_And_PageSize()
    {
        var query = new GetIncomeQuery(pageNumber: 0, pageSize: 500);

        Assert.Equal(1, query.PageNumber);
        Assert.Equal(100, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Default_SortOrder_When_Null()
    {
        var query = new GetIncomeQuery(sortOrder: null);

        Assert.Equal("desc", query.SortOrder);
    }

    [Fact]
    public void Constructor_Should_Map_All_Properties()
    {
        var query = new GetIncomeQuery(
            descriptionFilter: "Salary",
            minAmount: 100,
            maxAmount: 5000,
            year: 2026,
            month: 3,
            pageNumber: 2,
            pageSize: 50,
            sortBy: "Amount",
            sortOrder: "asc");

        Assert.Equal("Salary", query.DescriptionFilter);
        Assert.Equal(100, query.MinAmount);
        Assert.Equal(5000, query.MaxAmount);
        Assert.Equal(2026, query.Year);
        Assert.Equal(3, query.Month);
        Assert.Equal(2, query.PageNumber);
        Assert.Equal(50, query.PageSize);
        Assert.Equal("Amount", query.SortBy);
        Assert.Equal("asc", query.SortOrder);
    }

    [Fact]
    public void Constructor_Should_Set_Defaults_When_No_Args()
    {
        var query = new GetIncomeQuery();

        Assert.Null(query.Id);
        Assert.Null(query.DescriptionFilter);
        Assert.Null(query.MinAmount);
        Assert.Null(query.MaxAmount);
        Assert.Null(query.Year);
        Assert.Null(query.Month);
        Assert.Equal(1, query.PageNumber);
        Assert.Equal(20, query.PageSize);
        Assert.Null(query.SortBy);
        Assert.Equal("desc", query.SortOrder);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(-100, 1)]
    public void Constructor_Should_Accept_Valid_PageNumber(int input, int expected)
    {
        var query = new GetIncomeQuery(pageNumber: input);

        Assert.Equal(expected, query.PageNumber);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(50, 50)]
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(-50, 1)]
    public void Constructor_Should_Clamp_PageSize(int input, int expected)
    {
        var query = new GetIncomeQuery(pageSize: input);

        Assert.Equal(expected, query.PageSize);
    }

    [Fact]
    public void ParameterizedConstructor_Should_Not_Set_Id()
    {
        var query = new GetIncomeQuery(
            descriptionFilter: "Test",
            minAmount: 100,
            maxAmount: 500,
            year: 2024,
            month: 6,
            pageNumber: 2,
            pageSize: 30,
            sortBy: "Amount",
            sortOrder: "asc");

        Assert.Null(query.Id);
    }

    [Fact]
    public void Constructor_Should_Accept_EmptyString_For_SortOrder()
    {
        var query = new GetIncomeQuery(sortOrder: string.Empty);

        Assert.Equal(string.Empty, query.SortOrder);
    }

    [Fact]
    public void Constructor_Should_Accept_EmptyString_For_DescriptionFilter()
    {
        var query = new GetIncomeQuery(descriptionFilter: string.Empty);

        Assert.Equal(string.Empty, query.DescriptionFilter);
    }

    [Fact]
    public void Constructor_Should_Accept_EmptyString_For_SortBy()
    {
        var query = new GetIncomeQuery(sortBy: string.Empty);

        Assert.Equal(string.Empty, query.SortBy);
    }

    [Fact]
    public void Constructor_Should_Accept_Negative_MinAmount()
    {
        var query = new GetIncomeQuery(minAmount: -100.50m);

        Assert.Equal(-100.50m, query.MinAmount);
    }

    [Fact]
    public void Constructor_Should_Accept_Negative_MaxAmount()
    {
        var query = new GetIncomeQuery(maxAmount: -50.25m);

        Assert.Equal(-50.25m, query.MaxAmount);
    }

    [Fact]
    public void Constructor_Should_Accept_Zero_MinAmount()
    {
        var query = new GetIncomeQuery(minAmount: 0);

        Assert.Equal(0, query.MinAmount);
    }

    [Fact]
    public void Constructor_Should_Accept_Zero_MaxAmount()
    {
        var query = new GetIncomeQuery(maxAmount: 0);

        Assert.Equal(0, query.MaxAmount);
    }

    [Fact]
    public void Constructor_Should_Accept_Negative_Year()
    {
        var query = new GetIncomeQuery(year: -1);

        Assert.Equal(-1, query.Year);
    }

    [Fact]
    public void Constructor_Should_Accept_Zero_Year()
    {
        var query = new GetIncomeQuery(year: 0);

        Assert.Equal(0, query.Year);
    }

    [Fact]
    public void Constructor_Should_Accept_Negative_Month()
    {
        var query = new GetIncomeQuery(month: -5);

        Assert.Equal(-5, query.Month);
    }

    [Fact]
    public void Constructor_Should_Accept_Zero_Month()
    {
        var query = new GetIncomeQuery(month: 0);

        Assert.Equal(0, query.Month);
    }

    [Fact]
    public void Constructor_Should_Accept_Month_GreaterThan_12()
    {
        var query = new GetIncomeQuery(month: 15);

        Assert.Equal(15, query.Month);
    }

    [Fact]
    public void Constructor_Should_Accept_Large_Year_Value()
    {
        var query = new GetIncomeQuery(year: 9999);

        Assert.Equal(9999, query.Year);
    }

    [Fact]
    public void Constructor_Should_Accept_MaxValue_For_Decimal_Amounts()
    {
        var query = new GetIncomeQuery(minAmount: decimal.MaxValue, maxAmount: decimal.MaxValue);

        Assert.Equal(decimal.MaxValue, query.MinAmount);
        Assert.Equal(decimal.MaxValue, query.MaxAmount);
    }

    [Fact]
    public void Constructor_Should_Accept_MinValue_For_Decimal_Amounts()
    {
        var query = new GetIncomeQuery(minAmount: decimal.MinValue, maxAmount: decimal.MinValue);

        Assert.Equal(decimal.MinValue, query.MinAmount);
        Assert.Equal(decimal.MinValue, query.MaxAmount);
    }

    [Fact]
    public void DefaultConstructor_Should_Not_Modify_Properties_After_Creation()
    {
        var query = new GetIncomeQuery();
        var originalPageNumber = query.PageNumber;
        var originalPageSize = query.PageSize;
        var originalSortOrder = query.SortOrder;

        Assert.Equal(1, originalPageNumber);
        Assert.Equal(20, originalPageSize);
        Assert.Equal("desc", originalSortOrder);
    }

    [Theory]
    [InlineData(int.MaxValue, int.MaxValue)]
    [InlineData(int.MinValue, 1)]
    public void Constructor_Should_Handle_Extreme_PageNumber_Values(int input, int expected)
    {
        var query = new GetIncomeQuery(pageNumber: input);

        Assert.Equal(expected, query.PageNumber);
    }

    [Theory]
    [InlineData(int.MaxValue, 100)]
    [InlineData(int.MinValue, 1)]
    public void Constructor_Should_Handle_Extreme_PageSize_Values(int input, int expected)
    {
        var query = new GetIncomeQuery(pageSize: input);

        Assert.Equal(expected, query.PageSize);
    }

    [Fact]
    public void Constructor_Should_Accept_Year_Without_Month()
    {
        var query = new GetIncomeQuery(year: 2024, month: null);

        Assert.Equal(2024, query.Year);
        Assert.Null(query.Month);
    }

    [Fact]
    public void Constructor_Should_Accept_Month_Without_Year()
    {
        var query = new GetIncomeQuery(year: null, month: 6);

        Assert.Null(query.Year);
        Assert.Equal(6, query.Month);
    }

    [Fact]
    public void Constructor_Should_Accept_Both_Year_And_Month()
    {
        var query = new GetIncomeQuery(year: 2024, month: 6);

        Assert.Equal(2024, query.Year);
        Assert.Equal(6, query.Month);
    }

    [Fact]
    public void Constructor_Should_Accept_Neither_Year_Nor_Month()
    {
        var query = new GetIncomeQuery(year: null, month: null);

        Assert.Null(query.Year);
        Assert.Null(query.Month);
    }
}
