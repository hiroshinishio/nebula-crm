using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;

namespace Nebula.Application.Services;

public class TaskService(ITaskRepository taskRepo)
{
    public async Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await taskRepo.GetByIdAsync(id, ct);
        return task is null ? null : MapToDto(task);
    }

    public async Task<MyTasksResponseDto> GetMyTasksAsync(
        Guid assignedToUserId, string? callerDisplayName, int limit, CancellationToken ct = default)
    {
        var (tasks, totalCount) = await taskRepo.GetMyTasksAsync(assignedToUserId, limit, ct);
        var today = DateTime.UtcNow.Date;
        var summaries = tasks.Select(t => new TaskSummaryDto(
            t.Id, t.Title, t.Status, t.DueDate,
            t.LinkedEntityType, t.LinkedEntityId, null,
            t.DueDate.HasValue && t.DueDate.Value < today && t.Status != "Done",
            callerDisplayName))
            .ToList();

        return new MyTasksResponseDto(summaries, totalCount);
    }

    private static TaskDto MapToDto(Domain.Entities.TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
        t.AssignedToUserId, t.LinkedEntityType, t.LinkedEntityId,
        t.CreatedAt, t.UpdatedAt, t.CompletedAt);
}
