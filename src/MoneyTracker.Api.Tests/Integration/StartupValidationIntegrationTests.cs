using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Verifies that the API host fails fast on invalid startup configuration
/// rather than starting in a broken state.
/// </summary>
public sealed class StartupValidationIntegrationTests
{
    [Fact]
    public void Host_WhenJwtKeyIsMissing_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            config["Jwt:Key"] = null;
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenJwtKeyIsTooShort_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            // 10 bytes — below the 32-byte minimum.
            config["Jwt:Key"] = "short-key";
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenJwtIssuerIsMissing_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            config["Jwt:Issuer"] = null;
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenJwtAudienceIsMissing_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            config["Jwt:Audience"] = null;
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenJwtExpiryIsZero_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            config["Jwt:ExpiryMinutes"] = "0";
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenLoginProtectionMaxAttemptsIsZero_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            config["Security:LoginProtection:MaxFailedAttempts"] = "0";
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenLoginProtectionLockoutMinutesIsZero_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            config["Security:LoginProtection:LockoutMinutes"] = "0";
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenPasswordPolicyMinLengthBelowEight_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            config["Auth:PasswordPolicy:MinimumLength"] = "7";
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenPasswordPolicyMaxLengthBelowMin_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            config["Auth:PasswordPolicy:MinimumLength"] = "12";
            config["Auth:PasswordPolicy:MaximumLength"] = "10";
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenRefreshTokenExpiryIsZero_ThrowsOnBuild()
    {
        using var factory = new InvalidConfigFactory(config =>
        {
            config["Auth:RefreshToken:ExpiryDays"] = "0";
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Host_WhenValidConfigProvided_StartsSuccessfully()
    {
        // Verifies the baseline factory starts cleanly (smoke test).
        using var factory = new ApiWebFactory();
        var client = factory.CreateClient();
        Assert.NotNull(client);
    }

    /// <summary>
    /// A factory that starts from the valid baseline config but allows individual
    /// overrides to introduce specific invalid values.
    /// </summary>
    private sealed class InvalidConfigFactory : ApiWebFactory
    {
        private readonly Action<Dictionary<string, string?>> _overrideConfig;

        internal InvalidConfigFactory(Action<Dictionary<string, string?>> overrideConfig)
        {
            _overrideConfig = overrideConfig;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Apply valid baseline first.
            base.ConfigureWebHost(builder);

            // Then apply the caller's invalid overrides.
            var overrides = new Dictionary<string, string?>();
            _overrideConfig(overrides);

            foreach (var (key, value) in overrides)
                builder.UseSetting(key, value);
        }
    }
}
