using Microsoft.EntityFrameworkCore;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;
using Nebula.Infrastructure.Persistence;

namespace Nebula.Infrastructure.Repositories;

public class DashboardRepository(AppDbContext db) : IDashboardRepository
{
    public async Task<DashboardKpisDto> GetKpisAsync(CancellationToken ct = default)
    {
        var terminalSubmissionStatuses = await db.ReferenceSubmissionStatuses
            .Where(s => s.IsTerminal)
            .Select(s => s.Code)
            .ToListAsync(ct);
        var terminalRenewalStatuses = await db.ReferenceRenewalStatuses
            .Where(s => s.IsTerminal)
            .Select(s => s.Code)
            .ToListAsync(ct);

        var activeBrokers = await db.Brokers.CountAsync(b => b.Status == "Active", ct);
        var openSubmissions = await db.Submissions
            .CountAsync(s => !terminalSubmissionStatuses.Contains(s.CurrentStatus), ct);

        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);

        // Renewal rate: % of renewals reaching Bound out of all that exited opportunities in 90 days
        var exitedRenewals = await db.Renewals
            .Where(r => terminalRenewalStatuses.Contains(r.CurrentStatus) && r.UpdatedAt >= ninetyDaysAgo)
            .ToListAsync(ct);

        double? renewalRate = exitedRenewals.Count > 0
            ? Math.Round(exitedRenewals.Count(r => r.CurrentStatus == "Bound") * 100.0 / exitedRenewals.Count, 1)
            : null;

        // Avg turnaround: mean days from Submission.CreatedAt to first terminal transition
        var terminalTransitions = await db.WorkflowTransitions
            .Where(wt => wt.WorkflowType == "Submission"
                && terminalSubmissionStatuses.Contains(wt.ToState)
                && wt.OccurredAt >= ninetyDaysAgo)
            .GroupBy(wt => wt.EntityId)
            .Select(g => new { EntityId = g.Key, FirstTerminal = g.Min(wt => wt.OccurredAt) })
            .ToListAsync(ct);

        double? avgTurnaroundDays = null;
        if (terminalTransitions.Count > 0)
        {
            var submissionIds = terminalTransitions.Select(t => t.EntityId).ToList();
            var submissions = await db.Submissions.IgnoreQueryFilters()
                .Where(s => submissionIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.CreatedAt, ct);

            var turnarounds = terminalTransitions
                .Where(t => submissions.ContainsKey(t.EntityId))
                .Select(t => (t.FirstTerminal - submissions[t.EntityId]).TotalDays)
                .ToList();

            if (turnarounds.Count > 0)
                avgTurnaroundDays = Math.Round(turnarounds.Average(), 1);
        }

