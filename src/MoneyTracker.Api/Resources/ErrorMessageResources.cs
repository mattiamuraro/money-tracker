using System.Globalization;
using System.Resources;

namespace MoneyTracker.Api.Resources;

internal static class ErrorMessageResources
{
    private static readonly ResourceManager ResourceManager = new(
        "MoneyTracker.Api.Resources.ErrorMessageResources",
        typeof(ErrorMessageResources).Assembly);

    public static string ValidationError => Get(nameof(ValidationError), "One or more validation errors occurred.");
    public static string Unauthorized => Get(nameof(Unauthorized), "Authentication failed.");
    public static string Forbidden => Get(nameof(Forbidden), "Access is forbidden.");
    public static string Conflict => Get(nameof(Conflict), "The request conflicts with the current state.");
    public static string BadRequest => Get(nameof(BadRequest), "The request is invalid.");
    public static string InvalidOperation => Get(nameof(InvalidOperation), "The operation is invalid.");
    public static string NotFound => Get(nameof(NotFound), "The requested resource was not found.");
    public static string TransientFailure => Get(nameof(TransientFailure), "A temporary infrastructure error occurred. Please retry.");
    public static string InternalServerError => Get(nameof(InternalServerError), "An internal server error occurred.");

    private static string Get(string key, string fallback)
        => ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? fallback;
}
