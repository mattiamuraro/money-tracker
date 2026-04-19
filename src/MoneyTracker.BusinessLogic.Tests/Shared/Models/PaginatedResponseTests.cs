using Xunit;
using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.BusinessLogic.Tests.Shared.Models;

/// <summary>
/// Unit tests for PaginatedResponse
/// </summary>
public class PaginatedResponseTests
{
    [Xunit.Fact]
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
        Xunit.Assert.Equal(5, response.TotalPages); // 45 / 10 = 4.5 => 5 pages
    }

    [Xunit.Fact]
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
        Xunit.Assert.True(response.HasPreviousPage);
    }

    [Xunit.Fact]
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
        Xunit.Assert.False(response.HasPreviousPage);
    }

    [Xunit.Fact]
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
        Xunit.Assert.True(response.HasNextPage);
    }

    [Xunit.Fact]
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
        Xunit.Assert.False(response.HasNextPage);
    }

    [Xunit.Fact]
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
        Xunit.Assert.Equal(15, response.TotalPages); // Ceiling of 100/7 = 15
    }
}
