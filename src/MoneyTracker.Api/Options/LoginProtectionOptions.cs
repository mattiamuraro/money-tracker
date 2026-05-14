namespace MoneyTracker.Api.Options;

/// <summary>
/// Configures login abuse protection thresholds.
/// </summary>
public sealed class LoginProtectionOptions
{
    public const string SectionName = "Security:LoginProtection";

    /// <summary>
    /// Maximum number of consecutive failed login attempts before lockout.
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Lockout duration in minutes after reaching the failed-attempt threshold.
    /// </summary>
    public int LockoutMinutes { get; set; } = 15;
}
