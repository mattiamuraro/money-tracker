using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.GetPayment;

public class GetPaymentQueryHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    [Fact]
    public void Constructor_Should_Set_DbContext()
    {
        // Arrange
        using var db = CreateDbContext();

        // Act
        var handler = new GetPaymentQueryHandler(db);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Payments_When_No_Filters_Applied()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Restaurant",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Id()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var targetId = Guid.NewGuid();
        var payment1 = new Payment
        {
            Id = targetId,
            Description = "Groceries",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Restaurant",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { Id = targetId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(targetId, result.Items[0].Id);
        Assert.Equal("Groceries", result.Items[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Year_And_Month()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "January Payment",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "February Payment",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "January 2023 Payment",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 75m,
            Date = new DateTime(2023, 1, 20)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { Year = 2024, Month = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("January Payment", result.Items[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Not_Filter_When_Year_Only_Provided()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "2024 Payment",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "2023 Payment",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2023, 2, 10)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { Year = 2024 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Handle_Should_Not_Filter_When_Month_Only_Provided()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "January 2024 Payment",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "February 2024 Payment",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "January 2023 Payment",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 75m,
            Date = new DateTime(2023, 1, 20)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { Month = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_CategoryFilter()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category1 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var category2 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Transport",
            Code = "TRAN"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            PaymentCategoryId = category1.Id,
            PaymentCategory = category1,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Bus Ticket",
            PaymentCategoryId = category2.Id,
            PaymentCategory = category2,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.PaymentCategories.AddRange(category1, category2);
        dbContext.Payments.AddRange(payment1, payment2);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { CategoryFilter = "Food" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("Food", result.Items[0].Category);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_DescriptionFilter()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries at supermarket",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Restaurant dinner",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { DescriptionFilter = "supermarket" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Contains("supermarket", result.Items[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_CategoryId()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var targetCategoryId = Guid.NewGuid();
        var category1 = new PaymentCategory
        {
            Id = targetCategoryId,
            Name = "Food",
            Code = "FOOD"
        };
        var category2 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Transport",
            Code = "TRAN"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            PaymentCategoryId = category1.Id,
            PaymentCategory = category1,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Bus Ticket",
            PaymentCategoryId = category2.Id,
            PaymentCategory = category2,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.PaymentCategories.AddRange(category1, category2);
        dbContext.Payments.AddRange(payment1, payment2);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { CategoryId = targetCategoryId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(targetCategoryId, result.Items[0].PaymentCategoryId);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_MinAmount()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Expensive",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Cheap",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { MinAmount = 75m };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(100m, result.Items[0].Amount);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_MaxAmount()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Expensive",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Cheap",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { MaxAmount = 75m };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(50m, result.Items[0].Amount);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_MinAmount_And_MaxAmount()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "High",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 150m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Medium",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 2, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Low",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { MinAmount = 75m, MaxAmount = 125m };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(100m, result.Items[0].Amount);
    }

    [Fact]
    public async Task Handle_Should_Apply_Pagination()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        for (int i = 1; i <= 25; i++)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Description = $"Payment {i}",
                PaymentCategoryId = category.Id,
                PaymentCategory = category,
                Amount = i * 10m,
                Date = new DateTime(2024, 1, i % 28 + 1)
            };
            dbContext.Payments.Add(payment);
        }
        dbContext.PaymentCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { PageNumber = 2, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25, result.TotalItems);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Amount_Ascending()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 1",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 2",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 3",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 75m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { SortBy = "amount", SortOrder = "asc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(50m, result.Items[0].Amount);
        Assert.Equal(75m, result.Items[1].Amount);
        Assert.Equal(100m, result.Items[2].Amount);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Amount_Descending()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 1",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 2",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 3",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 75m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { SortBy = "amount", SortOrder = "desc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(100m, result.Items[0].Amount);
        Assert.Equal(75m, result.Items[1].Amount);
        Assert.Equal(50m, result.Items[2].Amount);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Description_Ascending()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Charlie",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Alpha",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Bravo",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 75m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { SortBy = "description", SortOrder = "asc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Alpha", result.Items[0].Description);
        Assert.Equal("Bravo", result.Items[1].Description);
        Assert.Equal("Charlie", result.Items[2].Description);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Description_Descending()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Charlie",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Alpha",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Bravo",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 75m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { SortBy = "description", SortOrder = "desc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Charlie", result.Items[0].Description);
        Assert.Equal("Bravo", result.Items[1].Description);
        Assert.Equal("Alpha", result.Items[2].Description);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Category_Ascending()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category1 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var category2 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Transport",
            Code = "TRAN"
        };
        var category3 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Entertainment",
            Code = "ENT"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 1",
            PaymentCategoryId = category1.Id,
            PaymentCategory = category1,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 2",
            PaymentCategoryId = category2.Id,
            PaymentCategory = category2,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 3",
            PaymentCategoryId = category3.Id,
            PaymentCategory = category3,
            Amount = 75m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.PaymentCategories.AddRange(category1, category2, category3);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { SortBy = "category", SortOrder = "asc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Entertainment", result.Items[0].Category);
        Assert.Equal("Food", result.Items[1].Category);
        Assert.Equal("Transport", result.Items[2].Category);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Category_Descending()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category1 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var category2 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Transport",
            Code = "TRAN"
        };
        var category3 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Entertainment",
            Code = "ENT"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 1",
            PaymentCategoryId = category1.Id,
            PaymentCategory = category1,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 2",
            PaymentCategoryId = category2.Id,
            PaymentCategory = category2,
            Amount = 50m,
            Date = new DateTime(2024, 2, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 3",
            PaymentCategoryId = category3.Id,
            PaymentCategory = category3,
            Amount = 75m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.PaymentCategories.AddRange(category1, category2, category3);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { SortBy = "category", SortOrder = "desc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Transport", result.Items[0].Category);
        Assert.Equal("Food", result.Items[1].Category);
        Assert.Equal("Entertainment", result.Items[2].Category);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Date_Ascending_When_SortBy_Not_Specified()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 1",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 3, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 2",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 1, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 3",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 75m,
            Date = new DateTime(2024, 2, 5)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { SortOrder = "asc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(new DateTime(2024, 1, 10), result.Items[0].Date);
        Assert.Equal(new DateTime(2024, 2, 5), result.Items[1].Date);
        Assert.Equal(new DateTime(2024, 3, 15), result.Items[2].Date);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Date_Descending_When_SortBy_Not_Specified()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 1",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 3, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 2",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 1, 10)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Payment 3",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 75m,
            Date = new DateTime(2024, 2, 5)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { SortOrder = "desc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(new DateTime(2024, 3, 15), result.Items[0].Date);
        Assert.Equal(new DateTime(2024, 2, 5), result.Items[1].Date);
        Assert.Equal(new DateTime(2024, 1, 10), result.Items[2].Date);
    }

    [Fact]
    public async Task Handle_Should_Map_PaymentRow_Properties_Correctly()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD"
        };
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Test Payment",
            PaymentCategoryId = categoryId,
            PaymentCategory = category,
            Amount = 123.45m,
            Date = new DateTime(2024, 6, 15),
            IsOneShot = true
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        var paymentRow = result.Items[0];
        Assert.Equal(paymentId, paymentRow.Id);
        Assert.Equal("Test Payment", paymentRow.Description);
        Assert.Equal(categoryId, paymentRow.PaymentCategoryId);
        Assert.Equal("Food", paymentRow.Category);
        Assert.Equal(123.45m, paymentRow.Amount);
        Assert.Equal(new DateTime(2024, 6, 15), paymentRow.Date);
        Assert.True(paymentRow.IsOneShot);
        Assert.Null(paymentRow.ForecastOccurrenceId);
        Assert.Null(paymentRow.ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Combine_Multiple_Filters()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Restaurant",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 50m,
            Date = new DateTime(2024, 1, 20)
        };
        var payment3 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Supermarket",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 150m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.AddRange(payment1, payment2, payment3);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery
        {
            Year = 2024,
            Month = 1,
            MinAmount = 75m,
            DescriptionFilter = "Groceries"
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("Groceries", result.Items[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_No_Payments_Match_Filters()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            PaymentCategoryId = category.Id,
            PaymentCategory = category,
            Amount = 100m,
            Date = new DateTime(2024, 1, 15)
        };
        dbContext.PaymentCategories.Add(category);
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery { Year = 2023, Month = 12 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_Database_Is_Empty()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_Should_Respect_CancellationToken()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetPaymentQueryHandler(dbContext);
        var query = new GetPaymentQuery();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await handler.Handle(query, cts.Token));
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
