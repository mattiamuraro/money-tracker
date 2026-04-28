using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.PaymentCategories.DeleteCategory;

public class DeleteCategoryCommandHandlerTests
{
    private readonly MoneyTrackerDbContext _dbContext;

    public DeleteCategoryCommandHandlerTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new MoneyTrackerDbContext(options, null!);
    }

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        // Act
        var handler = new DeleteCategoryCommandHandler(_dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldDeleteCategory_WhenCategoryExistsAndHasNoPayments()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteCategoryCommand(categoryId);
        var handler = new DeleteCategoryCommandHandler(_dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedCategory = await _dbContext.PaymentCategories.FindAsync(categoryId);
        Assert.Null(deletedCategory);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(nonExistentId);
        var handler = new DeleteCategoryCommandHandler(_dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal($"Payment category with id {nonExistentId} not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCategoryHasAssociatedPayments()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TST"
        };
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Test Payment",
            PaymentCategoryId = categoryId,
            Amount = 100.00m,
            Date = DateTime.UtcNow,
            PaymentCategory = category
        };
        category.Payments.Add(payment);

        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteCategoryCommand(categoryId);
        var handler = new DeleteCategoryCommandHandler(_dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Cannot delete a category that has associated payments.", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TST"
        };
        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteCategoryCommand(categoryId);
        var handler = new DeleteCategoryCommandHandler(_dbContext);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cts.Token));
    }
}
