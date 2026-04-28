using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.DeleteIncome;

public class DeleteIncomeCommandHandlerTests
{
    private readonly Mock<MoneyTrackerDbContext> _mockDbContext;
    private readonly Mock<DbSet<Income>> _mockIncomeDbSet;
    private readonly Mock<DbSet<ForecastOccurrence>> _mockForecastOccurrenceDbSet;

    public DeleteIncomeCommandHandlerTests()
    {
        _mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>().Options,
            null!);
        _mockIncomeDbSet = new Mock<DbSet<Income>>();
        _mockForecastOccurrenceDbSet = new Mock<DbSet<ForecastOccurrence>>();
    }

    [Fact]
    public void Constructor_Should_InitializeHandler_When_ValidDbContextProvided()
    {
        // Act
        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_InvalidOccurrenceAction()
    {
        // Arrange
        var command = new DeleteIncomeCommand(Guid.NewGuid(), "InvalidAction");
        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Equal("Occurrence action must be Auto, Reopen, or Skip.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_ThrowEntityNotFoundException_When_IncomeNotFound()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var command = new DeleteIncomeCommand(incomeId, "Auto");

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Income?)null);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Contains(incomeId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_MarkIncomeAsDeleted_When_IncomeHasNoForecastOccurrence()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = null,
            IsDeleted = false
        };
        var command = new DeleteIncomeCommand(incomeId, "Auto");

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == incomeId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(income.IsDeleted);
        _mockDbContext.Verify(db => db.Incomes.Update(income), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_MarkIncomeAsDeleted_When_ForecastOccurrenceNotFound()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var command = new DeleteIncomeCommand(incomeId, "Auto");

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((ForecastOccurrence?)null);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(income.IsDeleted);
        _mockDbContext.Verify(db => db.Incomes.Update(income), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsReopen()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = true,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeleteIncomeCommand(incomeId, "Reopen");

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(income.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToSkipped_When_OccurrenceActionIsSkip()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = true,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeleteIncomeCommand(incomeId, "Skip");

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(income.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsAutoAndFutureDate()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = true,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeleteIncomeCommand(incomeId, "Auto");

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(income.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToSkipped_When_OccurrenceActionIsAutoAndPastDate()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = true,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeleteIncomeCommand(incomeId, "Auto");

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(income.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.SkippedId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_UpdateOccurrenceStatusToPending_When_OccurrenceActionIsAutoAndTodayDate()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var occurrenceId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = occurrenceId,
            IsDeleted = false
        };
        var occurrence = new ForecastOccurrence
        {
            Id = occurrenceId,
            Description = "Test Occurrence",
            Amount = 100m,
            ExpectedDate = DateOnly.FromDateTime(DateTime.Today),
            ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId,
            ValidatedAt = DateTime.Now,
            IsIncome = true,
            ForecastDefinitionId = Guid.NewGuid()
        };
        var command = new DeleteIncomeCommand(incomeId, "Auto");

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.ForecastOccurrences)
            .Returns(_mockForecastOccurrenceDbSet.Object);

        _mockForecastOccurrenceDbSet.Setup(db => db.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ForecastOccurrence, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(occurrence);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(income.IsDeleted);
        Assert.Equal(ForecastOccurrenceStatus.PendingId, occurrence.ForecastOccurrenceStatusId);
        Assert.Null(occurrence.ValidatedAt);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_When_Called()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = null,
            IsDeleted = false
        };
        var command = new DeleteIncomeCommand(incomeId, "Auto");
        var cancellationToken = new CancellationToken();

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            cancellationToken))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        _mockDbContext.Verify(db => db.Incomes.FindAsync(It.IsAny<object[]>(), cancellationToken), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_HandleNullOccurrenceAction_When_OccurrenceActionIsNull()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = null,
            IsDeleted = false
        };
        var command = new DeleteIncomeCommand(incomeId, null);

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(income.IsDeleted);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_HandleEmptyOccurrenceAction_When_OccurrenceActionIsEmpty()
    {
        // Arrange
        var incomeId = Guid.NewGuid();
        var income = new Income
        {
            Id = incomeId,
            Description = "Test Income",
            Amount = 100m,
            Date = DateTime.Now,
            ForecastOccurrenceId = null,
            IsDeleted = false
        };
        var command = new DeleteIncomeCommand(incomeId, string.Empty);

        _mockDbContext.Setup(db => db.Incomes.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        _mockDbContext.Setup(db => db.Incomes.Update(It.IsAny<Income>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteIncomeCommandHandler(_mockDbContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(income.IsDeleted);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
