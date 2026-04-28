using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.UpdateIncome;

public class UpdateIncomeCommandHandlerTests
{
    private readonly Mock<IValidator<UpdateIncomeCommand>> _mockValidator;
    private readonly Mock<MoneyTrackerDbContext> _mockDbContext;
    private readonly Mock<DbSet<Income>> _mockIncomeDbSet;

    public UpdateIncomeCommandHandlerTests()
    {
        _mockValidator = new Mock<IValidator<UpdateIncomeCommand>>();
        _mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>().Options,
            null!);
        _mockIncomeDbSet = new Mock<DbSet<Income>>();
    }

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        // Act
        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_When_ValidationFails()
    {
        // Arrange
        var command = new UpdateIncomeCommand(Guid.NewGuid(), "Test", 100m, DateTime.Now);
        var validationFailure = new ValidationFailure("IncomeId", "Invalid income id");
        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_ThrowEntityNotFoundException_When_IncomeNotFound()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var command = new UpdateIncomeCommand(incomeId, "Test", 100m, DateTime.Now);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Income?)null);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Contains(incomeId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_UpdateAllProperties_When_AllPropertiesProvided()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Old Description",
            Amount = 50m,
            Date = DateTime.Now.AddDays(-10)
        };

        var newDescription = "  New Description  ";
        var newAmount = 100m;
        var newDate = DateTime.Now;
        var command = new UpdateIncomeCommand(incomeId, newDescription, newAmount, newDate);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == incomeId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Description", existingIncome.Description);
        Assert.Equal(newAmount, existingIncome.Amount);
        Assert.Equal(newDate, existingIncome.Date);
        _mockDbContext.Verify(db => db.Incomes.Update(existingIncome), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyDescription_When_OnlyDescriptionProvided()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Old Description",
            Amount = 50m,
            Date = DateTime.Now.AddDays(-10)
        };

        var originalAmount = existingIncome.Amount;
        var originalDate = existingIncome.Date;
        var command = new UpdateIncomeCommand(incomeId, "New Description", null, null);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Description", existingIncome.Description);
        Assert.Equal(originalAmount, existingIncome.Amount);
        Assert.Equal(originalDate, existingIncome.Date);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyAmount_When_OnlyAmountProvided()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.Now.AddDays(-10)
        };

        var originalDescription = existingIncome.Description;
        var originalDate = existingIncome.Date;
        var newAmount = 200m;
        var command = new UpdateIncomeCommand(incomeId, null, newAmount, null);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingIncome.Description);
        Assert.Equal(newAmount, existingIncome.Amount);
        Assert.Equal(originalDate, existingIncome.Date);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyDate_When_OnlyDateProvided()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.Now.AddDays(-10)
        };

        var originalDescription = existingIncome.Description;
        var originalAmount = existingIncome.Amount;
        var newDate = DateTime.Now.AddDays(5);
        var command = new UpdateIncomeCommand(incomeId, null, null, newDate);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingIncome.Description);
        Assert.Equal(originalAmount, existingIncome.Amount);
        Assert.Equal(newDate, existingIncome.Date);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsNull()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.Now.AddDays(-10)
        };

        var originalDescription = existingIncome.Description;
        var command = new UpdateIncomeCommand(incomeId, null, 100m, DateTime.Now);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingIncome.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsEmpty()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.Now.AddDays(-10)
        };

        var originalDescription = existingIncome.Description;
        var command = new UpdateIncomeCommand(incomeId, "", 100m, DateTime.Now);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingIncome.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsWhitespace()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.Now.AddDays(-10)
        };

        var originalDescription = existingIncome.Description;
        var command = new UpdateIncomeCommand(incomeId, "   ", 100m, DateTime.Now);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingIncome.Description);
    }

    [Fact]
    public async Task Handle_Should_TrimDescription_When_DescriptionHasLeadingOrTrailingSpaces()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.Now.AddDays(-10)
        };

        var command = new UpdateIncomeCommand(incomeId, "  New Description  ", null, null);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Description", existingIncome.Description);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_When_Called()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Description",
            Amount = 50m,
            Date = DateTime.Now
        };

        var command = new UpdateIncomeCommand(incomeId, "Updated", 100m, DateTime.Now);
        var cancellationToken = new CancellationToken();

        _mockValidator.Setup(v => v.ValidateAsync(command, cancellationToken))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            cancellationToken))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        _mockValidator.Verify(v => v.ValidateAsync(command, cancellationToken), Times.Once);
        _mockDbContext.Verify(db => db.Incomes.FindAsync(It.IsAny<object[]>(), cancellationToken), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateAnyProperty_When_AllPropertiesAreNull()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = incomeId,
            Description = "Original Description",
            Amount = 50m,
            Date = DateTime.Now.AddDays(-10)
        };

        var originalDescription = existingIncome.Description;
        var originalAmount = existingIncome.Amount;
        var originalDate = existingIncome.Date;
        var command = new UpdateIncomeCommand(incomeId, null, null, null);

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateIncomeCommandHandler(_mockValidator.Object, _mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalDescription, existingIncome.Description);
        Assert.Equal(originalAmount, existingIncome.Amount);
        Assert.Equal(originalDate, existingIncome.Date);
        _mockDbContext.Verify(db => db.Incomes.Update(existingIncome), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
