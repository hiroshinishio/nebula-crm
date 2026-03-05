using Nebula.Api.Helpers;
using Nebula.Application.Common;
using Nebula.Application.Interfaces;
using Nebula.Application.Services;

namespace Nebula.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        group.MapGet("/kpis", GetKpis);
        group.MapGet("/opportunities", GetOpportunities);
        group.MapGet("/opportunities/flow", GetOpportunityFlow);
        group.MapGet("/opportunities/{entityType}/{status}/items", GetOpportunityItems);
        group.MapGet("/nudges", GetNudges);

        return app;
    }

    private static async Task<IResult> GetKpis(
        DashboardService svc, IAuthorizationService authz, ICurrentUserService user, CancellationToken ct)
    {
        if (!await HasAccessAsync(user, authz, "dashboard_kpi"))
            return ProblemDetailsHelper.Forbidden();
        return Results.Ok(await svc.GetKpisAsync(ct));
    }

    private static async Task<IResult> GetOpportunities(
        DashboardService svc, IAuthorizationService authz, ICurrentUserService user, CancellationToken ct)
    {
        if (!await HasAccessAsync(user, authz, "dashboard_pipeline"))
            return ProblemDetailsHelper.Forbidden();
        return Results.Ok(await svc.GetOpportunitiesAsync(ct));
    }

    private static async Task<IResult> GetOpportunityFlow(
        string entityType, int? periodDays,
        DashboardService svc, IAuthorizationService authz, ICurrentUserService user, CancellationToken ct)
    {
        if (!await HasAccessAsync(user, authz, "dashboard_pipeline"))
            return ProblemDetailsHelper.Forbidden();

        if (entityType is not ("submission" or "renewal"))
        {
            return Results.BadRequest(new
            {
                code = "invalid_entity_type",
                message = "entityType must be 'submission' or 'renewal'.",
            });
        }

        return Results.Ok(await svc.GetOpportunityFlowAsync(entityType, periodDays ?? 180, ct));
    }

    private static async Task<IResult> GetOpportunityItems(
        string entityType, string status,
        DashboardService svc, IAuthorizationService authz, ICurrentUserService user, CancellationToken ct)
    {
        if (!await HasAccessAsync(user, authz, "dashboard_pipeline"))
            return ProblemDetailsHelper.Forbidden();
        return Results.Ok(await svc.GetOpportunityItemsAsync(entityType, status, ct));
    }

    private static async Task<IResult> GetNudges(
        DashboardService svc, IAuthorizationService authz, ICurrentUserService user, CancellationToken ct)
    {
        if (!await HasAccessAsync(user, authz, "dashboard_nudge"))
            return ProblemDetailsHelper.Forbidden();
        return Results.Ok(await svc.GetNudgesAsync(user.UserId, user, ct));
    }

    private static async Task<bool> HasAccessAsync(
        ICurrentUserService user, IAuthorizationService authz, string resource)
    {
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, resource, "read"))
                return true;
        }
        return false;
    }
}
