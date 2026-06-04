using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.UpdateIncome;

public class UpdateIncomeCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static UpdateIncomeCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new UpdateIncomeCommandValidator(), db);

    [Fact]
    public void Constructor_Should_Initialize_Handler()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_When_ValidationFails()
    {
        // Arrange
        using var db = CreateDbContext();
        var command = new UpdateIncomeCommand(Guid.NewGuid(), null, -1m, null); // Amount must be > 0
        var handler = CreateHandler(db);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_ThrowEntityNotFoundException_When_IncomeNotFound()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var command = new UpdateIncomeCommand(incomeId, "Test", 100m, DateTime.Now);
        var handler = CreateHandler(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None));
        Assert.Contains(incomeId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_Should_UpdateAllProperties_When_AllPropertiesProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Old Description", Amount = 50m, Date = DateTime.Now.AddDays(-10) });
        await db.SaveChangesAsync();

        var newDescription = "  New Description  ";
        var newAmount = 100m;
        var newDate = DateTime.Now;
        var command = new UpdateIncomeCommand(incomeId, newDescription, newAmount, newDate);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("New Description", updated!.Description);
        Assert.Equal(newAmount, updated.Amount);
        Assert.Equal(newDate, updated.Date);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyDescription_When_OnlyDescriptionProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var originalAmount = 50m;
        var originalDate = DateTime.Now.AddDays(-10);
        db.Incomes.Add(new Income { Id = incomeId, Description = "Old Description", Amount = originalAmount, Date = originalDate });
        await db.SaveChangesAsync();

        var command = new UpdateIncomeCommand(incomeId, "New Description", null, null);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("New Description", updated!.Description);
        Assert.Equal(originalAmount, updated.Amount);
        Assert.Equal(originalDate, updated.Date);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyAmount_When_OnlyAmountProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var originalDate = DateTime.Now.AddDays(-10);
        db.Incomes.Add(new Income { Id = incomeId, Description = "Description", Amount = 50m, Date = originalDate });
        await db.SaveChangesAsync();

        var newAmount = 200m;
        var command = new UpdateIncomeCommand(incomeId, null, newAmount, null);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("Description", updated!.Description);
        Assert.Equal(newAmount, updated.Amount);
        Assert.Equal(originalDate, updated.Date);
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyDate_When_OnlyDateProvided()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Description", Amount = 50m, Date = DateTime.Now.AddDays(-10) });
        await db.SaveChangesAsync();

        var newDate = DateTime.Now.AddDays(5);
        var command = new UpdateIncomeCommand(incomeId, null, null, newDate);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("Description", updated!.Description);
        Assert.Equal(50m, updated.Amount);
        Assert.Equal(newDate, updated.Date);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsNull()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Original Description", Amount = 50m, Date = DateTime.Now.AddDays(-10) });
        await db.SaveChangesAsync();

        var command = new UpdateIncomeCommand(incomeId, null, 100m, DateTime.Now);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("Original Description", updated!.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsEmpty()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Original Description", Amount = 50m, Date = DateTime.Now.AddDays(-10) });
        await db.SaveChangesAsync();

        var command = new UpdateIncomeCommand(incomeId, "", 100m, DateTime.Now);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("Original Description", updated!.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDescription_When_DescriptionIsWhitespace()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Original Description", Amount = 50m, Date = DateTime.Now.AddDays(-10) });
        await db.SaveChangesAsync();

        var command = new UpdateIncomeCommand(incomeId, "   ", 100m, DateTime.Now);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("Original Description", updated!.Description);
    }

    [Fact]
    public async Task Handle_Should_TrimDescription_When_DescriptionHasLeadingOrTrailingSpaces()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Original Description", Amount = 50m, Date = DateTime.Now.AddDays(-10) });
        await db.SaveChangesAsync();

        var command = new UpdateIncomeCommand(incomeId, "  New Description  ", null, null);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("New Description", updated!.Description);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_When_Called()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        db.Incomes.Add(new Income { Id = incomeId, Description = "Description", Amount = 50m, Date = DateTime.Now });
        await db.SaveChangesAsync();

        var command = new UpdateIncomeCommand(incomeId, "Updated", 100m, DateTime.Now);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("Updated", updated!.Description);
    }

    [Fact]
    public async Task Handle_Should_NotUpdateAnyProperty_When_AllPropertiesAreNull()
    {
        // Arrange
        using var db = CreateDbContext();
        var incomeId = Guid.NewGuid();
        var originalDate = DateTime.Now.AddDays(-10);
        db.Incomes.Add(new Income { Id = incomeId, Description = "Original Description", Amount = 50m, Date = originalDate });
        await db.SaveChangesAsync();

        var command = new UpdateIncomeCommand(incomeId, null, null, null);
        var handler = CreateHandler(db);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await db.Incomes.FindAsync(incomeId);
        Assert.Equal("Original Description", updated!.Description);
        Assert.Equal(50m, updated.Amount);
        Assert.Equal(originalDate, updated.Date);
    }
}
