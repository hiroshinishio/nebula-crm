using Microsoft.Extensions.Logging;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;

namespace Nebula.Application.Services;

public class DashboardService(IDashboardRepository dashboardRepo, ILogger<DashboardService> logger)
{
    private readonly ILogger<DashboardService> _logger = logger;

    public Task<DashboardKpisDto> GetKpisAsync(CancellationToken ct = default) =>
        dashboardRepo.GetKpisAsync(ct);

    public Task<DashboardOpportunitiesDto> GetOpportunitiesAsync(CancellationToken ct = default) =>
        dashboardRepo.GetOpportunitiesAsync(ct);

    public Task<OpportunityFlowDto> GetOpportunityFlowAsync(string entityType, int periodDays = 180, CancellationToken ct = default) =>
        dashboardRepo.GetOpportunityFlowAsync(entityType, periodDays, ct);

    public Task<OpportunityItemsDto> GetOpportunityItemsAsync(string entityType, string status, CancellationToken ct = default) =>
        dashboardRepo.GetOpportunityItemsAsync(entityType, status, ct);

    public async Task<NudgesResponseDto> GetNudgesAsync(Guid userId, ICurrentUserService user, CancellationToken ct = default)
    {
        var nudges = await dashboardRepo.GetNudgesAsync(userId, ct);
        AuditBrokerUserRead(user, "dashboard.nudges", null);
        return new NudgesResponseDto(nudges);
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
