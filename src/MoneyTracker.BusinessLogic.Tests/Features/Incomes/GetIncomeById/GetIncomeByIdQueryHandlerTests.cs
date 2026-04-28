using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.GetIncomeById;

public class GetIncomeByIdQueryHandlerTests
{
    [Fact]
    public void Constructor_Should_Set_DbContext()
    {
        // Arrange
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>().Options,
            null!);

        // Act
        var handler = new GetIncomeByIdQueryHandler(mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_Income_When_Exists()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 1000.50m,
            Date = new DateTime(2024, 1, 15),
            ForecastOccurrenceId = null
        };
        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeByIdQueryHandler(dbContext);
        var query = new GetIncomeByIdQuery(incomeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(incomeId, result.Id);
        Assert.Equal("Test Income", result.Description);
        Assert.Equal(1000.50m, result.Amount);
        Assert.Equal(new DateTime(2024, 1, 15), result.Date);
        Assert.Null(result.ForecastOccurrenceId);
        Assert.Null(result.ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Return_Income_With_ForecastOccurrence_When_Linked()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var forecastOccurrenceId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var forecastOccurrence = new ForecastOccurrence
        {
            Id = forecastOccurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            Description = "Forecast",
            Amount = 1000m,
            IsIncome = true,
            ExpectedDate = new DateOnly(2024, 1, 15)
        };
        dbContext.ForecastOccurrences.Add(forecastOccurrence);

        var income = new Income
        {
            Id = incomeId,
            Description = "Salary",
            Amount = 2500.00m,
            Date = new DateTime(2024, 1, 15),
            ForecastOccurrenceId = forecastOccurrenceId,
            ForecastOccurrence = forecastOccurrence
        };
        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeByIdQueryHandler(dbContext);
        var query = new GetIncomeByIdQuery(incomeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(incomeId, result.Id);
        Assert.Equal("Salary", result.Description);
        Assert.Equal(2500.00m, result.Amount);
        Assert.Equal(new DateTime(2024, 1, 15), result.Date);
        Assert.Equal(forecastOccurrenceId, result.ForecastOccurrenceId);
        Assert.Equal(new DateOnly(2024, 1, 15), result.ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Throw_EntityNotFoundException_When_Income_Not_Found()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetIncomeByIdQueryHandler(dbContext);
        var query = new GetIncomeByIdQuery(incomeId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(query, CancellationToken.None));
        Assert.Equal($"Income with id {incomeId} not found", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_Respect_CancellationToken()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetIncomeByIdQueryHandler(dbContext);
        var query = new GetIncomeByIdQuery(incomeId);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_Should_Return_Income_With_Null_ForecastExpectedDate_When_ForecastOccurrence_Is_Null()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var income = new Income
        {
            Id = incomeId,
            Description = "Bonus",
            Amount = 500.00m,
            Date = new DateTime(2024, 2, 20),
            ForecastOccurrenceId = Guid.NewGuid()
        };
        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeByIdQueryHandler(dbContext);
        var query = new GetIncomeByIdQuery(incomeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(incomeId, result.Id);
        Assert.NotNull(result.ForecastOccurrenceId);
        Assert.Null(result.ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Soft_Deleted_Income()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var income = new Income
        {
            Id = incomeId,
            Description = "Deleted Income",
            Amount = 100.00m,
            Date = new DateTime(2024, 3, 10),
            DeletedAt = DateTime.UtcNow,
            IsDeleted = true
        };
        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync();

        var handler = new GetIncomeByIdQueryHandler(dbContext);
        var query = new GetIncomeByIdQuery(incomeId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(query, CancellationToken.None));
        Assert.Equal($"Income with id {incomeId} not found", exception.Message);
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
