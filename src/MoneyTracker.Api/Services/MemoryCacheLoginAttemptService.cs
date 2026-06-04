using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MoneyTracker.Api.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Login;

namespace MoneyTracker.Api.Services;

public sealed class MemoryCacheLoginAttemptService : ILoginAttemptService
{
    private sealed class LoginAttemptState
    {
        public int ConsecutiveFailures { get; set; }
        public DateTimeOffset? LockedUntilUtc { get; set; }
    }

    private readonly IMemoryCache _cache;
    private readonly LoginProtectionOptions _options;

    public MemoryCacheLoginAttemptService(IMemoryCache cache, IOptions<LoginProtectionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(options);

        _cache = cache;
        _options = options.Value;
    }

    public bool IsLockedOut(string username, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        if (!_cache.TryGetValue(GetCacheKey(username), out LoginAttemptState? state)
            || state?.LockedUntilUtc is null)
        {
            return false;
        }

        if (state.LockedUntilUtc > nowUtc)
        {
            return true;
        }

        _cache.Remove(GetCacheKey(username));
        return false;
    }

    public void RegisterFailure(string username, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        var cacheKey = GetCacheKey(username);
        var state = _cache.GetOrCreate(cacheKey, static _ => new LoginAttemptState())!;
        state.ConsecutiveFailures++;

        if (state.ConsecutiveFailures >= _options.MaxFailedAttempts)
        {
            state.LockedUntilUtc = nowUtc.AddMinutes(_options.LockoutMinutes);
        }

        _cache.Set(cacheKey, state, TimeSpan.FromMinutes(_options.LockoutMinutes * 2));
    }

    public void RegisterSuccess(string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        _cache.Remove(GetCacheKey(username));
    }

    private static string GetCacheKey(string username)
        => $"login-attempt:{username.Trim().ToUpperInvariant()}";
}
