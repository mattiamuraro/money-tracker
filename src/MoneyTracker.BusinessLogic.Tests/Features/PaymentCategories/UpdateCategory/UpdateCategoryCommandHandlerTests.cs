using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.PaymentCategories.UpdateCategory;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<IValidator<UpdateCategoryCommand>> _mockValidator;
    private readonly MoneyTrackerDbContext _dbContext;

    public UpdateCategoryCommandHandlerTests()
    {
        _mockValidator = new Mock<IValidator<UpdateCategoryCommand>>();
        
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
        var handler = new UpdateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategory_WhenCommandIsValid()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new PaymentCategory
        {
            Id = categoryId,
            Name = "Old Name",
            Code = "OLD"
        };

        _dbContext.PaymentCategories.Add(existingCategory);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "New Name",
            Code = "NEW"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new UpdateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var category = await _dbContext.PaymentCategories.FindAsync(categoryId);
        Assert.NotNull(category);
        Assert.Equal("New Name", category.Name);
        Assert.Equal("NEW", category.Code);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = Guid.NewGuid(),
            Name = "",
            Code = ""
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var handler = new UpdateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateCategoryCommand
        {
            Id = nonExistentId,
            Name = "Test Name",
            Code = "TST"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new UpdateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal($"Payment category with id {nonExistentId} not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCodeConflictsWithAnotherCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new PaymentCategory
        {
            Id = categoryId,
            Name = "Category 1",
            Code = "CAT1"
        };

        var anotherCategoryId = Guid.NewGuid();
        var anotherCategory = new PaymentCategory
        {
            Id = anotherCategoryId,
            Name = "Category 2",
            Code = "CAT2"
        };

        _dbContext.PaymentCategories.Add(existingCategory);
        _dbContext.PaymentCategories.Add(anotherCategory);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "Updated Name",
            Code = "CAT2" // Trying to use code from another category
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new UpdateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Category with code 'CAT2' already exists.", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldAllowSameCode_WhenUpdatingTheSameCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new PaymentCategory
        {
            Id = categoryId,
            Name = "Original Name",
            Code = "SAME"
        };

        _dbContext.PaymentCategories.Add(existingCategory);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "Updated Name",
            Code = "SAME" // Same code, different name
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new UpdateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var category = await _dbContext.PaymentCategories.FindAsync(categoryId);
        Assert.NotNull(category);
        Assert.Equal("Updated Name", category.Name);
        Assert.Equal("SAME", category.Code);
    }

    [Fact]
    public async Task Handle_ShouldSaveChangesToDatabase_WhenCategoryIsUpdated()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new PaymentCategory
        {
            Id = categoryId,
            Name = "Old Name",
            Code = "OLD"
        };

        _dbContext.PaymentCategories.Add(existingCategory);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "New Name",
            Code = "NEW"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new UpdateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var categoriesCount = await _dbContext.PaymentCategories.CountAsync();
        Assert.Equal(1, categoriesCount);
        
        var updatedCategory = await _dbContext.PaymentCategories.FindAsync(categoryId);
        Assert.NotNull(updatedCategory);
        Assert.Equal("New Name", updatedCategory.Name);
        Assert.Equal("NEW", updatedCategory.Code);
    }

    [Fact]
    public async Task Handle_ShouldCallValidateAndThrowAsync_BeforeProcessing()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            Code = "TST"
        };

        _dbContext.PaymentCategories.Add(existingCategory);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "Updated Name",
            Code = "UPD"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new UpdateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockValidator.Verify(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
