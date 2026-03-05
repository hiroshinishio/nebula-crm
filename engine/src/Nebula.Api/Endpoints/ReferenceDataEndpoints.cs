using Nebula.Application.Services;

namespace Nebula.Api.Endpoints;

public static class ReferenceDataEndpoints
{
    public static IEndpointRouteBuilder MapReferenceDataEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/accounts", async (ReferenceDataService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetAccountsAsync(ct)))
            .WithTags("Accounts").RequireAuthorization();

        app.MapGet("/mgas", async (ReferenceDataService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetMgasAsync(ct)))
            .WithTags("MGAs").RequireAuthorization();

        app.MapGet("/programs", async (ReferenceDataService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetProgramsAsync(ct)))
            .WithTags("Programs").RequireAuthorization();

        return app;
    }
}
