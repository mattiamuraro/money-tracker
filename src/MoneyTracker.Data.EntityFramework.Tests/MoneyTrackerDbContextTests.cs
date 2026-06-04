using MoneyTracker.Data.EntityFramework.Tests.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Xunit;

namespace MoneyTracker.Data.EntityFramework.Tests;

public class MoneyTrackerDbContextTests : IDisposable
{
    private readonly InMemoryDbContextFixture _fixture;
    private readonly MoneyTrackerDbContext _dbContext;

    public MoneyTrackerDbContextTests()
    {
        _fixture = new InMemoryDbContextFixture();
        _dbContext = _fixture.CreateDbContext();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _fixture.Dispose();
    }

    [Fact]
    public void DbContext_CanCreateDatabase()
    {
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", _dbContext.Database.ProviderName);
    }

    [Fact]
    public async Task DbContext_CanAddPaymentCategory()
    {
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };

        _dbContext.PaymentCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var savedCategory = _dbContext.PaymentCategories.FirstOrDefault(c => c.Id == category.Id);
        Assert.NotNull(savedCategory);
        Assert.Equal("Food", savedCategory!.Name);
    }

    [Fact]
    public async Task DbContext_CanAddPayment()
    {
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD"
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = categoryId,
            IsOneShot = true
        };

        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        var savedPayment = _dbContext.Payments
            .Where(p => p.Id == payment.Id)
            .Select(p => new { p.Description, p.Amount, Category = p.PaymentCategory.Name })
            .FirstOrDefault();

        Assert.NotNull(savedPayment);
        Assert.Equal("Groceries", savedPayment!.Description);
        Assert.Equal(50.00m, savedPayment.Amount);
    }

    [Fact]
    public async Task DbContext_CanUpdatePayment()
    {
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD"
        };

        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Groceries",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = categoryId,
            IsOneShot = true
        };

        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        var paymentToUpdate = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.NotNull(paymentToUpdate);

        paymentToUpdate!.Description = "Updated Groceries";
        paymentToUpdate.Amount = 75.50m;
        await _dbContext.SaveChangesAsync();

        var updatedPayment = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.NotNull(updatedPayment);
        Assert.Equal("Updated Groceries", updatedPayment!.Description);
        Assert.Equal(75.50m, updatedPayment.Amount);
    }

    [Fact]
    public async Task DbContext_CanDeletePayment()
    {
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD"
        };

        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Description = "Groceries",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = categoryId,
            IsOneShot = true
        };

        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        var paymentToDelete = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.NotNull(paymentToDelete);

        _dbContext.Payments.Remove(paymentToDelete!);
        await _dbContext.SaveChangesAsync();

        var deletedPayment = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        Assert.Null(deletedPayment);
    }

    [Fact]
    public async Task DbContext_CanQueryPaymentsByCategory()
    {
        var category1 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD"
        };

        var category2 = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Transport",
            Code = "TRANS"
        };

        _dbContext.PaymentCategories.Add(category1);
        _dbContext.PaymentCategories.Add(category2);

        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Groceries",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category1.Id
        };

        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Bus Ticket",
            Amount = 10.00m,
            Date = DateTime.UtcNow,
            PaymentCategoryId = category2.Id
        };

        _dbContext.Payments.Add(payment1);
        _dbContext.Payments.Add(payment2);
        await _dbContext.SaveChangesAsync();

        var foodPayments = _dbContext.Payments
            .Where(p => p.PaymentCategoryId == category1.Id)
            .ToList();

        Assert.Single(foodPayments);
        Assert.Equal("Groceries", foodPayments.First().Description);
    }

    [Fact]
    public async Task DbContext_CanQueryPaymentsByDateRange()
    {
        var categoryId = Guid.NewGuid();
        var category = new PaymentCategory
        {
            Id = categoryId,
            Name = "Food",
            Code = "FOOD"
        };

        var today = DateTime.UtcNow.Date;

        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Old Payment",
            Amount = 50.00m,
            Date = today.AddDays(-5),
            PaymentCategoryId = categoryId
        };

        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            Description = "Recent Payment",
            Amount = 30.00m,
            Date = today.AddDays(1),
            PaymentCategoryId = categoryId
        };

        _dbContext.PaymentCategories.Add(category);
        _dbContext.Payments.Add(payment1);
        _dbContext.Payments.Add(payment2);
        await _dbContext.SaveChangesAsync();

        var paymentsInRange = _dbContext.Payments
            .Where(p => p.Date >= today && p.Date < today.AddDays(2))
            .ToList();

        Assert.Single(paymentsInRange);
        Assert.Equal("Recent Payment", paymentsInRange.First().Description);
    }

    [Fact]
    public void Constructor_WithNullHttpContextAccessor_CreatesInstance()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MoneyTrackerDbContext(options, null);

        Assert.NotNull(context);
    }

    [Fact]
    public void Constructor_WithHttpContextAccessor_CreatesInstance()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var httpContextAccessor = new FakeHttpContextAccessor();

        var context = new MoneyTrackerDbContext(options, httpContextAccessor);

        Assert.NotNull(context);
    }

    [Fact]
    public void OnModelCreating_ConfiguresEntities_Success()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new MoneyTrackerDbContext(options);
        var model = context.Model;

        Assert.NotNull(model.FindEntityType(typeof(User)));
        Assert.NotNull(model.FindEntityType(typeof(Payment)));
        Assert.NotNull(model.FindEntityType(typeof(Income)));
        Assert.NotNull(model.FindEntityType(typeof(PaymentCategory)));
        Assert.NotNull(model.FindEntityType(typeof(ForecastExpense)));
        Assert.NotNull(model.FindEntityType(typeof(ForecastIncome)));
        Assert.NotNull(model.FindEntityType(typeof(ForecastRecurrenceRuleType)));
        Assert.NotNull(model.FindEntityType(typeof(ForecastOccurrenceStatus)));
        Assert.NotNull(model.FindEntityType(typeof(ForecastOccurrence)));
    }

    [Fact]
    public void SaveChanges_AppliesAuditFields_ForAddedEntity()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new MoneyTrackerDbContext(options);
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Code = "TEST"
        };

        context.PaymentCategories.Add(category);
        context.SaveChanges();

        Assert.NotEqual(DateTime.MinValue, category.CreatedAt);
        Assert.NotEqual(Guid.Empty, category.CreatedById);
        Assert.NotEqual(DateTime.MinValue, category.ModifiedAt);
        Assert.NotEqual(Guid.Empty, category.ModifiedById);
        Assert.Equal(SystemUsers.SystemUserId, category.CreatedById);
        Assert.Equal(SystemUsers.SystemUserId, category.ModifiedById);
    }

    [Fact]
    public void SaveChanges_AppliesAuditFields_ForModifiedEntity()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new MoneyTrackerDbContext(options);
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Code = "TEST"
        };

        context.PaymentCategories.Add(category);
        context.SaveChanges();

        var originalCreatedAt = category.CreatedAt;
        var originalCreatedById = category.CreatedById;

        category.Name = "Updated";
        context.SaveChanges();

        Assert.Equal(originalCreatedAt, category.CreatedAt);
        Assert.Equal(originalCreatedById, category.CreatedById);
        Assert.True(category.ModifiedAt > originalCreatedAt);
        Assert.Equal(SystemUsers.SystemUserId, category.ModifiedById);
    }

    [Fact]
    public void SaveChanges_WithAuthenticatedUser_UsesUserIdFromClaims()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var userId = Guid.NewGuid();
        var httpContextAccessor = new FakeHttpContextAccessor(userId);

        using var context = new MoneyTrackerDbContext(options, httpContextAccessor);
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Code = "TEST"
        };

        context.PaymentCategories.Add(category);
        context.SaveChanges();

        Assert.Equal(userId, category.CreatedById);
        Assert.Equal(userId, category.ModifiedById);
    }

    [Fact]
    public async Task SaveChangesAsync_AppliesAuditFields_ForAddedEntity()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new MoneyTrackerDbContext(options);
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Code = "TEST"
        };

        context.PaymentCategories.Add(category);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.NotEqual(DateTime.MinValue, category.CreatedAt);
        Assert.NotEqual(Guid.Empty, category.CreatedById);
        Assert.NotEqual(DateTime.MinValue, category.ModifiedAt);
        Assert.NotEqual(Guid.Empty, category.ModifiedById);
        Assert.Equal(SystemUsers.SystemUserId, category.CreatedById);
        Assert.Equal(SystemUsers.SystemUserId, category.ModifiedById);
    }

    [Fact]
    public async Task SaveChangesAsync_AppliesAuditFields_ForModifiedEntity()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new MoneyTrackerDbContext(options);
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Code = "TEST"
        };

        context.PaymentCategories.Add(category);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var originalCreatedAt = category.CreatedAt;
        var originalCreatedById = category.CreatedById;

        category.Name = "Updated";
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(originalCreatedAt, category.CreatedAt);
        Assert.Equal(originalCreatedById, category.CreatedById);
        Assert.True(category.ModifiedAt > originalCreatedAt);
        Assert.Equal(SystemUsers.SystemUserId, category.ModifiedById);
    }

    [Fact]
    public async Task SaveChangesAsync_WithAuthenticatedUser_UsesUserIdFromClaims()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var userId = Guid.NewGuid();
        var httpContextAccessor = new FakeHttpContextAccessor(userId);

        using var context = new MoneyTrackerDbContext(options, httpContextAccessor);
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Code = "TEST"
        };

        context.PaymentCategories.Add(category);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(userId, category.CreatedById);
        Assert.Equal(userId, category.ModifiedById);
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_Success()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new MoneyTrackerDbContext(options);
        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Code = "TEST"
        };
        
        using var cts = new CancellationTokenSource();

        context.PaymentCategories.Add(category);
        await context.SaveChangesAsync(cts.Token);

        Assert.NotEqual(Guid.Empty, category.CreatedById);
    }

    [Fact]
    public void GetCurrentUser_WithNullHttpContextAccessor_ReturnsSystemUserId()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestableMoneyTrackerDbContext(options, null);

        var userId = context.GetCurrentUserPublic();

        Assert.Equal(SystemUsers.SystemUserId, userId);
    }

    [Fact]
    public void GetCurrentUser_WithNullHttpContext_ReturnsSystemUserId()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestableMoneyTrackerDbContext(options, new FakeHttpContextAccessor(httpContext: null));

        var userId = context.GetCurrentUserPublic();

        Assert.Equal(SystemUsers.SystemUserId, userId);
    }

    [Fact]
    public void GetCurrentUser_WithNullUser_ReturnsSystemUserId()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var httpContext = new DefaultHttpContext { User = null! };
        using var context = new TestableMoneyTrackerDbContext(options, new FakeHttpContextAccessor(httpContext));

        var userId = context.GetCurrentUserPublic();

        Assert.Equal(SystemUsers.SystemUserId, userId);
    }

    [Fact]
    public void GetCurrentUser_WithNoNameIdentifierClaim_ReturnsSystemUserId()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var claims = new[] { new Claim(ClaimTypes.Email, "test@example.com") };
        var identity = new ClaimsIdentity(claims);
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        using var context = new TestableMoneyTrackerDbContext(options, new FakeHttpContextAccessor(httpContext));

        var userId = context.GetCurrentUserPublic();

        Assert.Equal(SystemUsers.SystemUserId, userId);
    }

    [Fact]
    public void GetCurrentUser_WithInvalidGuidClaim_ReturnsSystemUserId()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") };
        var identity = new ClaimsIdentity(claims);
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        using var context = new TestableMoneyTrackerDbContext(options, new FakeHttpContextAccessor(httpContext));

        var userId = context.GetCurrentUserPublic();

        Assert.Equal(SystemUsers.SystemUserId, userId);
    }

    [Fact]
    public void GetCurrentUser_WithValidGuidClaim_ReturnsUserId()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var expectedUserId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        using var context = new TestableMoneyTrackerDbContext(options, new FakeHttpContextAccessor(httpContext));

        var userId = context.GetCurrentUserPublic();

        Assert.Equal(expectedUserId, userId);
    }

    private class TestableMoneyTrackerDbContext : MoneyTrackerDbContext
    {
        public TestableMoneyTrackerDbContext(DbContextOptions<MoneyTrackerDbContext> options, IHttpContextAccessor? httpContextAccessor)
            : base(options, httpContextAccessor)
        {
        }

        public Guid GetCurrentUserPublic() => GetCurrentUser();
    }

    private sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }

        public FakeHttpContextAccessor(HttpContext? httpContext = null) => HttpContext = httpContext;

        public FakeHttpContextAccessor(Guid userId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        }
    }
}
