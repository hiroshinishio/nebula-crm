using Nebula.Application.DTOs;

namespace Nebula.Application.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardKpisDto> GetKpisAsync(CancellationToken ct = default);
    Task<DashboardOpportunitiesDto> GetOpportunitiesAsync(int periodDays = 180, CancellationToken ct = default);
    Task<OpportunityFlowDto> GetOpportunityFlowAsync(string entityType, int periodDays, CancellationToken ct = default);
    Task<OpportunityItemsDto> GetOpportunityItemsAsync(string entityType, string status, CancellationToken ct = default);
    Task<OpportunityAgingDto> GetOpportunityAgingAsync(string entityType, int periodDays, CancellationToken ct = default);
    Task<OpportunityHierarchyDto> GetOpportunityHierarchyAsync(int periodDays, CancellationToken ct = default);
    Task<OpportunityOutcomesDto> GetOpportunityOutcomesAsync(int periodDays, CancellationToken ct = default);
    Task<OpportunityItemsDto> GetOpportunityOutcomeItemsAsync(string outcomeKey, int periodDays, CancellationToken ct = default);
    Task<IReadOnlyList<NudgeCardDto>> GetNudgesAsync(Guid userId, CancellationToken ct = default);
    /// <summary>
    /// BrokerUser variant: returns only OverdueTask nudges linked to the specified broker IDs (F0009 §14).
    /// StaleSubmission and UpcomingRenewal types are excluded entirely.
    /// </summary>
    Task<IReadOnlyList<NudgeCardDto>> GetNudgesForBrokerUserAsync(IReadOnlyList<Guid> brokerIds, CancellationToken ct = default);
}
