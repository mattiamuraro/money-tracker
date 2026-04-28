using Xunit;
using MoneyTracker.Api.Endpoints.Payments.Contracts;

namespace MoneyTracker.Api.Tests.Endpoints.Payments.Contracts;

/// <summary>
/// Unit tests for PaginationQuery
/// </summary>
public class PaginationQueryTests
{
    [Xunit.Fact]
    public void Should_Set_Default_Values()
    {
        // Arrange & Act
        var query = new PaginationQuery();

        // Assert
        Xunit.Assert.Equal(1, query.PageNumber);
        Xunit.Assert.Equal(20, query.PageSize);
        Xunit.Assert.Equal("desc", query.SortOrder);
    }

    [Xunit.Fact]
    public void Should_Clamp_PageNumber_To_Minimum_One()
    {
        // Arrange
        var query = new PaginationQuery { PageNumber = -5 };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(1, query.PageNumber);
    }

    [Xunit.Fact]
    public void Should_Clamp_PageNumber_To_Minimum_One_When_Zero()
    {
        // Arrange
        var query = new PaginationQuery { PageNumber = 0 };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(1, query.PageNumber);
    }

    [Xunit.Fact]
    public void Should_Clamp_PageSize_To_Minimum_One()
    {
        // Arrange
        var query = new PaginationQuery { PageSize = 0 };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(1, query.PageSize);
    }

    [Xunit.Fact]
    public void Should_Clamp_PageSize_To_Maximum_100()
    {
        // Arrange
        var query = new PaginationQuery { PageSize = 200 };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(100, query.PageSize);
    }

    [Xunit.Fact]
    public void Should_Keep_Valid_PageSize()
    {
        // Arrange
        var query = new PaginationQuery { PageSize = 50 };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(50, query.PageSize);
    }

    [Xunit.Fact]
    public void Should_Default_Invalid_SortOrder_To_Desc()
    {
        // Arrange
        var query = new PaginationQuery { SortOrder = "invalid" };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal("desc", query.SortOrder);
    }

    [Xunit.Theory]
    [Xunit.InlineData("asc")]
    [Xunit.InlineData("desc")]
    [Xunit.InlineData("ASC")]
    [Xunit.InlineData("DESC")]
    public void Should_Accept_Valid_SortOrders(string sortOrder)
    {
        // Arrange
        var query = new PaginationQuery { SortOrder = sortOrder };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.NotNull(query.SortOrder);
        Xunit.Assert.True(new[] { "asc", "desc" }.Contains(query.SortOrder.ToLower()));
    }

    [Xunit.Fact]
    public void Validate_ShouldClampNegativePageSizeToOne()
    {
        // Arrange
        var query = new PaginationQuery { PageSize = -10 };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(1, query.PageSize);
    }

    [Xunit.Fact]
    public void Validate_ShouldDefaultNullSortOrderToDesc()
    {
        // Arrange
        var query = new PaginationQuery { SortOrder = null };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal("desc", query.SortOrder);
    }

    [Xunit.Fact]
    public void Validate_ShouldKeepValidPageNumber()
    {
        // Arrange
        var query = new PaginationQuery { PageNumber = 5 };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(5, query.PageNumber);
    }

    [Xunit.Fact]
    public void Validate_ShouldKeepPageSizeAtMinimumBoundary()
    {
        // Arrange
        var query = new PaginationQuery { PageSize = 1 };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(1, query.PageSize);
    }

    [Xunit.Fact]
    public void Validate_ShouldKeepPageSizeAtMaximumBoundary()
    {
        // Arrange
        var query = new PaginationQuery { PageSize = 100 };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(100, query.PageSize);
    }

    [Xunit.Fact]
    public void Validate_ShouldHandleMultipleInvalidValues()
    {
        // Arrange
        var query = new PaginationQuery
        {
            PageNumber = -1,
            PageSize = 200,
            SortOrder = "invalid"
        };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(1, query.PageNumber);
        Xunit.Assert.Equal(100, query.PageSize);
        Xunit.Assert.Equal("desc", query.SortOrder);
    }

    [Xunit.Fact]
    public void Validate_ShouldDefaultEmptySortOrderToDesc()
    {
        // Arrange
        var query = new PaginationQuery { SortOrder = "" };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal("desc", query.SortOrder);
    }

    [Xunit.Theory]
    [Xunit.InlineData("Asc")]
    [Xunit.InlineData("Desc")]
    [Xunit.InlineData("aSc")]
    [Xunit.InlineData("DeSc")]
    public void Validate_ShouldAcceptMixedCaseSortOrders(string sortOrder)
    {
        // Arrange
        var query = new PaginationQuery { SortOrder = sortOrder };

        // Act
        query.Validate();

        // Assert
        Xunit.Assert.Equal(sortOrder, query.SortOrder);
    }
}
