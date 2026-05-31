namespace MoneyTracker.Api.ExtensionMethods;

internal static class AuthEndpointConventions
{
    internal static RouteHandlerBuilder RequireAuthRequestSizeLimit(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddEndpointFilter(async (context, next) =>
        {
            var limitResult = AuthRequestSizeLimit.CheckLimit(context.HttpContext.Request.ContentLength);
            if (limitResult is not null)
                return limitResult;

            return await next(context);
        });
    }

    internal static RouteHandlerBuilder AddNoStoreResponseHeaders(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddEndpointFilter(async (context, next) =>
        {
            var result = await next(context);
            NoStoreResponseHeaders.Apply(context.HttpContext.Response);
            return result;
        });
    }
}
