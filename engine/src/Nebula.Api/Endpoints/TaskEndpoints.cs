using Nebula.Api.Helpers;
using Nebula.Application.Common;
using Nebula.Application.Interfaces;
using Nebula.Application.Services;

namespace Nebula.Api.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/my/tasks", GetMyTasks)
            .WithTags("Tasks").RequireAuthorization();

        app.MapGet("/tasks/{taskId:guid}", GetTaskById)
            .WithTags("Tasks").RequireAuthorization();

        // F0003 write endpoints NOT registered — return 404 by default

        return app;
    }

    private static async Task<IResult> GetMyTasks(
        int? limit, TaskService svc, ICurrentUserService user,
        IAuthorizationService authz, CancellationToken ct)
    {
        // Ownership condition: the list is already scoped to the caller; assignee == subjectId by definition.
        var attrs = new Dictionary<string, object>
        {
            ["assignee"] = user.UserId,
            ["subjectId"] = user.UserId,
        };
        var authorized = false;
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, "task", "read", attrs))
            { authorized = true; break; }
        }
        if (!authorized) return ProblemDetailsHelper.Forbidden();

        return Results.Ok(await svc.GetMyTasksAsync(user.UserId, user.DisplayName, limit ?? 10, user, ct));
    }

    private static async Task<IResult> GetTaskById(
        Guid taskId, TaskService svc, ICurrentUserService user,
        IAuthorizationService authz, CancellationToken ct)
    {
        var task = await svc.GetByIdAsync(taskId, ct);
        if (task is null) return ProblemDetailsHelper.NotFound("Task", taskId);

        var attrs = new Dictionary<string, object>
        {
            ["assignee"] = task.AssignedToUserId,
            ["subjectId"] = user.UserId,
        };
        var authorized = false;
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, "task", "read", attrs))
            { authorized = true; break; }
        }
        if (!authorized) return ProblemDetailsHelper.Forbidden();

        return Results.Ok(task);
    }
}
