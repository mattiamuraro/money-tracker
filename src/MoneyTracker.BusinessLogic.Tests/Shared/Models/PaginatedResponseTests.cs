using MoneyTracker.BusinessLogic.Common.Models;

namespace MoneyTracker.BusinessLogic.Tests.Shared.Models;

/// <summary>
/// Unit tests for PaginatedResponse
/// </summary>
public class PaginatedResponseTests
{
    [Fact]
    public void Should_Calculate_TotalPages_Correctly()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 45
        };

        // Act & Assert
        Assert.Equal(5, response.TotalPages); // 45 / 10 = 4.5 => 5 pages
    }

    [Fact]
    public void Should_Have_PreviousPage_When_Not_On_First_Page()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 2,
            PageSize = 10,
            TotalItems = 50
        };

        // Act & Assert
        Assert.True(response.HasPreviousPage);
    }

    [Fact]
    public void Should_Not_Have_PreviousPage_On_First_Page()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 50
        };

        // Act & Assert
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void Should_Have_NextPage_When_Not_On_Last_Page()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 50
        };

        // Act & Assert
        Assert.True(response.HasNextPage);
    }

    [Fact]
    public void Should_Not_Have_NextPage_On_Last_Page()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 5,
            PageSize = 10,
            TotalItems = 50
        };

        // Act & Assert
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public void Should_Calculate_TotalPages_With_Remainder()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 1,
            PageSize = 7,
            TotalItems = 100
        };

        // Act & Assert
        Assert.Equal(15, response.TotalPages); // Ceiling of 100/7 = 15
    }

    [Fact]
    public void TotalPages_ZeroItems_ReturnsZero()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 0
        };

        // Act & Assert
        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public void TotalPages_ExactMultiple_ReturnsCorrectCount()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 30
        };

        // Act & Assert
        Assert.Equal(3, response.TotalPages);
    }

    [Fact]
    public void TotalPages_SingleItem_ReturnsOne()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 1
        };

        // Act & Assert
        Assert.Equal(1, response.TotalPages);
    }

    [Fact]
    public void TotalPages_ItemsEqualPageSize_ReturnsOne()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 10
        };

        // Act & Assert
        Assert.Equal(1, response.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_PageZero_ReturnsFalse()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 0,
            PageSize = 10,
            TotalItems = 50
        };

        // Act & Assert
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_HigherPageNumber_ReturnsTrue()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 5,
            PageSize = 10,
            TotalItems = 100
        };

        // Act & Assert
        Assert.True(response.HasPreviousPage);
    }

    [Fact]
    public void HasNextPage_WhenTotalPagesIsZero_ReturnsFalse()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 0
        };

        // Act & Assert
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public void HasNextPage_PageNumberEqualsTotalPages_ReturnsFalse()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 3,
            PageSize = 10,
            TotalItems = 30
        };

        // Act & Assert
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public void HasNextPage_PageNumberExceedsTotalPages_ReturnsFalse()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 10,
            PageSize = 10,
            TotalItems = 50
        };

        // Act & Assert
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public void HasNextPage_MiddlePage_ReturnsTrue()
    {
        // Arrange
        var response = new PaginatedResponse<string>
        {
            PageNumber = 2,
            PageSize = 10,
            TotalItems = 50
        };

        // Act & Assert
        Assert.True(response.HasNextPage);
    }
}
