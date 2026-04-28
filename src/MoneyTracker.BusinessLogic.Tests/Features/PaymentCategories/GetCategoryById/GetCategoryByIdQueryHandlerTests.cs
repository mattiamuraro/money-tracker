using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.PaymentCategories.GetCategoryById;

public class GetCategoryByIdQueryHandlerTests
{
    [Fact]
    public void Constructor_Should_Set_DbContext()
    {
        // Arrange
        var mockDbContext = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new MoneyTrackerDbContext(mockDbContext, null!);

        // Act
        var handler = new GetCategoryByIdQueryHandler(dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_Category_When_Exists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD"
        };
        dbContext.PaymentCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var handler = new GetCategoryByIdQueryHandler(dbContext);
        var query = new GetCategoryByIdQuery(categoryId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.Id);
        Assert.Equal("Food", result.Name);
        Assert.Equal("FOOD", result.Code);
    }

    [Fact]
    public async Task Handle_Should_Throw_EntityNotFoundException_When_Category_Not_Found()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetCategoryByIdQueryHandler(dbContext);
        var query = new GetCategoryByIdQuery(categoryId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(query, CancellationToken.None));
        Assert.Equal($"Payment category with id {categoryId} not found", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_Respect_CancellationToken()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetCategoryByIdQueryHandler(dbContext);
        var query = new GetCategoryByIdQuery(categoryId);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_Should_Return_Category_With_All_Properties_Mapped()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Entertainment",
            Code = "ENT"
        };
        dbContext.PaymentCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var handler = new GetCategoryByIdQueryHandler(dbContext);
        var query = new GetCategoryByIdQuery(categoryId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Code, result.Code);
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
