using Nebula.Application.Common;
using Nebula.Application.Services;

namespace Nebula.Api.Endpoints;

public static class TimelineEndpoints
{
    public static IEndpointRouteBuilder MapTimelineEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/timeline/events", async (
            string entityType, Guid? entityId, int? limit,
            TimelineService svc, ICurrentUserService user, CancellationToken ct) =>
            Results.Ok(await svc.ListEventsAsync(entityType, entityId, limit ?? 20, user, ct)))
            .WithTags("Timeline").RequireAuthorization();

        return app;
    }
}
