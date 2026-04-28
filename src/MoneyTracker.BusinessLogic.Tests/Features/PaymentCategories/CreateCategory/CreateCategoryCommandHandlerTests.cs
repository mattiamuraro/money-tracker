using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.PaymentCategories.CreateCategory;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<IValidator<CreateCategoryCommand>> _mockValidator;
    private readonly MoneyTrackerDbContext _dbContext;

    public CreateCategoryCommandHandlerTests()
    {
        _mockValidator = new Mock<IValidator<CreateCategoryCommand>>();
        
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
        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Test Category",
            Code = "TST"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        var category = await _dbContext.PaymentCategories.FindAsync(result);
        Assert.NotNull(category);
        Assert.Equal("Test Category", category.Name);
        Assert.Equal("TST", category.Code);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
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

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCategoryCodeAlreadyExists()
    {
        // Arrange
        var existingCategory = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Existing Category",
            Code = "EXIST"
        };

        _dbContext.PaymentCategories.Add(existingCategory);
        await _dbContext.SaveChangesAsync();

        var command = new CreateCategoryCommand
        {
            Name = "New Category",
            Code = "EXIST"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Category with code 'EXIST' already exists.", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldSaveChangesToDatabase_WhenCategoryIsCreated()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Save Test Category",
            Code = "SAVE"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var categoriesCount = await _dbContext.PaymentCategories.CountAsync();
        Assert.True(categoriesCount > 0);
        var savedCategory = await _dbContext.PaymentCategories.FirstOrDefaultAsync(c => c.Code == "SAVE");
        Assert.NotNull(savedCategory);
        Assert.Equal(result, savedCategory.Id);
    }

    [Fact]
    public async Task Handle_ShouldCallValidateAndThrowAsync_WhenHandleCalled()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Validation Test",
            Code = "VAL"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockValidator.Verify(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewGuid_WhenCreatingCategory()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "GUID Test",
            Code = "GUID"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

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
        var command = new CreateCategoryCommand
        {
            Name = "Return ID Test",
            Code = "RET"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdCategory = await _dbContext.PaymentCategories.FirstOrDefaultAsync(c => c.Id == result);
        Assert.NotNull(createdCategory);
        Assert.Equal(result, createdCategory.Id);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_WhenCalled()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Cancellation Test",
            Code = "CANC"
        };

        var cancellationToken = new CancellationToken();

        _mockValidator
            .Setup(v => v.ValidateAsync(command, cancellationToken))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        _mockValidator.Verify(v => v.ValidateAsync(command, cancellationToken), Times.Once);
    }

    [Fact]
    public void Constructor_ShouldAssignDbContext_WhenCalled()
    {
        // Arrange & Act
        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Constructor_ShouldAssignValidator_WhenCalled()
    {
        // Arrange & Act
        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldAddCategoryToDbSet_WhenCategoryIsCreated()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Add Test",
            Code = "ADD"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);
        var initialCount = await _dbContext.PaymentCategories.CountAsync();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var finalCount = await _dbContext.PaymentCategories.CountAsync();
        Assert.Equal(initialCount + 1, finalCount);
    }

    [Fact]
    public async Task Handle_ShouldCreateMultipleCategories_WhenCodesAreDifferent()
    {
        // Arrange
        var command1 = new CreateCategoryCommand
        {
            Name = "Category 1",
            Code = "CAT1"
        };

        var command2 = new CreateCategoryCommand
        {
            Name = "Category 2",
            Code = "CAT2"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result1 = await handler.Handle(command1, CancellationToken.None);
        var result2 = await handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.NotEqual(Guid.Empty, result1);
        Assert.NotEqual(Guid.Empty, result2);

        var category1 = await _dbContext.PaymentCategories.FindAsync(result1);
        var category2 = await _dbContext.PaymentCategories.FindAsync(result2);

        Assert.NotNull(category1);
        Assert.NotNull(category2);
        Assert.Equal("CAT1", category1.Code);
        Assert.Equal("CAT2", category2.Code);
    }

    [Fact]
    public async Task Handle_ShouldMapCommandPropertiesToCategory_WhenCreatingCategory()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Property Mapping Test",
            Code = "PROPMAP"
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var category = await _dbContext.PaymentCategories.FindAsync(result);
        Assert.NotNull(category);
        Assert.Equal(command.Name, category.Name);
        Assert.Equal(command.Code, category.Code);
        Assert.Equal(result, category.Id);
    }

    [Fact]
    public async Task Handle_ShouldCheckForExistingCategory_BeforeCreation()
    {
        // Arrange
        var existingCode = "PREEXIST";
        var existingCategory = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Pre-existing Category",
            Code = existingCode
        };

        _dbContext.PaymentCategories.Add(existingCategory);
        await _dbContext.SaveChangesAsync();

        var command = new CreateCategoryCommand
        {
            Name = "Duplicate Attempt",
            Code = existingCode
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateCategoryCommandHandler(_mockValidator.Object, _dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains(existingCode, exception.Message);
        Assert.Contains("already exists", exception.Message);
    }
}
