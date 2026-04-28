using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetPendingForecastOccurrences;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using Moq;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.GetPendingForecastOccurrences;

public class GetPendingForecastOccurrencesQueryHandlerTests
{
    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        // Arrange
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>().Options,
            null!);

        // Act
        var handler = new GetPendingForecastOccurrencesQueryHandler(mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Collection_When_No_Occurrences_Exist()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Should_Return_Pending_Occurrences_For_Specified_Month_And_Year()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var occurrenceId = Guid.NewGuid();
        var forecastDefinitionId = Guid.NewGuid();

        dbContext.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = forecastDefinitionId,
            Description = "Test Expense",
            Amount = 100.50m,
            ExpectedDate = new DateOnly(2024, 1, 15),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            PaymentCategoryId = null
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(occurrenceId, result[0].Id);
        Assert.Equal(forecastDefinitionId, result[0].ForecastDefinitionId);
        Assert.Equal("Test Expense", result[0].Description);
        Assert.Equal(100.50m, result[0].Amount);
        Assert.Equal(new DateOnly(2024, 1, 15), result[0].ExpectedDate);
        Assert.False(result[0].IsIncome);
        Assert.Null(result[0].PaymentCategoryId);
        Assert.Null(result[0].Category);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Status_Pending_Only()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();

        dbContext.ForecastOccurrences.AddRange(
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Pending Expense",
                Amount = 100m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Confirmed Expense",
                Amount = 200m,
                ExpectedDate = new DateOnly(2024, 1, 16),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId
            },
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Skipped Expense",
                Amount = 300m,
                ExpectedDate = new DateOnly(2024, 1, 17),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.SkippedId
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Pending Expense", result[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_IsIncome_False()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();

        dbContext.ForecastOccurrences.AddRange(
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Expense",
                Amount = 100m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Income",
                Amount = 200m,
                ExpectedDate = new DateOnly(2024, 1, 16),
                IsIncome = true,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Expense", result[0].Description);
        Assert.False(result[0].IsIncome);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_IsIncome_True()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();

        dbContext.ForecastOccurrences.AddRange(
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Expense",
                Amount = 100m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Income",
                Amount = 200m,
                ExpectedDate = new DateOnly(2024, 1, 16),
                IsIncome = true,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Income", result[0].Description);
        Assert.True(result[0].IsIncome);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Year()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();

        dbContext.ForecastOccurrences.AddRange(
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "2024 Expense",
                Amount = 100m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "2023 Expense",
                Amount = 200m,
                ExpectedDate = new DateOnly(2023, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("2024 Expense", result[0].Description);
        Assert.Equal(2024, result[0].ExpectedDate.Year);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Month()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();

        dbContext.ForecastOccurrences.AddRange(
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "January Expense",
                Amount = 100m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "February Expense",
                Amount = 200m,
                ExpectedDate = new DateOnly(2024, 2, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("January Expense", result[0].Description);
        Assert.Equal(1, result[0].ExpectedDate.Month);
    }

    [Fact]
    public async Task Handle_Should_Order_By_ExpectedDate_Then_By_Description()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        var id4 = Guid.NewGuid();

        dbContext.ForecastOccurrences.AddRange(
            new ForecastOccurrence
            {
                Id = id3,
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Z Description",
                Amount = 100m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            new ForecastOccurrence
            {
                Id = id1,
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "A Description",
                Amount = 200m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            new ForecastOccurrence
            {
                Id = id4,
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "B Description",
                Amount = 300m,
                ExpectedDate = new DateOnly(2024, 1, 20),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            new ForecastOccurrence
            {
                Id = id2,
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "M Description",
                Amount = 400m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);

        // First by date (Jan 15), then alphabetically
        Assert.Equal(id1, result[0].Id);
        Assert.Equal("A Description", result[0].Description);
        Assert.Equal(new DateOnly(2024, 1, 15), result[0].ExpectedDate);

        Assert.Equal(id2, result[1].Id);
        Assert.Equal("M Description", result[1].Description);
        Assert.Equal(new DateOnly(2024, 1, 15), result[1].ExpectedDate);

        Assert.Equal(id3, result[2].Id);
        Assert.Equal("Z Description", result[2].Description);
        Assert.Equal(new DateOnly(2024, 1, 15), result[2].ExpectedDate);

        // Later date (Jan 20)
        Assert.Equal(id4, result[3].Id);
        Assert.Equal("B Description", result[3].Description);
        Assert.Equal(new DateOnly(2024, 1, 20), result[3].ExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_Category_When_PaymentCategory_Is_Null()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();

        dbContext.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = Guid.NewGuid(),
            Description = "Expense without category",
            Amount = 100m,
            ExpectedDate = new DateOnly(2024, 1, 15),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            PaymentCategoryId = null,
            PaymentCategory = null
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Null(result[0].PaymentCategoryId);
        Assert.Null(result[0].Category);
    }

    [Fact]
    public async Task Handle_Should_Return_Category_Name_When_PaymentCategory_Is_Not_Null()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Groceries",
            Code = "GRC"
        };
        dbContext.PaymentCategories.Add(category);

        dbContext.ForecastOccurrences.Add(new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = Guid.NewGuid(),
            Description = "Expense with category",
            Amount = 100m,
            ExpectedDate = new DateOnly(2024, 1, 15),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
            PaymentCategoryId = categoryId
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(categoryId, result[0].PaymentCategoryId);
        Assert.Equal("Groceries", result[0].Category);
    }

    [Fact]
    public async Task Handle_Should_Return_Multiple_Occurrences_With_Mixed_Categories()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();

        dbContext.PaymentCategories.AddRange(
            new PaymentCategory
            {
                Id = categoryId1,
                Name = "Groceries",
                Code = "GRC"
            },
            new PaymentCategory
            {
                Id = categoryId2,
                Name = "Utilities",
                Code = "UTL"
            });

        dbContext.ForecastOccurrences.AddRange(
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Expense with category 1",
                Amount = 100m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
                PaymentCategoryId = categoryId1
            },
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Expense without category",
                Amount = 200m,
                ExpectedDate = new DateOnly(2024, 1, 16),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
                PaymentCategoryId = null
            },
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Expense with category 2",
                Amount = 300m,
                ExpectedDate = new DateOnly(2024, 1, 17),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
                PaymentCategoryId = categoryId2
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal("Groceries", result[0].Category);
        Assert.Equal(categoryId1, result[0].PaymentCategoryId);

        Assert.Null(result[1].Category);
        Assert.Null(result[1].PaymentCategoryId);

        Assert.Equal("Utilities", result[2].Category);
        Assert.Equal(categoryId2, result[2].PaymentCategoryId);
    }

    [Fact]
    public async Task Handle_Should_Respect_Cancellation_Token()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_Should_Return_All_Pending_Occurrences_For_Complex_Scenario()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();

        dbContext.PaymentCategories.Add(new PaymentCategory
        {
            Id = categoryId,
            Name = "Entertainment",
            Code = "ENT"
        });

        // Add various occurrences with different combinations
        dbContext.ForecastOccurrences.AddRange(
            // Matching - Pending, Expense, Jan 2024
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Movie Tickets",
                Amount = 50m,
                ExpectedDate = new DateOnly(2024, 1, 5),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
                PaymentCategoryId = categoryId
            },
            // Not matching - Different status
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Confirmed Expense",
                Amount = 100m,
                ExpectedDate = new DateOnly(2024, 1, 10),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId
            },
            // Not matching - Different IsIncome
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Salary",
                Amount = 5000m,
                ExpectedDate = new DateOnly(2024, 1, 15),
                IsIncome = true,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            // Not matching - Different month
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "February Expense",
                Amount = 200m,
                ExpectedDate = new DateOnly(2024, 2, 5),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            // Not matching - Different year
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "2023 Expense",
                Amount = 150m,
                ExpectedDate = new DateOnly(2023, 1, 5),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            },
            // Matching - Pending, Expense, Jan 2024, no category
            new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = Guid.NewGuid(),
                Description = "Miscellaneous Expense",
                Amount = 75m,
                ExpectedDate = new DateOnly(2024, 1, 20),
                IsIncome = false,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId,
                PaymentCategoryId = null
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetPendingForecastOccurrencesQueryHandler(dbContext);
        var query = new GetPendingForecastOccurrencesQuery(2024, 1, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        // Ordered by date
        Assert.Equal("Movie Tickets", result[0].Description);
        Assert.Equal(50m, result[0].Amount);
        Assert.Equal(new DateOnly(2024, 1, 5), result[0].ExpectedDate);
        Assert.Equal("Entertainment", result[0].Category);

        Assert.Equal("Miscellaneous Expense", result[1].Description);
        Assert.Equal(75m, result[1].Amount);
        Assert.Equal(new DateOnly(2024, 1, 20), result[1].ExpectedDate);
        Assert.Null(result[1].Category);
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
