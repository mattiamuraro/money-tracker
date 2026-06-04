using Xunit;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.Api.Endpoints.ForecastIncomes.Contracts;

namespace MoneyTracker.Api.Tests.Endpoints.ForecastIncomes.Contracts;

/// <summary>
/// Unit tests for ForecastIncomeOccurrencesQuery
/// </summary>
public class ForecastIncomeOccurrencesQueryTests
{
    [Fact]
    public void GetRequiredYearMonth_Should_Return_Valid_YearMonth_For_Valid_Format()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "2024-01" };

        // Act
        var result = query.GetRequiredYearMonth();

        // Assert
        Xunit.Assert.Equal(2024, result.Year);
        Xunit.Assert.Equal(1, result.Month);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Return_Valid_YearMonth_For_December()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "2023-12" };

        // Act
        var result = query.GetRequiredYearMonth();

        // Assert
        Xunit.Assert.Equal(2023, result.Year);
        Xunit.Assert.Equal(12, result.Month);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Null()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = null! };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Empty()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Whitespace()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "   " };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Has_No_Dash()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "202401" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Has_Too_Many_Parts()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "2024-01-15" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Year_Is_Not_Numeric()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "abcd-01" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Not_Numeric()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "2024-ab" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Zero()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "2024-00" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Is_Thirteen()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "2024-13" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Has_Single_Digit()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "2024-5" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Month_Has_Whitespace_Around_Parts()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = " 2024 - 03 " };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Only_Year_Is_Provided()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "2024" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }

    [Xunit.Fact]
    public void GetRequiredYearMonth_Should_Throw_When_Only_Dash_Is_Provided()
    {
        // Arrange
        var query = new ForecastIncomeOccurrencesQuery { Month = "-" };

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>((Action)(() => query.GetRequiredYearMonth()));
        Xunit.Assert.Equal("Month is required and must use yyyy-MM format.", exception.Message);
    }
}






