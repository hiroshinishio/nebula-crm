using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;
using Nebula.Domain.Entities;

namespace Nebula.Application.Services;

public class TaskService(
    ITaskRepository taskRepo,
    ITimelineRepository timelineRepo,
    IUnitOfWork unitOfWork,
    IAuthorizationService authz,
    BrokerScopeResolver scopeResolver,
    ILogger<TaskService> logger)
{
    private readonly ILogger<TaskService> _logger = logger;

    // Valid status transitions: (from, to)
    private static readonly HashSet<(string, string)> ValidTransitions =
    [
        ("Open", "InProgress"),
        ("InProgress", "Open"),
        ("InProgress", "Done"),
        ("Done", "Open"),
        ("Done", "InProgress"),
    ];

    public async Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await taskRepo.GetByIdAsync(id, ct);
        return task is null ? null : MapToDto(task);
    }

    /// <summary>
    /// Fetch task by ID and verify the caller is authorized (Casbin + ownership).
    /// Returns (dto, null) on success, or (null, errorCode) on failure.
    /// Both not-found and not-authorized return "not_found" to prevent IDOR enumeration.
    /// </summary>
    public async Task<(TaskDto? Dto, string? ErrorCode)> GetByIdAuthorizedAsync(
        Guid id, ICurrentUserService user, CancellationToken ct = default)
    {
        var task = await taskRepo.GetByIdAsync(id, ct);
        if (task is null)
            return (null, "not_found");

        if (!await AuthorizeTaskAsync(user, "read", task))
            return (null, "not_found"); // Normalize to 404 — prevent IDOR

        return (MapToDto(task), null);
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

    /// <summary>
    /// BrokerUser variant: returns tasks scoped to the resolved broker entity (F0009 §12).
    /// Only tasks where LinkedEntityType='Broker' AND LinkedEntityId=resolvedBrokerId.
    /// Throws BrokerScopeUnresolvableException if scope cannot be resolved.
    /// </summary>
    public async Task<MyTasksResponseDto> GetBrokerScopedTasksAsync(
        int limit, ICurrentUserService user, CancellationToken ct = default)
    {
        var resolvedBrokerId = await scopeResolver.ResolveAsync(user, ct);
        var (tasks, totalCount) = await taskRepo.GetBrokerScopedTasksAsync(resolvedBrokerId, limit, ct);
        var today = DateTime.UtcNow.Date;
        var summaries = tasks.Select(t => new TaskSummaryDto(
            t.Id, t.Title, t.Status, t.DueDate,
            t.LinkedEntityType, t.LinkedEntityId, null,
            t.DueDate.HasValue && t.DueDate.Value < today && t.Status != "Done",
            null)) // assignee display name not returned to BrokerUser
            .ToList();

        AuditBrokerUserRead(user, "broker.tasks", resolvedBrokerId, resolvedBrokerId);
        return new MyTasksResponseDto(summaries, totalCount);
    }

    // F0003-S0001: Create Task
    public async Task<(TaskDto? Dto, string? ErrorCode)> CreateAsync(
        TaskCreateRequestDto dto, ICurrentUserService user, CancellationToken ct = default)
    {
        // Self-assignment guard
        if (dto.AssignedToUserId != user.UserId)
            return (null, "forbidden");

        // LinkedEntity pairing guard (also validated by FluentValidation, but defense-in-depth)
        if ((dto.LinkedEntityType is not null) != (dto.LinkedEntityId is not null))
            return (null, "validation_error");

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = "Open",
            Priority = dto.Priority ?? "Normal",
            DueDate = dto.DueDate,
            AssignedToUserId = dto.AssignedToUserId,
            LinkedEntityType = dto.LinkedEntityType,
            LinkedEntityId = dto.LinkedEntityId,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = user.UserId,
            UpdatedByUserId = user.UserId,
        };

        await taskRepo.AddAsync(task, ct);

        await timelineRepo.AddEventAsync(new ActivityTimelineEvent
        {
            EntityType = "Task",
            EntityId = task.Id,
            EventType = "TaskCreated",
            EventDescription = $"Task \"{task.Title}\" created",
            BrokerDescription = null, // InternalOnly
            ActorUserId = user.UserId,
            ActorDisplayName = user.DisplayName,
            OccurredAt = now,
            EventPayloadJson = JsonSerializer.Serialize(new
            {
                title = task.Title,
                assignedToUserId = task.AssignedToUserId,
                dueDate = task.DueDate,
                linkedEntityType = task.LinkedEntityType,
                linkedEntityId = task.LinkedEntityId,
            }),
        }, ct);

        await unitOfWork.CommitAsync(ct);

        return (MapToDto(task), null);
    }

    // F0003-S0002: Update Task
    // Performs single fetch + Casbin auth + ownership guard + mutation (eliminates TOCTOU double-fetch).
    public async Task<(TaskDto? Dto, string? ErrorCode, string? TransitionFrom, string? TransitionTo)> UpdateAsync(
        Guid taskId, TaskUpdateRequestDto dto, IReadOnlySet<string> presentFields,
        uint rowVersion, ICurrentUserService user, CancellationToken ct = default)
    {
        var task = await taskRepo.GetByIdAsync(taskId, ct);
        if (task is null)
            return (null, "not_found", null, null);

        // Casbin authorization (single fetch — no TOCTOU gap)
        if (!await AuthorizeTaskAsync(user, "update", task))
            return (null, "not_found", null, null); // Normalize to 404 — prevent IDOR

        // Ownership guard (defense-in-depth — Casbin already checks assignee == subjectId)
        if (task.AssignedToUserId != user.UserId)
            return (null, "not_found", null, null);

        // AssignedToUserId reassignment guard
        if (dto.AssignedToUserId.HasValue && dto.AssignedToUserId.Value != user.UserId)
            return (null, "forbidden", null, null);

        // Set RowVersion for optimistic concurrency (C-1 fix)
        task.RowVersion = rowVersion;

        var now = DateTime.UtcNow;
        var oldStatus = task.Status;
        var changedFields = new Dictionary<string, object?>();

        // Status transition validation
        if (dto.Status is not null && dto.Status != oldStatus)
        {
            if (!ValidTransitions.Contains((oldStatus, dto.Status)))
                return (null, "invalid_status_transition", oldStatus, dto.Status);
        }

        // Apply present fields
        if (dto.Title is not null)
        {
            if (dto.Title != task.Title)
                changedFields["title"] = new { from = task.Title, to = dto.Title };
            task.Title = dto.Title;
        }

        if (presentFields.Contains("description"))
        {
            if (dto.Description != task.Description)
                changedFields["description"] = new { from = task.Description, to = dto.Description };
            task.Description = dto.Description;
        }

        if (dto.Status is not null)
        {
            if (dto.Status != oldStatus)
                changedFields["status"] = new { from = oldStatus, to = dto.Status };
            task.Status = dto.Status;
        }

        if (dto.Priority is not null)
        {
            if (dto.Priority != task.Priority)
                changedFields["priority"] = new { from = task.Priority, to = dto.Priority };
            task.Priority = dto.Priority;
        }

        if (presentFields.Contains("dueDate"))
        {
            if (dto.DueDate != task.DueDate)
                changedFields["dueDate"] = new { from = task.DueDate, to = dto.DueDate };
            task.DueDate = dto.DueDate;
        }

        if (dto.AssignedToUserId.HasValue)
        {
            task.AssignedToUserId = dto.AssignedToUserId.Value;
        }

        // CompletedAt handling for status transitions
        var previousCompletedAt = task.CompletedAt; // Capture before clearing
        if (dto.Status is not null && dto.Status != oldStatus)
        {
            if (dto.Status == "Done")
                task.CompletedAt = now;
            else if (oldStatus == "Done")
                task.CompletedAt = null;
        }

        task.UpdatedAt = now;
        task.UpdatedByUserId = user.UserId;

        // Emit appropriate timeline event
        string eventType;
        string eventDescription;
        string payloadJson;

        if (dto.Status is not null && dto.Status != oldStatus && dto.Status == "Done")
        {
            eventType = "TaskCompleted";
            eventDescription = "Task completed";
            payloadJson = JsonSerializer.Serialize(new { completedAt = task.CompletedAt });
        }
        else if (dto.Status is not null && dto.Status != oldStatus && oldStatus == "Done")
        {
            eventType = "TaskReopened";
            eventDescription = "Task reopened";
            payloadJson = JsonSerializer.Serialize(new { previousCompletedAt });
        }
        else
        {
            eventType = "TaskUpdated";
            eventDescription = changedFields.Count > 0
                ? $"Task updated ({string.Join(", ", changedFields.Keys)})"
                : "Task updated";
            payloadJson = JsonSerializer.Serialize(new { changedFields });
        }

        await timelineRepo.AddEventAsync(new ActivityTimelineEvent
        {
            EntityType = "Task",
            EntityId = task.Id,
            EventType = eventType,
            EventDescription = eventDescription,
            BrokerDescription = null, // InternalOnly
            ActorUserId = user.UserId,
            ActorDisplayName = user.DisplayName,
            OccurredAt = now,
            EventPayloadJson = payloadJson,
        }, ct);

        await unitOfWork.CommitAsync(ct);

        return (MapToDto(task), null, null, null);
    }

    // F0003-S0003: Delete Task (soft delete)
    // Performs single fetch + Casbin auth + ownership guard + mutation (eliminates TOCTOU double-fetch).
    public async Task<string?> DeleteAsync(
        Guid taskId, ICurrentUserService user, CancellationToken ct = default)
    {
        var task = await taskRepo.GetByIdAsync(taskId, ct);
        if (task is null)
            return "not_found";

        // Casbin authorization (single fetch — no TOCTOU gap)
        if (!await AuthorizeTaskAsync(user, "delete", task))
            return "not_found"; // Normalize to 404 — prevent IDOR

        // Ownership guard (defense-in-depth)
        if (task.AssignedToUserId != user.UserId)
            return "not_found";

        var now = DateTime.UtcNow;
        task.IsDeleted = true;
        task.DeletedAt = now;
        task.DeletedByUserId = user.UserId;
        task.UpdatedAt = now;
        task.UpdatedByUserId = user.UserId;

        await timelineRepo.AddEventAsync(new ActivityTimelineEvent
        {
            EntityType = "Task",
            EntityId = task.Id,
            EventType = "TaskDeleted",
            EventDescription = "Task deleted",
            BrokerDescription = null, // InternalOnly
            ActorUserId = user.UserId,
            ActorDisplayName = user.DisplayName,
            OccurredAt = now,
            EventPayloadJson = JsonSerializer.Serialize(new { }),
        }, ct);

        await unitOfWork.CommitAsync(ct);

        return null;
    }

    /// <summary>
    /// Check Casbin authorization for a task action against the fetched entity.
    /// Used by Update/Delete/GetById to avoid a second DB fetch (TOCTOU fix).
    /// </summary>
    private async Task<bool> AuthorizeTaskAsync(ICurrentUserService user, string action, TaskItem task)
    {
        var attrs = new Dictionary<string, object>
        {
            ["assignee"] = task.AssignedToUserId,
            ["subjectId"] = user.UserId,
        };
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, "task", action, attrs))
                return true;
        }
        return false;
    }

    private static TaskDto MapToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
        t.AssignedToUserId, t.LinkedEntityType, t.LinkedEntityId,
        t.CreatedAt, t.UpdatedAt, t.CompletedAt, t.RowVersion);

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
