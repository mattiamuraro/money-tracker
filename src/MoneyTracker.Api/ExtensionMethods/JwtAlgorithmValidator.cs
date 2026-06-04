using Microsoft.IdentityModel.Tokens;

namespace MoneyTracker.Api.ExtensionMethods;

/// <summary>
/// Validates that a JWT token uses the expected signing algorithm.
/// Extracted to allow direct unit testing without a full authentication pipeline.
/// </summary>
public static class JwtAlgorithmValidator
{
    /// <summary>
    /// Returns an error message if the algorithm resolved from the token header is not <see cref="SecurityAlgorithms.HmacSha256"/>,
    /// or <c>null</c> when the algorithm is valid.
    /// </summary>
    public static string? Validate(string? algorithm)
    {
        if (algorithm is null)
            return "Invalid token type.";

        if (!string.Equals(algorithm, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            return "Invalid token algorithm.";

        return null;
    }
}
