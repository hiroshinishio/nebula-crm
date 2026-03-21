using Microsoft.EntityFrameworkCore;
using Nebula.Application.Interfaces;
using Nebula.Domain.Entities;
using Nebula.Infrastructure.Persistence;

namespace Nebula.Infrastructure.Repositories;

public class TaskRepository(AppDbContext db) : ITaskRepository
{
    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<(IReadOnlyList<TaskItem> Tasks, int TotalCount)> GetMyTasksAsync(
        Guid assignedToUserId, int limit, CancellationToken ct = default)
    {
        var query = db.Tasks
            .Where(t => t.AssignedToUserId == assignedToUserId && t.Status != "Done");

        var totalCount = await query.CountAsync(ct);

        var tasks = await query
            .OrderBy(t => t.DueDate.HasValue ? 0 : 1)
            .ThenBy(t => t.DueDate)
            .Take(limit)
            .ToListAsync(ct);

        return (tasks, totalCount);
    }

    public async Task<(IReadOnlyList<TaskItem> Tasks, int TotalCount)> GetBrokerScopedTasksAsync(
        Guid brokerId, int limit, CancellationToken ct = default)
    {
        // F0009 §12: tasks where LinkedEntityType='Broker' AND LinkedEntityId=brokerId AND IsDeleted=false.
        // The global IsDeleted query filter handles the soft-delete guard; IsDeleted=false is
        // also applied explicitly here in case the filter is bypassed.
        var query = db.Tasks
            .Where(t => t.LinkedEntityType == "Broker" && t.LinkedEntityId == brokerId && !t.IsDeleted);

        var totalCount = await query.CountAsync(ct);
        var tasks = await query
            .OrderBy(t => t.DueDate.HasValue ? 0 : 1)
            .ThenBy(t => t.DueDate)
            .Take(limit)
            .ToListAsync(ct);

        return (tasks, totalCount);
    }

    public async Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        await db.Tasks.AddAsync(task, ct);
    }
}
