namespace MoneyTracker.BusinessLogic.Features.Auth;

/// <summary>
/// Authorization claim types, values, and policies used by MoneyTracker APIs.
/// </summary>
public static class AuthAuthorization
{
    /// <summary>
    /// Claim type containing API permission values.
    /// </summary>
    public const string PermissionClaimType = "permission";

    public static class Permissions
    {
        public const string Read = "read";
        public const string Write = "write";
    }

    public static class Policies
    {
        public const string ReadAccess = "ReadAccess";
        public const string WriteAccess = "WriteAccess";
    }
}
