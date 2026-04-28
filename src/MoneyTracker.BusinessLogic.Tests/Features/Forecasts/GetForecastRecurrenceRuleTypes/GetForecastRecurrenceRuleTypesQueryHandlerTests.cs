using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.GetForecastRecurrenceRuleTypes;

public class GetForecastRecurrenceRuleTypesQueryHandlerTests
{
    [Fact]
    public void Constructor_Should_Set_DbContext()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();

        // Act
        var handler = new GetForecastRecurrenceRuleTypesQueryHandler(dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Collection_When_No_Types_Exist()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastRecurrenceRuleTypesQueryHandler(dbContext);
        var query = new GetForecastRecurrenceRuleTypesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Types_Ordered_By_OrderIndex()
    {
        // Arrange
        var typeId1 = Guid.NewGuid();
        var typeId2 = Guid.NewGuid();
        var typeId3 = Guid.NewGuid();

        var dbContext = CreateInMemoryDbContext();
        dbContext.ForecastRecurrenceRuleTypes.AddRange(
            new ForecastRecurrenceRuleType
            {
                Id = typeId2,
                Name = "Week",
                Code = "W",
                OrderIndex = 2
            },
            new ForecastRecurrenceRuleType
            {
                Id = typeId1,
                Name = "Day",
                Code = "D",
                OrderIndex = 1
            },
            new ForecastRecurrenceRuleType
            {
                Id = typeId3,
                Name = "Month",
                Code = "M",
                OrderIndex = 3
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRecurrenceRuleTypesQueryHandler(dbContext);
        var query = new GetForecastRecurrenceRuleTypesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal(typeId1, result[0].Id);
        Assert.Equal("Day", result[0].Name);
        Assert.Equal("D", result[0].Code);

        Assert.Equal(typeId2, result[1].Id);
        Assert.Equal("Week", result[1].Name);
        Assert.Equal("W", result[1].Code);

        Assert.Equal(typeId3, result[2].Id);
        Assert.Equal("Month", result[2].Name);
        Assert.Equal("M", result[2].Code);
    }

    [Fact]
    public async Task Handle_Should_Return_Single_Type()
    {
        // Arrange
        var typeId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        dbContext.ForecastRecurrenceRuleTypes.Add(new ForecastRecurrenceRuleType
        {
            Id = typeId,
            Name = "One Time",
            Code = "O",
            OrderIndex = 0
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRecurrenceRuleTypesQueryHandler(dbContext);
        var query = new GetForecastRecurrenceRuleTypesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(typeId, result[0].Id);
        Assert.Equal("One Time", result[0].Name);
        Assert.Equal("O", result[0].Code);
    }

    [Fact]
    public async Task Handle_Should_Handle_Null_OrderIndex()
    {
        // Arrange
        var typeId1 = Guid.NewGuid();
        var typeId2 = Guid.NewGuid();

        var dbContext = CreateInMemoryDbContext();
        dbContext.ForecastRecurrenceRuleTypes.AddRange(
            new ForecastRecurrenceRuleType
            {
                Id = typeId1,
                Name = "Type With OrderIndex",
                Code = "T1",
                OrderIndex = 1
            },
            new ForecastRecurrenceRuleType
            {
                Id = typeId2,
                Name = "Type Without OrderIndex",
                Code = "T2",
                OrderIndex = null
            });
        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRecurrenceRuleTypesQueryHandler(dbContext);
        var query = new GetForecastRecurrenceRuleTypesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_Should_Respect_Cancellation_Token()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastRecurrenceRuleTypesQueryHandler(dbContext);
        var query = new GetForecastRecurrenceRuleTypesQuery();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await handler.Handle(query, cts.Token));
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
