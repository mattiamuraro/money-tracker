using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using Moq;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.GetForecastRows;

public class GetForecastRowsQueryHandlerTests
{
    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        // Arrange
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>().Options,
            null!);

        // Act
        var handler = new GetForecastRowsQueryHandler(mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Throw_BadRequestException_When_EndDate_Is_Before_StartDate()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 12, 31),
            new DateOnly(2024, 1, 1));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(query, CancellationToken.None));

        Assert.Equal("End date must be greater than or equal to start date.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_Throw_BadRequestException_When_DateRange_Exceeds_366_Days()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 1, 1),
            new DateOnly(2025, 1, 2));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(query, CancellationToken.None));

        Assert.Equal("Date range cannot exceed 366 days.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_DateRange_Is_Exactly_366_Days()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 1, 1),
            new DateOnly(2025, 1, 1));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Pending_Occurrences_Exist()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Should_Return_Pending_Occurrences_Within_Date_Range()
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

        var forecastId = Guid.NewGuid();
        var occurrence1 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Monthly Groceries",
            Amount = 500.00m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = false,
            PaymentCategoryId = categoryId,
            PaymentCategory = category,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence1);

        var occurrence2 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Weekly Shopping",
            Amount = 100.00m,
            ExpectedDate = new DateOnly(2024, 6, 22),
            IsIncome = false,
            PaymentCategoryId = categoryId,
            PaymentCategory = category,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence2);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(occurrence1.Id, result[0].Id);
        Assert.Equal(occurrence2.Id, result[1].Id);
        Assert.Equal("Monthly Groceries", result[0].Description);
        Assert.Equal("Weekly Shopping", result[1].Description);
        Assert.Equal(500.00m, result[0].Amount);
        Assert.Equal(100.00m, result[1].Amount);
        Assert.Equal(new DateOnly(2024, 6, 15), result[0].Date);
        Assert.Equal(new DateOnly(2024, 6, 22), result[1].Date);
        Assert.False(result[0].IsIncome);
        Assert.False(result[1].IsIncome);
        Assert.Equal(categoryId, result[0].PaymentCategoryId);
        Assert.Equal(categoryId, result[1].PaymentCategoryId);
        Assert.Equal("Groceries", result[0].Category);
        Assert.Equal("Groceries", result[1].Category);
    }

    [Fact]
    public async Task Handle_Should_Exclude_Occurrences_Outside_Date_Range()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastId = Guid.NewGuid();

        var occurrenceBefore = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Before Range",
            Amount = 100.00m,
            ExpectedDate = new DateOnly(2024, 5, 31),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrenceBefore);

        var occurrenceInRange = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "In Range",
            Amount = 200.00m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrenceInRange);

        var occurrenceAfter = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "After Range",
            Amount = 300.00m,
            ExpectedDate = new DateOnly(2024, 7, 1),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrenceAfter);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(occurrenceInRange.Id, result[0].Id);
        Assert.Equal("In Range", result[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Include_Occurrences_On_Boundary_Dates()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastId = Guid.NewGuid();

        var occurrenceOnStartDate = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "On Start Date",
            Amount = 100.00m,
            ExpectedDate = new DateOnly(2024, 6, 1),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrenceOnStartDate);

        var occurrenceOnEndDate = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "On End Date",
            Amount = 200.00m,
            ExpectedDate = new DateOnly(2024, 6, 30),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrenceOnEndDate);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Description == "On Start Date");
        Assert.Contains(result, r => r.Description == "On End Date");
    }

    [Fact]
    public async Task Handle_Should_Only_Return_Pending_Occurrences()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastId = Guid.NewGuid();

        var pendingOccurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Pending",
            Amount = 100.00m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(pendingOccurrence);

        var confirmedOccurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Confirmed",
            Amount = 200.00m,
            ExpectedDate = new DateOnly(2024, 6, 16),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId
        };
        dbContext.ForecastOccurrences.Add(confirmedOccurrence);

        var skippedOccurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Skipped",
            Amount = 300.00m,
            ExpectedDate = new DateOnly(2024, 6, 17),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.SkippedId
        };
        dbContext.ForecastOccurrences.Add(skippedOccurrence);

        var cancelledOccurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Cancelled",
            Amount = 400.00m,
            ExpectedDate = new DateOnly(2024, 6, 18),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId
        };
        dbContext.ForecastOccurrences.Add(cancelledOccurrence);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(pendingOccurrence.Id, result[0].Id);
        Assert.Equal("Pending", result[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Order_Results_By_ExpectedDate_Then_Description()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastId = Guid.NewGuid();

        var occurrence1 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Zebra",
            Amount = 100.00m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence1);

        var occurrence2 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Apple",
            Amount = 200.00m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence2);

        var occurrence3 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Banana",
            Amount = 300.00m,
            ExpectedDate = new DateOnly(2024, 6, 10),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence3);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("Banana", result[0].Description);
        Assert.Equal(new DateOnly(2024, 6, 10), result[0].Date);
        Assert.Equal("Apple", result[1].Description);
        Assert.Equal(new DateOnly(2024, 6, 15), result[1].Date);
        Assert.Equal("Zebra", result[2].Description);
        Assert.Equal(new DateOnly(2024, 6, 15), result[2].Date);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_Category_When_PaymentCategory_Is_Null()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastId = Guid.NewGuid();

        var occurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "No Category",
            Amount = 100.00m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = true,
            PaymentCategoryId = null,
            PaymentCategory = null,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Null(result[0].Category);
        Assert.Null(result[0].PaymentCategoryId);
    }

    [Fact]
    public async Task Handle_Should_Return_Income_Occurrences()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastId = Guid.NewGuid();

        var incomeOccurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Salary",
            Amount = 5000.00m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = true,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(incomeOccurrence);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(incomeOccurrence.Id, result[0].Id);
        Assert.Equal("Salary", result[0].Description);
        Assert.Equal(5000.00m, result[0].Amount);
        Assert.True(result[0].IsIncome);
    }

    [Fact]
    public async Task Handle_Should_Map_All_Properties_Correctly()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Transportation",
            Code = "TRN"
        };
        dbContext.PaymentCategories.Add(category);

        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            ForecastDefinitionId = forecastId,
            Description = "Gas Money",
            Amount = 75.50m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = false,
            PaymentCategoryId = categoryId,
            PaymentCategory = category,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(occurrenceId, result[0].Id);
        Assert.Equal(forecastId, result[0].ForecastDefinitionId);
        Assert.Equal("Gas Money", result[0].Description);
        Assert.Equal(75.50m, result[0].Amount);
        Assert.Equal(new DateOnly(2024, 6, 15), result[0].Date);
        Assert.False(result[0].IsIncome);
        Assert.Equal(categoryId, result[0].PaymentCategoryId);
        Assert.Equal("Transportation", result[0].Category);
    }

    [Fact]
    public async Task Handle_Should_Respect_Cancellation_Token()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31));

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_StartDate_Equals_EndDate()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastId = Guid.NewGuid();

        var occurrence = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId,
            Description = "Same Day",
            Amount = 100.00m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 15),
            new DateOnly(2024, 6, 15));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Same Day", result[0].Description);
    }

    [Fact]
    public async Task Handle_Should_Return_Multiple_Occurrences_From_Different_Forecasts()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var forecastId1 = Guid.NewGuid();
        var forecastId2 = Guid.NewGuid();

        var occurrence1 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId1,
            Description = "Forecast 1",
            Amount = 100.00m,
            ExpectedDate = new DateOnly(2024, 6, 15),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence1);

        var occurrence2 = new ForecastOccurrence
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = forecastId2,
            Description = "Forecast 2",
            Amount = 200.00m,
            ExpectedDate = new DateOnly(2024, 6, 16),
            IsIncome = false,
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
        };
        dbContext.ForecastOccurrences.Add(occurrence2);

        await dbContext.SaveChangesAsync();

        var handler = new GetForecastRowsQueryHandler(dbContext);
        var query = new GetForecastRowsQuery(
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(forecastId1, result[0].ForecastDefinitionId);
        Assert.Equal(forecastId2, result[1].ForecastDefinitionId);
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
