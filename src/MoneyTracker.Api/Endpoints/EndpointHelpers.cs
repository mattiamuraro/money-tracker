using FluentValidation;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.Data;
using System.Security.Claims;

namespace MoneyTracker.Api.Endpoints;

internal static class EndpointHelpers
{
    internal static Guid GetCurrentUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : SystemUsers.SystemUserId;
    }

    internal static bool TryParseOccurrenceAction(string? occurrenceAction, out ForecastOccurrenceDeleteAction action)
    {
        if (string.IsNullOrWhiteSpace(occurrenceAction))
        {
            action = ForecastOccurrenceDeleteAction.Auto;
            return true;
        }

        return Enum.TryParse(occurrenceAction, true, out action);
    }

    internal static Dictionary<string, string[]> ToValidationErrors(ValidationException exception)
    {
        return exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }
}
