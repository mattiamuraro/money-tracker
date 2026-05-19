using Microsoft.AspNetCore.Http.HttpResults;
using MoneyTracker.Api.Resources;

namespace MoneyTracker.Api.ExtensionMethods;

internal static class AuthEndpointConventions
{
    private const long AuthRequestBodySizeLimitBytes = 4 * 1024;

    internal static RouteHandlerBuilder RequireAuthRequestSizeLimit(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddEndpointFilter(async (context, next) =>
        {
            var request = context.HttpContext.Request;
            if (request.ContentLength is > AuthRequestBodySizeLimitBytes)
            {
                return Results.Problem(
                    ErrorMessageResources.RequestPayloadTooLarge,
                    statusCode: StatusCodes.Status413PayloadTooLarge);
            }

            return await next(context);
        });
    }

    internal static RouteHandlerBuilder AddNoStoreResponseHeaders(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddEndpointFilter(async (context, next) =>
        {
            var result = await next(context);
            var response = context.HttpContext.Response;
            response.Headers.CacheControl = "no-store, no-cache, max-age=0";
            response.Headers.Pragma = "no-cache";
            response.Headers.Expires = "0";
            return result;
        });
    }
}
