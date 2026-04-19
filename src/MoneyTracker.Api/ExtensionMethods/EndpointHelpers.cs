using FluentValidation;
using MoneyTracker.Data;
using System.Security.Claims;

namespace MoneyTracker.Api.ExtensionMethods;

internal static class EndpointHelpers
{
    internal static Guid GetCurrentUserId(this HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : SystemUsers.SystemUserId;
    }

    internal static bool TryParseOccurrenceAction(this string? occurrenceAction, out ForecastOccurrenceDeleteAction action)
    {
        if (string.IsNullOrWhiteSpace(occurrenceAction))
        {
            action = ForecastOccurrenceDeleteAction.Auto;
            return true;
        }

        return Enum.TryParse(occurrenceAction, true, out action);
    }

    internal static Dictionary<string, string[]> ToValidationErrors(this ValidationException exception)
    {
        return exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }
}
