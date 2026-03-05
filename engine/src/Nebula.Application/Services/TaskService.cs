using Microsoft.Extensions.Logging;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;

namespace Nebula.Application.Services;

public class TaskService(ITaskRepository taskRepo, ILogger<TaskService> logger)
{
    private readonly ILogger<TaskService> _logger = logger;

    public async Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await taskRepo.GetByIdAsync(id, ct);
        return task is null ? null : MapToDto(task);
    }

    public async Task<MyTasksResponseDto> GetMyTasksAsync(
        Guid assignedToUserId, string? callerDisplayName, int limit, ICurrentUserService user, CancellationToken ct = default)
    {
        var (tasks, totalCount) = await taskRepo.GetMyTasksAsync(assignedToUserId, limit, ct);
        var today = DateTime.UtcNow.Date;
        var summaries = tasks.Select(t => new TaskSummaryDto(
            t.Id, t.Title, t.Status, t.DueDate,
            t.LinkedEntityType, t.LinkedEntityId, null,
            t.DueDate.HasValue && t.DueDate.Value < today && t.Status != "Done",
            callerDisplayName))
            .ToList();

        AuditBrokerUserRead(user, "broker.tasks", null);
        return new MyTasksResponseDto(summaries, totalCount);
    }

    private static TaskDto MapToDto(Domain.Entities.TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
        t.AssignedToUserId, t.LinkedEntityType, t.LinkedEntityId,
        t.CreatedAt, t.UpdatedAt, t.CompletedAt);

    private void AuditBrokerUserRead(ICurrentUserService user, string resource, Guid? entityId, Guid? resolvedBrokerId = null)
    {
        if (!user.Roles.Contains("BrokerUser")) return;
        _logger.LogInformation(
            "BrokerUser access: {Resource} by BrokerTenantId={BrokerTenantId} ResolvedBrokerId={ResolvedBrokerId} EntityId={EntityId} OccurredAt={OccurredAt}",
            resource,
            user.BrokerTenantId,
            resolvedBrokerId,
            entityId,
            DateTime.UtcNow);
    }
}
