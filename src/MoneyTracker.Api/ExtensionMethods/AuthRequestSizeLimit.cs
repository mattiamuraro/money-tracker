using MoneyTracker.Api.Resources;

namespace MoneyTracker.Api.ExtensionMethods;

/// <summary>
/// Enforces a maximum request body size on auth endpoints to prevent oversized payload attacks.
/// Extracted to allow direct unit testing of the size-check logic.
/// </summary>
public static class AuthRequestSizeLimit
{
    /// <summary>Maximum allowed body size for auth endpoints (4 KB).</summary>
    public const long LimitBytes = 4 * 1024;

    /// <summary>
    /// Returns a 413 problem result when <paramref name="contentLength"/> exceeds <see cref="LimitBytes"/>,
    /// or <c>null</c> when the request is within the allowed size.
    /// </summary>
    public static IResult? CheckLimit(long? contentLength)
    {
        if (contentLength is > LimitBytes)
            return Results.Problem(
                ErrorMessageResources.RequestPayloadTooLarge,
                statusCode: StatusCodes.Status413PayloadTooLarge);

        return null;
    }
}