        return new DashboardKpisDto(activeBrokers, openSubmissions, renewalRate, avgTurnaroundDays);
    }

    public async Task<DashboardOpportunitiesDto> GetOpportunitiesAsync(CancellationToken ct = default)
    {
        var submissionStatuses = await db.ReferenceSubmissionStatuses
            .Where(s => !s.IsTerminal)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new StatusMeta(s.Code, s.DisplayName, s.IsTerminal, s.DisplayOrder, s.ColorGroup))
            .ToListAsync(ct);

        var submissionTerminalStatuses = await db.ReferenceSubmissionStatuses
            .Where(s => s.IsTerminal)
            .Select(s => s.Code)
            .ToListAsync(ct);

        var submissionCounts = await db.Submissions
            .Where(s => !submissionTerminalStatuses.Contains(s.CurrentStatus))
            .GroupBy(s => s.CurrentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count, ct);

        var submissionOpportunities = submissionStatuses
            .Select(s => new OpportunityStatusCountDto(
                s.Code,
                submissionCounts.GetValueOrDefault(s.Code, 0),
                s.ColorGroup ?? "intake"))
            .ToList();

        var renewalStatuses = await db.ReferenceRenewalStatuses
            .Where(s => !s.IsTerminal)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new StatusMeta(s.Code, s.DisplayName, s.IsTerminal, s.DisplayOrder, s.ColorGroup))
            .ToListAsync(ct);

        var renewalTerminalStatuses = await db.ReferenceRenewalStatuses
            .Where(s => s.IsTerminal)
            .Select(s => s.Code)
            .ToListAsync(ct);

        var renewalCounts = await db.Renewals
            .Where(r => !renewalTerminalStatuses.Contains(r.CurrentStatus))
            .GroupBy(r => r.CurrentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count, ct);

        var renewalOpportunities = renewalStatuses
            .Select(s => new OpportunityStatusCountDto(
                s.Code,
                renewalCounts.GetValueOrDefault(s.Code, 0),
                s.ColorGroup ?? "intake"))
            .ToList();

        return new DashboardOpportunitiesDto(submissionOpportunities, renewalOpportunities);
    }

    public async Task<OpportunityFlowDto> GetOpportunityFlowAsync(
        string entityType,
        int periodDays,
        CancellationToken ct = default)
    {
        if (periodDays <= 0) periodDays = 180;
        if (periodDays > 730) periodDays = 730;

        var normalizedEntityType = entityType.Trim().ToLowerInvariant();
        var windowEnd = DateTime.UtcNow;
        var windowStart = windowEnd.AddDays(-periodDays);

        string workflowType;
        List<StatusMeta> statuses;
        Dictionary<string, int> currentCounts;

        if (normalizedEntityType == "submission")
        {
            statuses = await db.ReferenceSubmissionStatuses
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new StatusMeta(s.Code, s.DisplayName, s.IsTerminal, s.DisplayOrder, s.ColorGroup))
                .ToListAsync(ct);

            currentCounts = await db.Submissions
                .GroupBy(s => s.CurrentStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count, ct);

            workflowType = "Submission";
        }
        else if (normalizedEntityType == "renewal")
        {
            statuses = await db.ReferenceRenewalStatuses
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new StatusMeta(s.Code, s.DisplayName, s.IsTerminal, s.DisplayOrder, s.ColorGroup))
                .ToListAsync(ct);

            currentCounts = await db.Renewals
                .GroupBy(r => r.CurrentStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count, ct);

            workflowType = "Renewal";
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(entityType), "entityType must be 'submission' or 'renewal'.");
        }

        var linkRows = await db.WorkflowTransitions
            .Where(wt => wt.WorkflowType == workflowType
                && wt.OccurredAt >= windowStart
                && wt.OccurredAt <= windowEnd
                && wt.FromState != wt.ToState)
            .GroupBy(wt => new { wt.FromState, wt.ToState })
            .Select(g => new OpportunityFlowLinkDto(g.Key.FromState, g.Key.ToState, g.Count()))
            .ToListAsync(ct);

        var inflowByStatus = linkRows
            .GroupBy(l => l.TargetStatus)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));
        var outflowByStatus = linkRows
            .GroupBy(l => l.SourceStatus)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

        var knownStatuses = statuses.Select(s => s.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknownStatuses = linkRows
            .SelectMany(l => new[] { l.SourceStatus, l.TargetStatus })
            .Where(s => !knownStatuses.Contains(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)
            .Select((status, index) => new StatusMeta(status, status, false, (short)(1000 + index), "decision"))
            .ToList();

        var allStatuses = statuses.Concat(unknownStatuses).ToList();

        var nodes = allStatuses.Select(s => new OpportunityFlowNodeDto(
            s.Code,
            s.DisplayName,
            s.IsTerminal,
            s.DisplayOrder,
            s.ColorGroup ?? (s.IsTerminal ? "decision" : "intake"),
            currentCounts.GetValueOrDefault(s.Code, 0),
            inflowByStatus.GetValueOrDefault(s.Code, 0),
            outflowByStatus.GetValueOrDefault(s.Code, 0)))
            .ToList();

        return new OpportunityFlowDto(
            normalizedEntityType,
            periodDays,
            windowStart,
            windowEnd,
            nodes,
            linkRows.OrderByDescending(l => l.Count).ThenBy(l => l.SourceStatus).ThenBy(l => l.TargetStatus).ToList());
    }

    public async Task<OpportunityItemsDto> GetOpportunityItemsAsync(
        string entityType,
        string status,
        CancellationToken ct = default)
    {
        if (entityType == "submission")
        {
            var query = db.Submissions
                .Include(s => s.Account)
                .Where(s => s.CurrentStatus == status);

            var totalCount = await query.CountAsync(ct);

            var lastTransitions = await db.WorkflowTransitions
                .Where(wt => wt.WorkflowType == "Submission" && wt.ToState == status)
                .GroupBy(wt => wt.EntityId)
                .Select(g => new { EntityId = g.Key, LastTransition = g.Max(wt => wt.OccurredAt) })
                .ToDictionaryAsync(g => g.EntityId, g => g.LastTransition, ct);

            var items = await query.Take(5).ToListAsync(ct);

            var userIds = items.Select(s => s.AssignedToUserId).Distinct().ToList();
            var users = await db.UserProfiles
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, ct);

            var miniCards = items.Select(s =>
            {
                var daysInStatus = lastTransitions.TryGetValue(s.Id, out var transitionDate)
                    ? (int)(DateTime.UtcNow - transitionDate).TotalDays
                    : (int)(DateTime.UtcNow - s.CreatedAt).TotalDays;

                users.TryGetValue(s.AssignedToUserId, out var user);
                var initials = GetInitials(user?.DisplayName);

                return new OpportunityMiniCardDto(s.Id, s.Account.Name, (double)s.PremiumEstimate, daysInStatus, initials, user?.DisplayName);
            }).OrderByDescending(c => c.DaysInStatus).ToList();

            return new OpportunityItemsDto(miniCards, totalCount);
        }
        else // renewal
        {
            var query = db.Renewals
                .Include(r => r.Account)
                .Where(r => r.CurrentStatus == status);

            var totalCount = await query.CountAsync(ct);

            var lastTransitions = await db.WorkflowTransitions
                .Where(wt => wt.WorkflowType == "Renewal" && wt.ToState == status)
                .GroupBy(wt => wt.EntityId)
                .Select(g => new { EntityId = g.Key, LastTransition = g.Max(wt => wt.OccurredAt) })
                .ToDictionaryAsync(g => g.EntityId, g => g.LastTransition, ct);

            var items = await query.Take(5).ToListAsync(ct);

            var userIds = items.Select(r => r.AssignedToUserId).Distinct().ToList();
            var users = await db.UserProfiles
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, ct);

            var miniCards = items.Select(r =>
            {
                var daysInStatus = lastTransitions.TryGetValue(r.Id, out var transitionDate)
                    ? (int)(DateTime.UtcNow - transitionDate).TotalDays
                    : (int)(DateTime.UtcNow - r.CreatedAt).TotalDays;

                users.TryGetValue(r.AssignedToUserId, out var user);
                var initials = GetInitials(user?.DisplayName);

                return new OpportunityMiniCardDto(r.Id, r.Account.Name, null, daysInStatus, initials, user?.DisplayName);
            }).OrderByDescending(c => c.DaysInStatus).ToList();

            return new OpportunityItemsDto(miniCards, totalCount);
        }
    }

    public async Task<IReadOnlyList<NudgeCardDto>> GetNudgesAsync(Guid userId, CancellationToken ct = default)
    {
        var nudges = new List<NudgeCardDto>();
        var today = DateTime.UtcNow.Date;

        var terminalSubmissionStatuses = await db.ReferenceSubmissionStatuses
            .Where(s => s.IsTerminal)
            .Select(s => s.Code)
            .ToListAsync(ct);
        var terminalRenewalStatuses = await db.ReferenceRenewalStatuses
            .Where(s => s.IsTerminal)
            .Select(s => s.Code)
            .ToListAsync(ct);

        // Priority 1: Overdue tasks assigned to this user (oldest DueDate first).
        // Scoped to AssignedToUserId — only tasks the user owns are surfaced.
        var overdueTasks = await db.Tasks
            .Where(t => t.AssignedToUserId == userId && t.Status != "Done"
                && t.DueDate.HasValue && t.DueDate.Value < today)
            .OrderBy(t => t.DueDate)
            .Take(10)
            .ToListAsync(ct);

        foreach (var task in overdueTasks)
        {
            var daysOverdue = (int)(today - task.DueDate!.Value).TotalDays;
            nudges.Add(new NudgeCardDto(
                "OverdueTask", task.Title,
                $"{daysOverdue} day{(daysOverdue != 1 ? "s" : "")} overdue",
                task.LinkedEntityType ?? "Task", task.LinkedEntityId ?? task.Id,
                task.Title, daysOverdue, "Review Now"));
        }

        if (nudges.Count >= 10) return nudges.Take(10).ToList();

        // Priority 2: Stale submissions assigned to this user (>5 days in current status).
        // Scoped to AssignedToUserId — only submissions the user owns are surfaced.
        // Staleness is computed from the most recent WorkflowTransition into the submission's
        // current status, not from UpdatedAt. Falls back to CreatedAt when no matching
        // transition record exists (e.g. submission never changed state).
        var candidateSubData = await db.Submissions
            .Where(s => s.AssignedToUserId == userId
                && !terminalSubmissionStatuses.Contains(s.CurrentStatus))
            .Select(s => new { s.Id, s.CurrentStatus, AccountName = s.Account.Name, s.CreatedAt })
            .ToListAsync(ct);

        if (candidateSubData.Count > 0)
        {
            var candidateIds = candidateSubData.Select(x => x.Id).ToList();

            var allSubTransitions = await db.WorkflowTransitions
                .Where(wt => wt.WorkflowType == "Submission" && candidateIds.Contains(wt.EntityId))
                .Select(wt => new { wt.EntityId, wt.ToState, wt.OccurredAt })
                .ToListAsync(ct);

            var transitionsBySubmission = allSubTransitions
                .GroupBy(wt => wt.EntityId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var staleSubmissions = candidateSubData
                .Select(s =>
                {
                    var enteredCurrentStatus = transitionsBySubmission.TryGetValue(s.Id, out var transitions)
                        ? transitions
                            .Where(wt => wt.ToState == s.CurrentStatus)
                            .Select(wt => wt.OccurredAt)
                            .DefaultIfEmpty(s.CreatedAt)
                            .Max()
                        : s.CreatedAt;
                    var daysInStatus = (int)(DateTime.UtcNow - enteredCurrentStatus).TotalDays;
                    return new { s.Id, s.CurrentStatus, s.AccountName, DaysInStatus = daysInStatus };
                })
                .Where(x => x.DaysInStatus > 5)
                .OrderByDescending(x => x.DaysInStatus)
                .Take(10 - nudges.Count)
                .ToList();

            foreach (var sub in staleSubmissions)
            {
                nudges.Add(new NudgeCardDto(
                    "StaleSubmission", $"Follow up on {sub.AccountName}",
                    $"{sub.DaysInStatus} days in {sub.CurrentStatus}",
                    "Submission", sub.Id, sub.AccountName, sub.DaysInStatus, "Take Action"));
            }
        }

        if (nudges.Count >= 10) return nudges.Take(10).ToList();

        // Priority 3: Upcoming renewals assigned to this user (within 14 days, non-terminal).
        // Scoped to AssignedToUserId — only renewals the user owns are surfaced.
        var fourteenDaysFromNow = today.AddDays(14);
        var upcomingRenewals = await db.Renewals
            .Where(r => r.AssignedToUserId == userId
                && !terminalRenewalStatuses.Contains(r.CurrentStatus)
                && r.RenewalDate >= today && r.RenewalDate <= fourteenDaysFromNow)
            .OrderBy(r => r.RenewalDate)
            .Select(r => new { r.Id, r.CurrentStatus, AccountName = r.Account.Name, r.RenewalDate })
            .Take(10 - nudges.Count)
            .ToListAsync(ct);

        foreach (var ren in upcomingRenewals)
        {
            var daysUntil = (int)(ren.RenewalDate - today).TotalDays;
            nudges.Add(new NudgeCardDto(
                "UpcomingRenewal", $"Renewal for {ren.AccountName}",
                $"Due in {daysUntil} day{(daysUntil != 1 ? "s" : "")}",
                "Renewal", ren.Id, ren.AccountName, daysUntil, "Start Outreach"));
        }

        return nudges.Take(10).ToList();
    }

    public async Task<IReadOnlyList<NudgeCardDto>> GetNudgesForBrokerUserAsync(
        IReadOnlyList<Guid> brokerIds, CancellationToken ct = default)
    {
        // F0009 §14: BrokerUser sees only OverdueTask nudges for tasks linked to their broker(s).
        // StaleSubmission and UpcomingRenewal types are excluded entirely.
        var today = DateTime.UtcNow.Date;

        var overdueTasks = await db.Tasks
            .Where(t => t.LinkedEntityType == "Broker"
                && brokerIds.Contains(t.LinkedEntityId!.Value)
                && t.Status != "Done"
                && t.DueDate.HasValue && t.DueDate.Value < today)
            .OrderBy(t => t.DueDate)
            .Take(3)
            .ToListAsync(ct);

        var nudges = overdueTasks.Select(task =>
        {
            var daysOverdue = (int)(today - task.DueDate!.Value).TotalDays;
            return new NudgeCardDto(
                "OverdueTask", task.Title,
                $"{daysOverdue} day{(daysOverdue != 1 ? "s" : "")} overdue",
                "Broker", task.LinkedEntityId!.Value,
                task.Title, daysOverdue, "Review Now");
        }).ToList();

        return nudges;
    }

    private static string? GetInitials(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName)) return null;

        var initials = string.Concat(displayName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part[0]))
            .ToUpperInvariant();

        return initials.Length switch
        {
            0 => null,
            <= 2 => initials,
            _ => initials[..2],
        };
    }

    private sealed record StatusMeta(
        string Code,
        string DisplayName,
        bool IsTerminal,
        short DisplayOrder,
        string? ColorGroup);
}
