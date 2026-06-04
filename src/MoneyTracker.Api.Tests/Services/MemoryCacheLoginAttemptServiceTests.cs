using Microsoft.Extensions.Caching.Memory;
using MoneyTracker.Api.Options;
using MoneyTracker.Api.Services;
using Xunit;

namespace MoneyTracker.Api.Tests.Services;

public class MemoryCacheLoginAttemptServiceTests
{
    [Fact]
    public void RegisterFailure_WhenThresholdReached_LocksOutUser()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 2,
                LockoutMinutes = 1
            }));

        var now = DateTimeOffset.UtcNow;

        // Act
        service.RegisterFailure("user", now);
        service.RegisterFailure("user", now);

        // Assert
        Assert.True(service.IsLockedOut("user", now));
    }

    [Fact]
    public void RegisterSuccess_ClearsLockoutState()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 1,
                LockoutMinutes = 5
            }));

        var now = DateTimeOffset.UtcNow;
        service.RegisterFailure("user", now);

        // Act
        service.RegisterSuccess("user");

        // Assert
        Assert.False(service.IsLockedOut("user", now));
    }

    [Fact]
    public void IsLockedOut_ReturnsFalse_WhenNoFailuresRegistered()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 3,
                LockoutMinutes = 5
            }));

        Assert.False(service.IsLockedOut("user", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsLockedOut_ReturnsFalse_WhenBelowFailureThreshold()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 3,
                LockoutMinutes = 5
            }));

        var now = DateTimeOffset.UtcNow;
        service.RegisterFailure("user", now);
        service.RegisterFailure("user", now);

        Assert.False(service.IsLockedOut("user", now));
    }

    [Fact]
    public void IsLockedOut_ReturnsFalse_WhenLockoutHasExpired()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 1,
                LockoutMinutes = 5
            }));

        var past = DateTimeOffset.UtcNow.AddMinutes(-10);
        service.RegisterFailure("user", past);

        // Checking at a time after the lockout window has passed
        Assert.False(service.IsLockedOut("user", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsLockedOut_IsCaseInsensitive()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 1,
                LockoutMinutes = 5
            }));

        var now = DateTimeOffset.UtcNow;
        service.RegisterFailure("USER", now);

        Assert.True(service.IsLockedOut("user", now));
        Assert.True(service.IsLockedOut("User", now));
        Assert.True(service.IsLockedOut("USER", now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsLockedOut_ThrowsArgumentException_WhenUsernameIsNullOrWhiteSpace(string? username)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 3,
                LockoutMinutes = 5
            }));

        Assert.ThrowsAny<ArgumentException>(() => service.IsLockedOut(username!, DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RegisterFailure_ThrowsArgumentException_WhenUsernameIsNullOrWhiteSpace(string? username)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 3,
                LockoutMinutes = 5
            }));

        Assert.ThrowsAny<ArgumentException>(() => service.RegisterFailure(username!, DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RegisterSuccess_ThrowsArgumentException_WhenUsernameIsNullOrWhiteSpace(string? username)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 3,
                LockoutMinutes = 5
            }));

        Assert.ThrowsAny<ArgumentException>(() => service.RegisterSuccess(username!));
    }

    [Fact]
    public void RegisterFailure_AtExactThreshold_LocksOutUser()
    {
        // Boundary test: exactly MaxFailedAttempts failures (not one more)
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 3,
                LockoutMinutes = 5
            }));

        var now = DateTimeOffset.UtcNow;
        service.RegisterFailure("user", now);
        service.RegisterFailure("user", now);
        // Still not locked after 2 failures
        Assert.False(service.IsLockedOut("user", now));

        // Exactly at threshold
        service.RegisterFailure("user", now);
        Assert.True(service.IsLockedOut("user", now));
    }

    [Fact]
    public void RegisterSuccess_AllowsNewFailuresToStartFresh()
    {
        // After a successful login the counter resets;
        // a single subsequent failure must NOT lock the user out
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheLoginAttemptService(
            cache,
            Microsoft.Extensions.Options.Options.Create(new LoginProtectionOptions
            {
                MaxFailedAttempts = 2,
                LockoutMinutes = 5
            }));

        var now = DateTimeOffset.UtcNow;
        service.RegisterFailure("user", now);
        service.RegisterSuccess("user");

        service.RegisterFailure("user", now);

        Assert.False(service.IsLockedOut("user", now));
    }
}
