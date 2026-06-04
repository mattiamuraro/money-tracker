using Xunit;
using MoneyTracker.Api.Endpoints.Incomes.Contracts;
using MoneyTracker.BusinessLogic.Common.Exceptions;

namespace MoneyTracker.Api.Tests.Endpoints.Incomes.Contracts;

public class IncomeFilterQueryTests
{
    [Fact]
    public void GetRequiredYearMonth_Should_Return_Parsed_Values()
    {
        var query = new IncomeFilterQuery { Month = "2026-07" };

        var (year, month) = query.GetRequiredYearMonth();

        Xunit.Assert.Equal(2026, year);
        Xunit.Assert.Equal(7, month);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Missing_Or_Invalid()
    {
        var missing = new IncomeFilterQuery();
        var invalid = new IncomeFilterQuery { Month = "07-2026" };

        Xunit.Assert.Throws<BadRequestException>(() => missing.GetRequiredYearMonth());
        Xunit.Assert.Throws<BadRequestException>(() => invalid.GetRequiredYearMonth());
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Whitespace()
    {
        var query = new IncomeFilterQuery { Month = "   " };

        var exception = Xunit.Assert.Throws<BadRequestException>(() => query.GetRequiredYearMonth());

        Xunit.Assert.Equal("Month filter is required and must use yyyy-MM format.", exception.Message);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Empty_String()
    {
        var query = new IncomeFilterQuery { Month = "" };

        var exception = Xunit.Assert.Throws<BadRequestException>(() => query.GetRequiredYearMonth());

        Xunit.Assert.Equal("Month filter is required and must use yyyy-MM format.", exception.Message);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Handle_Minimum_Valid_Date()
    {
        var query = new IncomeFilterQuery { Month = "0001-01" };

        var (year, month) = query.GetRequiredYearMonth();

        Xunit.Assert.Equal(1, year);
        Xunit.Assert.Equal(1, month);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Handle_Maximum_Valid_Date()
    {
        var query = new IncomeFilterQuery { Month = "9999-12" };

        var (year, month) = query.GetRequiredYearMonth();

        Xunit.Assert.Equal(9999, year);
        Xunit.Assert.Equal(12, month);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Has_Single_Digit()
    {
        var query = new IncomeFilterQuery { Month = "2026-1" };

        var exception = Xunit.Assert.Throws<BadRequestException>(() => query.GetRequiredYearMonth());

        Xunit.Assert.Equal("Month filter is required and must use yyyy-MM format.", exception.Message);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Text()
    {
        var query = new IncomeFilterQuery { Month = "2026-JAN" };

        var exception = Xunit.Assert.Throws<BadRequestException>(() => query.GetRequiredYearMonth());

        Xunit.Assert.Equal("Month filter is required and must use yyyy-MM format.", exception.Message);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Only_Year_Provided()
    {
        var query = new IncomeFilterQuery { Month = "2026" };

        var exception = Xunit.Assert.Throws<BadRequestException>(() => query.GetRequiredYearMonth());

        Xunit.Assert.Equal("Month filter is required and must use yyyy-MM format.", exception.Message);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Day_Included()
    {
        var query = new IncomeFilterQuery { Month = "2026-07-15" };

        var exception = Xunit.Assert.Throws<BadRequestException>(() => query.GetRequiredYearMonth());

        Xunit.Assert.Equal("Month filter is required and must use yyyy-MM format.", exception.Message);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Out_Of_Range()
    {
        var query = new IncomeFilterQuery { Month = "2026-13" };

        var exception = Xunit.Assert.Throws<BadRequestException>(() => query.GetRequiredYearMonth());

        Xunit.Assert.Equal("Month filter is required and must use yyyy-MM format.", exception.Message);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Zero()
    {
        var query = new IncomeFilterQuery { Month = "2026-00" };

        var exception = Xunit.Assert.Throws<BadRequestException>(() => query.GetRequiredYearMonth());

        Xunit.Assert.Equal("Month filter is required and must use yyyy-MM format.", exception.Message);
    }
}



