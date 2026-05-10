using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.GetIncome;

public class GetIncomeQueryHandlerTests
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
        var handler = new GetIncomeQueryHandler(db);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Incomes_When_No_Filters_Applied()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Salary",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Bonus",
            Amount = 1000m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.Incomes.AddRange(income1, income2);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery();

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
        var targetId = Guid.NewGuid();
        var income1 = new Income
        {
            Id = targetId,
            Description = "Salary",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Bonus",
            Amount = 1000m,
            Date = new DateTime(2024, 2, 10)
        };
        dbContext.Incomes.AddRange(income1, income2);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { Id = targetId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(targetId, result.Items[0].Id);
        Assert.Equal("Salary", result.Items[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Year_And_Month()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "January Income",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "February Income",
            Amount = 3000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "January Income 2023",
            Amount = 4000m,
            Date = new DateTime(2023, 1, 20)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { Year = 2024, Month = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("January Income", result.Items[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_DescriptionFilter()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Salary Payment",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Bonus",
            Amount = 1000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Freelance Payment",
            Amount = 2000m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { DescriptionFilter = "Salary" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.All(result.Items, item => Assert.StartsWith("Salary", item.Description));
    }

    [Fact]
    public async Task Handle_Should_Filter_By_MinAmount()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "High Income",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Medium Income",
            Amount = 3000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Low Income",
            Amount = 1000m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { MinAmount = 3000m };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.True(item.Amount >= 3000m));
    }

    [Fact]
    public async Task Handle_Should_Filter_By_MaxAmount()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "High Income",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Medium Income",
            Amount = 3000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Low Income",
            Amount = 1000m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { MaxAmount = 3000m };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.True(item.Amount <= 3000m));
    }

    [Fact]
    public async Task Handle_Should_Apply_Combined_Filters()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Salary Payment",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Bonus Payment",
            Amount = 1000m,
            Date = new DateTime(2024, 1, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Freelance Payment",
            Amount = 3000m,
            Date = new DateTime(2024, 2, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery
        {
            Year = 2024,
            Month = 1,
            DescriptionFilter = "Salary",
            MinAmount = 2000m
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("Salary Payment", result.Items[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Amount_Ascending()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "High",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Low",
            Amount = 1000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Medium",
            Amount = 3000m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { SortBy = "amount", SortOrder = "asc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1000m, result.Items[0].Amount);
        Assert.Equal(3000m, result.Items[1].Amount);
        Assert.Equal(5000m, result.Items[2].Amount);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Amount_Descending()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "High",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Low",
            Amount = 1000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Medium",
            Amount = 3000m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { SortBy = "amount", SortOrder = "desc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(5000m, result.Items[0].Amount);
        Assert.Equal(3000m, result.Items[1].Amount);
        Assert.Equal(1000m, result.Items[2].Amount);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Description_Ascending()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Charlie",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Alpha",
            Amount = 1000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Bravo",
            Amount = 3000m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { SortBy = "description", SortOrder = "asc" };

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
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Charlie",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Alpha",
            Amount = 1000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Bravo",
            Amount = 3000m,
            Date = new DateTime(2024, 3, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { SortBy = "description", SortOrder = "desc" };

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
    public async Task Handle_Should_Sort_By_Date_Ascending_By_Default()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Latest",
            Amount = 5000m,
            Date = new DateTime(2024, 3, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Earliest",
            Amount = 1000m,
            Date = new DateTime(2024, 1, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Middle",
            Amount = 3000m,
            Date = new DateTime(2024, 2, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { SortOrder = "asc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Earliest", result.Items[0].Description);
        Assert.Equal("Middle", result.Items[1].Description);
        Assert.Equal("Latest", result.Items[2].Description);
    }

    [Fact]
    public async Task Handle_Should_Sort_By_Date_Descending_By_Default()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Latest",
            Amount = 5000m,
            Date = new DateTime(2024, 3, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Earliest",
            Amount = 1000m,
            Date = new DateTime(2024, 1, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Middle",
            Amount = 3000m,
            Date = new DateTime(2024, 2, 5)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { SortOrder = "desc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Latest", result.Items[0].Description);
        Assert.Equal("Middle", result.Items[1].Description);
        Assert.Equal("Earliest", result.Items[2].Description);
    }

    [Fact]
    public async Task Handle_Should_Apply_Pagination()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        for (int i = 1; i <= 25; i++)
        {
            dbContext.Incomes.Add(new Income
            {
                Id = Guid.NewGuid(),
                Description = $"Income {i}",
                Amount = i * 100m,
                Date = new DateTime(2024, 1, i)
            });
        }
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25, result.TotalItems);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Handle_Should_Return_Correct_Page()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        for (int i = 1; i <= 25; i++)
        {
            dbContext.Incomes.Add(new Income
            {
                Id = Guid.NewGuid(),
                Description = $"Income {i}",
                Amount = i * 100m,
                Date = new DateTime(2024, 1, i)
            });
        }
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { PageNumber = 2, PageSize = 10, SortBy = "amount", SortOrder = "asc" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25, result.TotalItems);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(1100m, result.Items[0].Amount);
    }

    [Fact]
    public async Task Handle_Should_Return_Income_With_ForecastOccurrence()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastOccurrenceId = Guid.NewGuid();
        var forecastOccurrence = new ForecastOccurrence
        {
            Id = forecastOccurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            Description = "Expected Income",
            Amount = 5000m,
            IsIncome = true,
            ExpectedDate = new DateOnly(2024, 1, 15)
        };
        dbContext.ForecastOccurrences.Add(forecastOccurrence);

        var income = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Salary",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15),
            ForecastOccurrenceId = forecastOccurrenceId,
            ForecastOccurrence = forecastOccurrence
        };
        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(forecastOccurrenceId, result.Items[0].ForecastOccurrenceId);
        Assert.Equal(new DateOnly(2024, 1, 15), result.Items[0].ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_ForecastExpectedDate_When_ForecastOccurrence_Is_Null()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Salary",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15),
            ForecastOccurrenceId = Guid.NewGuid()
        };
        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.NotNull(result.Items[0].ForecastOccurrenceId);
        Assert.Null(result.Items[0].ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Soft_Deleted_Incomes()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Active Income",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Deleted Income",
            Amount = 3000m,
            Date = new DateTime(2024, 2, 10),
            DeletedAt = DateTime.UtcNow,
            IsDeleted = true
        };
        dbContext.Incomes.AddRange(income1, income2);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("Active Income", result.Items[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Respect_CancellationToken()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Incomes_Match()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Salary",
            Amount = 5000m,
            Date = new DateTime(2024, 1, 15)
        };
        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { Id = Guid.NewGuid() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_Should_Project_All_Properties_Correctly()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var incomeId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 1234.56m,
            Date = new DateTime(2024, 3, 15, 10, 30, 0)
        };
        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        var item = result.Items[0];
        Assert.Equal(incomeId, item.Id);
        Assert.Equal("Test Income", item.Description);
        Assert.Equal(1234.56m, item.Amount);
        Assert.Equal(new DateTime(2024, 3, 15, 10, 30, 0), item.Date);
        Assert.Null(item.ForecastOccurrenceId);
        Assert.Null(item.ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Ignore_Null_DescriptionFilter()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Income 1",
            Amount = 100m,
            Date = new DateTime(2024, 1, 1)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Income 2",
            Amount = 200m,
            Date = new DateTime(2024, 1, 2)
        };
        dbContext.Incomes.AddRange(income1, income2);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { DescriptionFilter = null };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task Handle_Should_Ignore_Empty_DescriptionFilter()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Income 1",
            Amount = 100m,
            Date = new DateTime(2024, 1, 1)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Income 2",
            Amount = 200m,
            Date = new DateTime(2024, 1, 2)
        };
        dbContext.Incomes.AddRange(income1, income2);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { DescriptionFilter = "" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task Handle_Should_Ignore_Whitespace_DescriptionFilter()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Income 1",
            Amount = 100m,
            Date = new DateTime(2024, 1, 1)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "Income 2",
            Amount = 200m,
            Date = new DateTime(2024, 1, 2)
        };
        dbContext.Incomes.AddRange(income1, income2);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { DescriptionFilter = "   " };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task Handle_Should_Not_Filter_By_Year_When_Month_Is_Not_Provided()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "January 2024",
            Amount = 1000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "February 2024",
            Amount = 2000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "January 2023",
            Amount = 3000m,
            Date = new DateTime(2023, 1, 20)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { Year = 2024 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task Handle_Should_Not_Filter_By_Month_When_Year_Is_Not_Provided()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "January 2024",
            Amount = 1000m,
            Date = new DateTime(2024, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "February 2024",
            Amount = 2000m,
            Date = new DateTime(2024, 2, 10)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            Description = "January 2023",
            Amount = 3000m,
            Date = new DateTime(2023, 1, 20)
        };
        dbContext.Incomes.AddRange(income1, income2, income3);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeQueryHandler(dbContext);
        var query = new GetIncomeQuery { Month = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.Items.Count);
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
