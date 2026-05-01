using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.PaymentCategories.UpdateCategory;

public class UpdateCategoryCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static UpdateCategoryCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new UpdateCategoryCommandValidator(), db);

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategory_WhenCommandIsValid()
    {
        // Arrange
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Old Name", Code = "OLD" });
        await db.SaveChangesAsync();

        var command = new UpdateCategoryCommand { Id = categoryId, Name = "New Name", Code = "NEW" };
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var category = await db.PaymentCategories.FindAsync(categoryId);
        Assert.NotNull(category);
        Assert.Equal("New Name", category.Name);
        Assert.Equal("NEW", category.Code);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new UpdateCategoryCommand { Id = Guid.NewGuid(), Name = "", Code = "" };
        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        using var db = CreateDbContext();
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateCategoryCommand { Id = nonExistentId, Name = "Test Name", Code = "TST" };
        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal($"Payment category with id {nonExistentId} not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCodeConflictsWithAnotherCategory()
    {
        // Arrange
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Category 1", Code = "CAT1" });
        db.PaymentCategories.Add(new PaymentCategory { Id = Guid.NewGuid(), Name = "Category 2", Code = "CAT2" });
        await db.SaveChangesAsync();

        var command = new UpdateCategoryCommand { Id = categoryId, Name = "Updated Name", Code = "CAT2" };
        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Category with code 'CAT2' already exists.", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldAllowSameCode_WhenUpdatingTheSameCategory()
    {
        // Arrange
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Original Name", Code = "SAME" });
        await db.SaveChangesAsync();

        var command = new UpdateCategoryCommand { Id = categoryId, Name = "Updated Name", Code = "SAME" };
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var category = await db.PaymentCategories.FindAsync(categoryId);
        Assert.NotNull(category);
        Assert.Equal("Updated Name", category.Name);
        Assert.Equal("SAME", category.Code);
    }

    [Fact]
    public async Task Handle_ShouldSaveChangesToDatabase_WhenCategoryIsUpdated()
    {
        // Arrange
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Old Name", Code = "OLD" });
        await db.SaveChangesAsync();

        var command = new UpdateCategoryCommand { Id = categoryId, Name = "New Name", Code = "NEW" };
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var categoriesCount = await db.PaymentCategories.CountAsync();
        Assert.Equal(1, categoriesCount);

        var updatedCategory = await db.PaymentCategories.FindAsync(categoryId);
        Assert.NotNull(updatedCategory);
        Assert.Equal("New Name", updatedCategory.Name);
        Assert.Equal("NEW", updatedCategory.Code);
    }

    [Fact]
    public async Task Handle_ShouldCallValidateAndThrowAsync_BeforeProcessing()
    {
        // Arrange
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategory { Id = categoryId, Name = "Test Category", Code = "TST" });
        await db.SaveChangesAsync();

        var command = new UpdateCategoryCommand { Id = categoryId, Name = "Updated Name", Code = "UPD" };
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var category = await db.PaymentCategories.FindAsync(categoryId);
        Assert.Equal("Updated Name", category!.Name);
        Assert.Equal("UPD", category.Code);
    }
}
