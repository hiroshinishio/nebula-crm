using Microsoft.Extensions.Logging;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;

namespace Nebula.Application.Services;

public class DashboardService(IDashboardRepository dashboardRepo, BrokerScopeResolver scopeResolver, ILogger<DashboardService> logger)
{
    private readonly ILogger<DashboardService> _logger = logger;

    public Task<DashboardKpisDto> GetKpisAsync(CancellationToken ct = default) =>
        dashboardRepo.GetKpisAsync(ct);

    public Task<DashboardOpportunitiesDto> GetOpportunitiesAsync(int periodDays = 180, CancellationToken ct = default) =>
        dashboardRepo.GetOpportunitiesAsync(periodDays, ct);

    public Task<OpportunityFlowDto> GetOpportunityFlowAsync(string entityType, int periodDays = 180, CancellationToken ct = default) =>
        dashboardRepo.GetOpportunityFlowAsync(entityType, periodDays, ct);

    public Task<OpportunityItemsDto> GetOpportunityItemsAsync(string entityType, string status, CancellationToken ct = default) =>
        dashboardRepo.GetOpportunityItemsAsync(entityType, status, ct);

    public Task<OpportunityAgingDto> GetOpportunityAgingAsync(string entityType, int periodDays = 180, CancellationToken ct = default) =>
        dashboardRepo.GetOpportunityAgingAsync(entityType, periodDays, ct);

    public Task<OpportunityHierarchyDto> GetOpportunityHierarchyAsync(int periodDays = 180, CancellationToken ct = default) =>
        dashboardRepo.GetOpportunityHierarchyAsync(periodDays, ct);

    public Task<OpportunityOutcomesDto> GetOpportunityOutcomesAsync(int periodDays = 180, CancellationToken ct = default) =>
        dashboardRepo.GetOpportunityOutcomesAsync(periodDays, ct);

    public Task<OpportunityItemsDto> GetOpportunityOutcomeItemsAsync(string outcomeKey, int periodDays = 180, CancellationToken ct = default) =>
        dashboardRepo.GetOpportunityOutcomeItemsAsync(outcomeKey, periodDays, ct);

    public async Task<NudgesResponseDto> GetNudgesAsync(Guid userId, ICurrentUserService user, CancellationToken ct = default)
    {
        var nudges = await dashboardRepo.GetNudgesAsync(userId, ct);
        AuditBrokerUserRead(user, "dashboard.nudges", null);
        return new NudgesResponseDto(nudges);
    }

    /// <summary>
    /// BrokerUser variant: returns only OverdueTask nudges for tasks linked to their broker scope (F0009 §14).
    /// Empty result returned if no overdue tasks; 403 thrown only if scope cannot be resolved.
    /// </summary>
    public async Task<NudgesResponseDto> GetNudgesForBrokerUserAsync(ICurrentUserService user, CancellationToken ct = default)
    {
        var resolvedBrokerId = await scopeResolver.ResolveAsync(user, ct);
        var nudges = await dashboardRepo.GetNudgesForBrokerUserAsync([resolvedBrokerId], ct);
        AuditBrokerUserRead(user, "dashboard.nudges", null, resolvedBrokerId);
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
