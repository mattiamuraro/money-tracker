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
}
