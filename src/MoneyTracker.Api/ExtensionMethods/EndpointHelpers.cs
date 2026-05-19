using System.Globalization;
using System.Security.Claims;
using FluentValidation;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.Data;

namespace MoneyTracker.Api.ExtensionMethods;

internal static class EndpointHelpers
{
    internal static Guid GetCurrentUserId(this HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : SystemUsers.SystemUserId;
    }

    internal static Dictionary<string, string[]> ToValidationErrors(this ValidationException exception)
    {
        return exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }

    internal static (int Year, int Month) GetRequiredYearMonth(this string? month, string validationMessage)
    {
        if (string.IsNullOrWhiteSpace(month))
            throw new BadRequestException(validationMessage);

        if (!DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedMonth))
            throw new BadRequestException(validationMessage);

        return (parsedMonth.Year, parsedMonth.Month);
    }

    internal static string? GetIdempotencyKey(this HttpContext httpContext, string? requestIdempotencyKey = null)
    {
        var headerIdempotencyKey = httpContext.Request.Headers["X-Idempotency-Key"].ToString();
        return string.IsNullOrWhiteSpace(headerIdempotencyKey)
            ? (string.IsNullOrWhiteSpace(requestIdempotencyKey) ? null : requestIdempotencyKey)
            : headerIdempotencyKey;
    }
}
