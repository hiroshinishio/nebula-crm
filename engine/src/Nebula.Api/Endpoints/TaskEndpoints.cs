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
        // BrokerUser: scope-isolated tasks by linked broker entity (F0009 §12).
        if (user.Roles.Contains("BrokerUser"))
        {
            // Casbin check: BrokerUser, task, read (policy.csv §2.10 — condition is 'true' for broker scope).
            var brokerUserAuthorized = false;
            foreach (var role in user.Roles)
            {
                // Use placeholder attrs — BrokerUser scope is enforced at query layer, not policy condition.
                if (await authz.AuthorizeAsync(role, "task", "read", new Dictionary<string, object>()))
                { brokerUserAuthorized = true; break; }
            }
            if (!brokerUserAuthorized) return ProblemDetailsHelper.Forbidden();

            return Results.Ok(await svc.GetBrokerScopedTasksAsync(limit ?? 10, user, ct));
        }

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
