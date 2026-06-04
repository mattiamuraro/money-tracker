using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.GetPaymentById;

public class GetPaymentByIdQueryHandlerTests
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
        var handler = new GetPaymentByIdQueryHandler(db);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_Payment_When_Exists()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Groceries",
            Code = "GRC"
        };
        dbContext.PaymentCategories.Add(category);

        var payment = new Payment
        {
            Id = paymentId,
            Description = "Weekly Shopping",
            PaymentCategoryId = categoryId,
            PaymentCategory = category,
            Amount = 150.75m,
            Date = new DateTime(2024, 1, 15),
            IsOneShot = true,
            ForecastOccurrenceId = null
        };
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentByIdQueryHandler(dbContext);
        var query = new GetPaymentByIdQuery(paymentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paymentId, result.Id);
        Assert.Equal("Weekly Shopping", result.Description);
        Assert.Equal(categoryId, result.PaymentCategoryId);
        Assert.Equal("Groceries", result.Category);
        Assert.Equal(150.75m, result.Amount);
        Assert.Equal(new DateTime(2024, 1, 15), result.Date);
        Assert.True(result.IsOneShot);
        Assert.Null(result.ForecastOccurrenceId);
        Assert.Null(result.ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Return_Payment_With_ForecastOccurrence_When_Linked()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var forecastOccurrenceId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Utilities",
            Code = "UTL"
        };
        dbContext.PaymentCategories.Add(category);

        var forecastOccurrence = new ForecastOccurrence
        {
            Id = forecastOccurrenceId,
            ForecastDefinitionId = Guid.NewGuid(),
            Description = "Monthly Rent",
            Amount = 1200m,
            IsIncome = false,
            ExpectedDate = new DateOnly(2024, 1, 1)
        };
        dbContext.ForecastOccurrences.Add(forecastOccurrence);

        var payment = new Payment
        {
            Id = paymentId,
            Description = "January Rent",
            PaymentCategoryId = categoryId,
            PaymentCategory = category,
            Amount = 1200m,
            Date = new DateTime(2024, 1, 1),
            IsOneShot = false,
            ForecastOccurrenceId = forecastOccurrenceId,
            ForecastOccurrence = forecastOccurrence
        };
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentByIdQueryHandler(dbContext);
        var query = new GetPaymentByIdQuery(paymentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paymentId, result.Id);
        Assert.Equal("January Rent", result.Description);
        Assert.Equal(categoryId, result.PaymentCategoryId);
        Assert.Equal("Utilities", result.Category);
        Assert.Equal(1200m, result.Amount);
        Assert.Equal(new DateTime(2024, 1, 1), result.Date);
        Assert.False(result.IsOneShot);
        Assert.Equal(forecastOccurrenceId, result.ForecastOccurrenceId);
        Assert.Equal(new DateOnly(2024, 1, 1), result.ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Throw_EntityNotFoundException_When_Payment_Not_Found()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetPaymentByIdQueryHandler(dbContext);
        var query = new GetPaymentByIdQuery(paymentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(query, CancellationToken.None));
        Assert.Equal($"Payment with id {paymentId} not found", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_Respect_CancellationToken()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetPaymentByIdQueryHandler(dbContext);
        var query = new GetPaymentByIdQuery(paymentId);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_Should_Return_Payment_With_Null_ForecastExpectedDate_When_ForecastOccurrence_Is_Null()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Entertainment",
            Code = "ENT"
        };
        dbContext.PaymentCategories.Add(category);

        var payment = new Payment
        {
            Id = paymentId,
            Description = "Movie Tickets",
            PaymentCategoryId = categoryId,
            PaymentCategory = category,
            Amount = 25.00m,
            Date = new DateTime(2024, 2, 20),
            IsOneShot = true,
            ForecastOccurrenceId = Guid.NewGuid()
        };
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentByIdQueryHandler(dbContext);
        var query = new GetPaymentByIdQuery(paymentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paymentId, result.Id);
        Assert.NotNull(result.ForecastOccurrenceId);
        Assert.Null(result.ForecastExpectedDate);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Soft_Deleted_Payment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD"
        };
        dbContext.PaymentCategories.Add(category);

        var payment = new Payment
        {
            Id = paymentId,
            Description = "Deleted Payment",
            PaymentCategoryId = categoryId,
            PaymentCategory = category,
            Amount = 100.00m,
            Date = new DateTime(2024, 3, 10),
            IsOneShot = true,
            DeletedAt = DateTime.UtcNow,
            IsDeleted = true
        };
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var handler = new GetPaymentByIdQueryHandler(dbContext);
        var query = new GetPaymentByIdQuery(paymentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(query, CancellationToken.None));
        Assert.Equal($"Payment with id {paymentId} not found", exception.Message);
    }

    private static MoneyTrackerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoneyTrackerDbContext(options, null!);
    }
}
