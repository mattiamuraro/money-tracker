using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.PaymentCategories.GetAllCategories;

public class GetAllCategoriesQueryHandlerTests
{
    [Fact]
    public void Constructor_Should_Set_DbContext()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();

        // Act
        var handler = new GetAllCategoriesQueryHandler(dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Collection_When_No_Categories_Exist()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetAllCategoriesQueryHandler(dbContext);
        var query = new GetAllCategoriesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Should_Return_Single_Category()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD",
            CreatedAt = createdAt
        };
        dbContext.PaymentCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var handler = new GetAllCategoriesQueryHandler(dbContext);
        var query = new GetAllCategoriesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);

        var resultCategory = resultList[0];
        Assert.Equal(categoryId, resultCategory.Id);
        Assert.Equal("Food", resultCategory.Name);
        Assert.Equal("FOOD", resultCategory.Code);
        Assert.Equal(createdAt, resultCategory.CreatedAt);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Categories()
    {
        // Arrange
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var createdAt1 = DateTime.UtcNow.AddDays(-1);
        var createdAt2 = DateTime.UtcNow;
        var dbContext = CreateInMemoryDbContext();

        var category1 = new PaymentCategory
        {
            Id = categoryId1,
            Name = "Food",
            Code = "FOOD",
            CreatedAt = createdAt1
        };
        var category2 = new PaymentCategory
        {
            Id = categoryId2,
            Name = "Transport",
            Code = "TRNSP",
            CreatedAt = createdAt2
        };
        dbContext.PaymentCategories.Add(category1);
        dbContext.PaymentCategories.Add(category2);
        await dbContext.SaveChangesAsync();

        var handler = new GetAllCategoriesQueryHandler(dbContext);
        var query = new GetAllCategoriesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);

        var firstCategory = resultList.First(c => c.Id == categoryId1);
        Assert.Equal("Food", firstCategory.Name);
        Assert.Equal("FOOD", firstCategory.Code);
        Assert.Equal(createdAt1, firstCategory.CreatedAt);

        var secondCategory = resultList.First(c => c.Id == categoryId2);
        Assert.Equal("Transport", secondCategory.Name);
        Assert.Equal("TRNSP", secondCategory.Code);
        Assert.Equal(createdAt2, secondCategory.CreatedAt);
    }

    [Fact]
    public async Task Handle_Should_Map_All_Properties_Correctly()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var createdAt = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Healthcare",
            Code = "HELTH",
            CreatedAt = createdAt
        };
        dbContext.PaymentCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var handler = new GetAllCategoriesQueryHandler(dbContext);
        var query = new GetAllCategoriesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var resultCategory = result.Single();
        Assert.Equal(categoryId, resultCategory.Id);
        Assert.Equal("Healthcare", resultCategory.Name);
        Assert.Equal("HELTH", resultCategory.Code);
        Assert.Equal(createdAt, resultCategory.CreatedAt);
    }

    [Fact]
    public async Task Handle_Should_Return_Multiple_Categories_With_Different_Properties()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var categories = new List<PaymentCategory>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Food",
                Code = "FOOD",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Transport",
                Code = "TRNSP",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Entertainment",
                Code = "ENT",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };
        dbContext.PaymentCategories.AddRange(categories);
        await dbContext.SaveChangesAsync();

        var handler = new GetAllCategoriesQueryHandler(dbContext);
        var query = new GetAllCategoriesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);

        foreach (var category in categories)
        {
            var resultCategory = resultList.First(r => r.Id == category.Id);
            Assert.Equal(category.Name, resultCategory.Name);
            Assert.Equal(category.Code, resultCategory.Code);
            Assert.Equal(category.CreatedAt, resultCategory.CreatedAt);
        }
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
