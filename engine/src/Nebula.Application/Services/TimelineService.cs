using Microsoft.Extensions.Logging;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;

namespace Nebula.Application.Services;

public class TimelineService(ITimelineRepository timelineRepo, ILogger<TimelineService> logger)
{
    private readonly ILogger<TimelineService> _logger = logger;

    public async Task<IReadOnlyList<TimelineEventDto>> ListEventsAsync(
        string entityType, Guid? entityId, int limit, ICurrentUserService user, CancellationToken ct = default)
    {
        var events = await timelineRepo.ListEventsAsync(entityType, entityId, limit, ct);
        AuditBrokerUserRead(user, "broker.timeline", entityId);
        return events.Select(e => new TimelineEventDto(
            e.Id, e.EntityType, e.EntityId, e.EventType,
            e.EventDescription, null, e.ActorDisplayName, e.OccurredAt))
            .ToList();
    }

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
