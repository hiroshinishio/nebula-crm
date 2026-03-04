using Microsoft.EntityFrameworkCore;
using Nebula.Application.Interfaces;
using Nebula.Domain.Entities;
using Nebula.Infrastructure.Persistence;

namespace Nebula.Infrastructure.Repositories;

public class TimelineRepository(AppDbContext db) : ITimelineRepository
{
    public async Task<IReadOnlyList<ActivityTimelineEvent>> ListEventsAsync(
        string entityType, Guid? entityId, int limit, CancellationToken ct = default)
    {
        var query = db.ActivityTimelineEvents
            .Where(e => e.EntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(e => e.EntityId == entityId.Value);

        return await query
            .OrderByDescending(e => e.OccurredAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public Task AddEventAsync(ActivityTimelineEvent evt, CancellationToken ct = default)
    {
        db.ActivityTimelineEvents.Add(evt);
        return Task.CompletedTask;
    }
}
