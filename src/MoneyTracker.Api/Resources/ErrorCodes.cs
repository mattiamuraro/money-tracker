namespace MoneyTracker.Api.Resources;

internal static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string BadRequest = "BAD_REQUEST";
    public const string InvalidOperation = "INVALID_OPERATION";
    public const string NotFound = "NOT_FOUND";
    public const string TransientFailure = "TRANSIENT_FAILURE";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
}
