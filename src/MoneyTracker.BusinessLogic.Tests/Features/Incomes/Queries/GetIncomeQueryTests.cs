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

        Assert.Equal(1, query.PageNumber);
        Assert.Equal(20, query.PageSize);
        Assert.Equal("desc", query.SortOrder);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
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
    public void Constructor_Should_Clamp_PageSize(int input, int expected)
    {
        var query = new GetIncomeQuery(pageSize: input);

        Assert.Equal(expected, query.PageSize);
    }
}
