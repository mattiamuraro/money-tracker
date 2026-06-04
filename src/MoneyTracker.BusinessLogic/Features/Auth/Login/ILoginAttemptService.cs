namespace MoneyTracker.BusinessLogic.Features.Auth.Login;

/// <summary>
/// Tracks authentication failures and temporary lockout state for usernames.
/// </summary>
public interface ILoginAttemptService
{
    /// <summary>
    /// Determines whether the supplied username is currently locked out.
    /// </summary>
    bool IsLockedOut(string username, DateTimeOffset nowUtc);

    /// <summary>
    /// Registers a failed login attempt for the supplied username.
    /// </summary>
    void RegisterFailure(string username, DateTimeOffset nowUtc);

    /// <summary>
    /// Clears tracked failures for the supplied username after successful login.
    /// </summary>
    void RegisterSuccess(string username);
}
