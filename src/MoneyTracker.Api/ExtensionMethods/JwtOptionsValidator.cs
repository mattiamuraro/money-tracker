using System.Text;
using MoneyTracker.BusinessLogic.Features.Auth.Options;

namespace MoneyTracker.Api.ExtensionMethods;

/// <summary>
/// Validates <see cref="JwtOptions"/> values at startup.
/// Extracted to allow direct unit testing without a full host build.
/// </summary>
public static class JwtOptionsValidator
{
    /// <summary>Minimum key size in bytes required for HS256.</summary>
    public const int MinimumKeyBytes = 32;

    /// <summary>Returns an error message when <paramref name="key"/> is null or whitespace, otherwise <c>null</c>.</summary>
    public static string? ValidateKeyPresence(string? key) =>
        string.IsNullOrWhiteSpace(key) ? "Jwt:Key is required." : null;

    /// <summary>Returns an error message when <paramref name="key"/> encodes to fewer than <see cref="MinimumKeyBytes"/> bytes, otherwise <c>null</c>.</summary>
    public static string? ValidateKeyLength(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null; // presence check owns that error

        return Encoding.UTF8.GetByteCount(key) < MinimumKeyBytes
            ? $"Jwt:Key must be at least {MinimumKeyBytes} bytes."
            : null;
    }

    /// <summary>Returns an error message when <paramref name="value"/> is null or whitespace, otherwise <c>null</c>.</summary>
    public static string? ValidateRequiredField(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value) ? $"{fieldName} is required." : null;

    /// <summary>Returns an error message when <paramref name="minutes"/> is not positive, otherwise <c>null</c>.</summary>
    public static string? ValidateExpiryMinutes(int minutes) =>
        minutes <= 0 ? "Jwt:ExpiryMinutes must be greater than 0." : null;
}
