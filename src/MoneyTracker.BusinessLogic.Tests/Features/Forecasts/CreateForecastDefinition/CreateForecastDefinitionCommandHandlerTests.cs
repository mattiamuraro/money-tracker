using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.CreateForecastDefinition;

public class CreateForecastDefinitionCommandHandlerTests
{
    [Fact]
    public void Constructor_ShouldInitialize_AllDependencies()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        // Act
        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        var validationFailure = new ValidationFailure("Description", "Description is required");
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateForecastDefinitionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            mockDbContext.Object);

        var command = new CreateForecastDefinitionCommand
        {
            Description = "",
            Amount = 100.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_IncomeCommand_CreatesIncome()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateForecastDefinitionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            dbContext);

        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Income",
            Amount = 1000.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        var createdIncome = await dbContext.ForecastIncomes.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(createdIncome);
        Assert.Equal("Test Income", createdIncome.Description);
        Assert.Equal(1000.00m, createdIncome.Amount);
        Assert.True(createdIncome.IsActive);
    }

    [Fact]
    public async Task Handle_ExpenseCommand_CreatesExpense()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            CreatedById = userId,
            ModifiedById = userId
        };
        dbContext.PaymentCategories.Add(category);

        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateForecastDefinitionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            dbContext);

        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Expense",
            Amount = 500.00m,
            IsIncome = false,
            PaymentCategoryId = categoryId,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        var createdExpense = await dbContext.ForecastExpenses.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(createdExpense);
        Assert.Equal("Test Expense", createdExpense.Description);
        Assert.Equal(500.00m, createdExpense.Amount);
        Assert.Equal(categoryId, createdExpense.PaymentCategoryId);
        Assert.True(createdExpense.IsActive);
    }

    [Fact]
    public async Task Handle_ExpenseCommandWithInvalidCategory_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateForecastDefinitionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            dbContext);

        var nonExistentCategoryId = Guid.NewGuid();
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Expense",
            Amount = 500.00m,
            IsIncome = false,
            PaymentCategoryId = nonExistentCategoryId,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("The requested payment category does not exist.", exception.Message);
    }

    [Fact]
    public async Task Handle_IncomeCommand_CreatesForecastOccurrences()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateForecastDefinitionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            dbContext);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Income",
            Amount = 1000.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = today,
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdOccurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == result)
            .ToListAsync();

        Assert.NotEmpty(createdOccurrences);
        Assert.All(createdOccurrences, occ =>
        {
            Assert.Equal("Test Income", occ.Description);
            Assert.Equal(1000.00m, occ.Amount);
            Assert.True(occ.IsIncome);
            Assert.Equal(ForecastOccurrenceStatus.PendingId, occ.ForecastOccurrenceStatusId);
        });
    }

    [Fact]
    public async Task Handle_ExpenseCommand_CreatesForecastOccurrences()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            CreatedById = userId,
            ModifiedById = userId
        };
        dbContext.PaymentCategories.Add(category);

        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateForecastDefinitionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            dbContext);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Expense",
            Amount = 500.00m,
            IsIncome = false,
            PaymentCategoryId = categoryId,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = today,
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdOccurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == result)
            .ToListAsync();

        Assert.NotEmpty(createdOccurrences);
        Assert.All(createdOccurrences, occ =>
        {
            Assert.Equal("Test Expense", occ.Description);
            Assert.Equal(500.00m, occ.Amount);
            Assert.False(occ.IsIncome);
            Assert.Equal(categoryId, occ.PaymentCategoryId);
            Assert.Equal(ForecastOccurrenceStatus.PendingId, occ.ForecastOccurrenceStatusId);
        });
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateForecastDefinitionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            mockDbContext.Object);

        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_IncomeCommandWithRecurrenceEnd_CreatesIncome()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateForecastDefinitionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            dbContext);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Income",
            Amount = 1000.00m,
            IsIncome = true,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = today,
            RecurrenceEnd = today.AddDays(30),
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        var createdIncome = await dbContext.ForecastIncomes.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(createdIncome);
        Assert.Equal(today.AddDays(30), createdIncome.RecurrenceEnd);
    }

    [Fact]
    public async Task Handle_ExpenseCommandWithRecurrenceEnd_CreatesExpense()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var userId = Guid.NewGuid();
        var recurrenceRuleTypeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hash"
        };
        dbContext.Users.Add(user);

        var recurrenceRuleType = new ForecastRecurrenceRuleType
        {
            Id = recurrenceRuleTypeId,
            Name = "Daily",
            Code = "Day"
        };
        dbContext.ForecastRecurrenceRuleTypes.Add(recurrenceRuleType);

        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Test Category",
            CreatedById = userId,
            ModifiedById = userId
        };
        dbContext.PaymentCategories.Add(category);

        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<CreateForecastDefinitionCommand>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateForecastDefinitionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateForecastDefinitionCommandHandler(
            mockValidator.Object,
            dbContext);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test Expense",
            Amount = 500.00m,
            IsIncome = false,
            PaymentCategoryId = categoryId,
            ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId,
            RecurrenceStart = today,
            RecurrenceEnd = today.AddDays(30),
            Interval = 1
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        var createdExpense = await dbContext.ForecastExpenses.FirstOrDefaultAsync(x => x.Id == result);
        Assert.NotNull(createdExpense);
        Assert.Equal(today.AddDays(30), createdExpense.RecurrenceEnd);
    }
}
