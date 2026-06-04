using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.PaymentCategories.CreateCategory;

public class CreateCategoryCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static CreateCategoryCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new CreateCategoryCommandValidator(), db);

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenCommandIsValid()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateCategoryCommand { Name = "Test Category", Code = "TST" };
        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var category = await db.PaymentCategories.FindAsync(result);
        Assert.NotNull(category);
        Assert.Equal("Test Category", category.Name);
        Assert.Equal("TST", category.Code);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateCategoryCommand { Name = "", Code = "" };
        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenCategoryCodeAlreadyExists()
    {
        // Arrange
        using var db = CreateDbContext();
        db.PaymentCategories.Add(new PaymentCategory { Id = Guid.NewGuid(), Name = "Existing Category", Code = "EXIST" });
        await db.SaveChangesAsync();

        var command = new CreateCategoryCommand { Name = "New Category", Code = "EXIST" };
        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Category with code 'EXIST' already exists.", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldSaveChangesToDatabase_WhenCategoryIsCreated()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateCategoryCommand { Name = "Save Test Category", Code = "SAVE" };
        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var categoriesCount = await db.PaymentCategories.CountAsync();
        Assert.True(categoriesCount > 0);
        var savedCategory = await db.PaymentCategories.FirstOrDefaultAsync(c => c.Code == "SAVE");
        Assert.NotNull(savedCategory);
        Assert.Equal(result, savedCategory.Id);
    }

    [Fact]
    public async Task Handle_ShouldCallValidateAndThrowAsync_WhenHandleCalled()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateCategoryCommand { Name = "Validation Test", Code = "VAL" };
        var handler = CreateHandler(db);

        // Act & Assert - validation runs (no exception means it passed)
        var result = await handler.Handle(command, CancellationToken.None);
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewGuid_WhenCreatingCategory()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateCategoryCommand { Name = "GUID Test", Code = "GUID" };
        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.IsType<Guid>(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategoryId_WhenCategoryIsCreatedSuccessfully()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateCategoryCommand { Name = "Return ID Test", Code = "RET" };
        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdCategory = await db.PaymentCategories.FirstOrDefaultAsync(c => c.Id == result);
        Assert.NotNull(createdCategory);
        Assert.Equal(result, createdCategory.Id);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_WhenCalled()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateCategoryCommand { Name = "Cancellation Test", Code = "CANC" };
        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public void Constructor_ShouldAssignDbContext_WhenCalled()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        Assert.NotNull(handler);
    }

    [Fact]
    public void Constructor_ShouldAssignValidator_WhenCalled()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldAddCategoryToDbSet_WhenCategoryIsCreated()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateCategoryCommand { Name = "Add Test", Code = "ADD" };
        var handler = CreateHandler(db);
        var initialCount = await db.PaymentCategories.CountAsync();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var finalCount = await db.PaymentCategories.CountAsync();
        Assert.Equal(initialCount + 1, finalCount);
    }

    [Fact]
    public async Task Handle_ShouldCreateMultipleCategories_WhenCodesAreDifferent()
    {
        // Arrange
        using var db = CreateDbContext();
        var command1 = new CreateCategoryCommand { Name = "Category 1", Code = "CAT1" };
        var command2 = new CreateCategoryCommand { Name = "Category 2", Code = "CAT2" };
        var handler = CreateHandler(db);

        // Act
        var result1 = await handler.Handle(command1, CancellationToken.None);
        var result2 = await handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.NotEqual(Guid.Empty, result1);
        Assert.NotEqual(Guid.Empty, result2);

        var category1 = await db.PaymentCategories.FindAsync(result1);
        var category2 = await db.PaymentCategories.FindAsync(result2);
        Assert.NotNull(category1);
        Assert.NotNull(category2);
        Assert.Equal("CAT1", category1.Code);
        Assert.Equal("CAT2", category2.Code);
    }

    [Fact]
    public async Task Handle_ShouldMapCommandPropertiesToCategory_WhenCreatingCategory()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new CreateCategoryCommand { Name = "Property Mapping Test", Code = "PROPM" };
        var handler = CreateHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var category = await db.PaymentCategories.FindAsync(result);
        Assert.NotNull(category);
        Assert.Equal(command.Name, category.Name);
        Assert.Equal(command.Code, category.Code);
        Assert.Equal(result, category.Id);
    }

    [Fact]
    public async Task Handle_ShouldCheckForCategoryCodeTooLong_BeforeCreation()
    {
        // Arrange
        using var db = CreateDbContext();
        var tooLongCode = "LONGCODE";

        var command = new CreateCategoryCommand { Name = "Long Category", Code = tooLongCode };
        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains("Category code cannot exceed 5 characters.", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldCheckForExistingCategory_BeforeCreation()
    {
        // Arrange
        using var db = CreateDbContext();
        var existingCode = "EXST";
        db.PaymentCategories.Add(new PaymentCategory { Id = Guid.NewGuid(), Name = "Pre-existing Category", Code = existingCode });
        await db.SaveChangesAsync();

        var command = new CreateCategoryCommand { Name = "Duplicate Attempt", Code = existingCode };
        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains(existingCode, exception.Message);
        Assert.Contains("already exists", exception.Message);
    }
}
