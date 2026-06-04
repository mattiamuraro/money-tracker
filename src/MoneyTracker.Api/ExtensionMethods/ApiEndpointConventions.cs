using MoneyTracker.BusinessLogic.Features.Auth;

namespace MoneyTracker.Api.ExtensionMethods;

internal static class ApiEndpointConventions
{
    internal static RouteGroupBuilder MapApiGroup(this WebApplication app, string routeSegment, string tag)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(routeSegment);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        return app.MapGroup(ApiRoutes.CreateGroupPath(routeSegment))
            .WithTags(tag);
    }

    internal static RouteGroupBuilder RequireReadAccess(this RouteGroupBuilder group)
    {
        ArgumentNullException.ThrowIfNull(group);
        return group.RequireAuthorization(AuthAuthorization.Policies.ReadAccess);
    }

    internal static RouteHandlerBuilder RequireWriteAccess(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.RequireAuthorization(AuthAuthorization.Policies.WriteAccess);
    }

    internal static IResult CreatedResource(string routeSegment, Guid id)
        => Results.Created(ApiRoutes.CreateResourcePath(routeSegment, id), id);
}
