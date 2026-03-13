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

    public async Task<DashboardOpportunitiesDto> GetOpportunitiesAsync(int periodDays = 180, CancellationToken ct = default)
    {
        if (periodDays <= 0) periodDays = 180;
        if (periodDays > 730) periodDays = 730;
        var windowStart = DateTime.UtcNow.AddDays(-periodDays);

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
            .Where(s =>
                !submissionTerminalStatuses.Contains(s.CurrentStatus)
                && s.CreatedAt >= windowStart)
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
            .Where(r =>
                !renewalTerminalStatuses.Contains(r.CurrentStatus)
                && r.CreatedAt >= windowStart)
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

    private static readonly IReadOnlyList<OutcomeDefinition> OutcomeDefinitions =
    [
        new("bound", "Bound", "solid"),
        new("no_quote", "No Quote", "red_dashed"),
        new("declined", "Declined", "red_dashed"),
        new("expired", "Expired", "gray_dotted"),
        new("lost_competitor", "Lost to Competitor", "red_dashed"),
    ];

    public async Task<OpportunityOutcomesDto> GetOpportunityOutcomesAsync(
        int periodDays,
        CancellationToken ct = default)
    {
        if (periodDays <= 0) periodDays = 180;
        if (periodDays > 730) periodDays = 730;
        var windowStart = DateTime.UtcNow.AddDays(-periodDays);

        var submissionTerminalStatuses = await db.ReferenceSubmissionStatuses
            .Where(s => s.IsTerminal)
            .Select(s => s.Code)
            .ToHashSetAsync(ct);
        var renewalTerminalStatuses = await db.ReferenceRenewalStatuses
            .Where(s => s.IsTerminal)
            .Select(s => s.Code)
            .ToHashSetAsync(ct);

        var submissionTransitions = await db.WorkflowTransitions
            .Where(wt =>
                wt.WorkflowType == "Submission"
                && wt.OccurredAt >= windowStart
                && submissionTerminalStatuses.Contains(wt.ToState))
            .Select(wt => new ExitTransition(wt.EntityId, wt.ToState, wt.OccurredAt))
            .ToListAsync(ct);

        var renewalTransitions = await db.WorkflowTransitions
            .Where(wt =>
                wt.WorkflowType == "Renewal"
                && wt.OccurredAt >= windowStart
                && renewalTerminalStatuses.Contains(wt.ToState))
            .Select(wt => new ExitTransition(wt.EntityId, wt.ToState, wt.OccurredAt))
            .ToListAsync(ct);

        var firstSubmissionExits = submissionTransitions
            .GroupBy(t => t.EntityId)
            .Select(g => g.OrderBy(t => t.ExitAtUtc).First())
            .ToList();
        var firstRenewalExits = renewalTransitions
            .GroupBy(t => t.EntityId)
            .Select(g => g.OrderBy(t => t.ExitAtUtc).First())
            .ToList();

        var submissionIds = firstSubmissionExits.Select(e => e.EntityId).ToList();
        var renewalIds = firstRenewalExits.Select(e => e.EntityId).ToList();

        var submissionCreatedAt = submissionIds.Count == 0
            ? new Dictionary<Guid, DateTime>()
            : await db.Submissions.IgnoreQueryFilters()
                .Where(s => submissionIds.Contains(s.Id))
                .Select(s => new { s.Id, s.CreatedAt })
                .ToDictionaryAsync(s => s.Id, s => s.CreatedAt, ct);

        var renewalCreatedAt = renewalIds.Count == 0
            ? new Dictionary<Guid, DateTime>()
            : await db.Renewals.IgnoreQueryFilters()
                .Where(r => renewalIds.Contains(r.Id))
                .Select(r => new { r.Id, r.CreatedAt })
                .ToDictionaryAsync(r => r.Id, r => r.CreatedAt, ct);

        var exits = new List<OutcomeExitEntry>();

        foreach (var exit in firstSubmissionExits)
        {
            if (!submissionCreatedAt.TryGetValue(exit.EntityId, out var createdAt))
                continue;

            var outcomeKey = MapOutcomeKey("submission", exit.ExitStatus);
            if (outcomeKey is null)
                continue;

            var daysToExit = Math.Max(0, (int)(exit.ExitAtUtc - createdAt).TotalDays);
            exits.Add(new OutcomeExitEntry(outcomeKey, daysToExit));
        }

        foreach (var exit in firstRenewalExits)
        {
            if (!renewalCreatedAt.TryGetValue(exit.EntityId, out var createdAt))
                continue;

            var outcomeKey = MapOutcomeKey("renewal", exit.ExitStatus);
            if (outcomeKey is null)
                continue;

            var daysToExit = Math.Max(0, (int)(exit.ExitAtUtc - createdAt).TotalDays);
            exits.Add(new OutcomeExitEntry(outcomeKey, daysToExit));
        }

        var grouped = exits
            .GroupBy(e => e.OutcomeKey)
            .ToDictionary(g => g.Key, g => g.ToList());

        var totalExits = exits.Count;

        var outcomes = OutcomeDefinitions.Select(definition =>
        {
            var entries = grouped.GetValueOrDefault(definition.Key, []);
            var count = entries.Count;
            var percent = totalExits == 0
                ? 0
                : Math.Round(count * 100.0 / totalExits, 1);
            double? avgDays = count == 0
                ? null
                : Math.Round(entries.Average(e => e.DaysToExit), 1);

            return new OpportunityOutcomeDto(
                definition.Key,
                definition.Label,
                definition.BranchStyle,
                count,
                percent,
                avgDays);
        }).ToList();

        return new OpportunityOutcomesDto(periodDays, totalExits, outcomes);
    }

    public async Task<OpportunityItemsDto> GetOpportunityOutcomeItemsAsync(
        string outcomeKey,
        int periodDays,
        CancellationToken ct = default)
    {
        if (periodDays <= 0) periodDays = 180;
        if (periodDays > 730) periodDays = 730;
        var normalizedOutcomeKey = outcomeKey.Trim().ToLowerInvariant();

        if (!OutcomeDefinitions.Any(o => o.Key == normalizedOutcomeKey))
            throw new ArgumentOutOfRangeException(nameof(outcomeKey), "Unsupported outcome key.");

        var windowStart = DateTime.UtcNow.AddDays(-periodDays);

        var submissionItems = await GetOutcomeSubmissionItemsAsync(normalizedOutcomeKey, windowStart, ct);
        var renewalItems = await GetOutcomeRenewalItemsAsync(normalizedOutcomeKey, windowStart, ct);

        var combined = submissionItems
            .Concat(renewalItems)
            .OrderByDescending(i => i.DaysInStatus)
            .ThenBy(i => i.EntityName)
            .ToList();

        return new OpportunityItemsDto(combined.Take(5).ToList(), combined.Count);
    }

    private async Task<IReadOnlyList<OpportunityMiniCardDto>> GetOutcomeSubmissionItemsAsync(
        string outcomeKey,
        DateTime windowStart,
        CancellationToken ct)
    {
        var statuses = GetOutcomeStatuses(outcomeKey, "submission");
        if (statuses.Count == 0)
            return [];

        var transitions = await db.WorkflowTransitions
            .Where(wt =>
                wt.WorkflowType == "Submission"
                && wt.OccurredAt >= windowStart
                && statuses.Contains(wt.ToState))
            .Select(wt => new ExitTransition(wt.EntityId, wt.ToState, wt.OccurredAt))
            .ToListAsync(ct);

        if (transitions.Count == 0)
            return [];

        var firstExits = transitions
            .GroupBy(t => t.EntityId)
            .Select(g => g.OrderBy(t => t.ExitAtUtc).First())
            .ToDictionary(e => e.EntityId, e => e);
        var entityIds = firstExits.Keys.ToList();

        var submissions = await db.Submissions
            .Include(s => s.Account)
            .Where(s => entityIds.Contains(s.Id))
            .Select(s => new
            {
                s.Id,
                s.CreatedAt,
                s.PremiumEstimate,
                AccountName = s.Account.Name,
                s.AssignedToUserId,
            })
            .ToListAsync(ct);

        var userIds = submissions.Select(s => s.AssignedToUserId).Distinct().ToList();
        Dictionary<Guid, string?> users = userIds.Count == 0
            ? []
            : await db.UserProfiles
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName })
                .ToDictionaryAsync(u => u.Id, u => (string?)u.DisplayName, ct);

        return submissions.Select(s =>
        {
            var exit = firstExits[s.Id];
            var daysToExit = Math.Max(0, (int)(exit.ExitAtUtc - s.CreatedAt).TotalDays);
            users.TryGetValue(s.AssignedToUserId, out var displayName);

            return new OpportunityMiniCardDto(
                s.Id,
                s.AccountName,
                (double?)s.PremiumEstimate,
                daysToExit,
                GetInitials(displayName),
                displayName);
        }).ToList();
    }

    private async Task<IReadOnlyList<OpportunityMiniCardDto>> GetOutcomeRenewalItemsAsync(
        string outcomeKey,
        DateTime windowStart,
        CancellationToken ct)
    {
        var statuses = GetOutcomeStatuses(outcomeKey, "renewal");
        if (statuses.Count == 0)
            return [];

        var transitions = await db.WorkflowTransitions
            .Where(wt =>
                wt.WorkflowType == "Renewal"
                && wt.OccurredAt >= windowStart
                && statuses.Contains(wt.ToState))
            .Select(wt => new ExitTransition(wt.EntityId, wt.ToState, wt.OccurredAt))
            .ToListAsync(ct);

        if (transitions.Count == 0)
            return [];

        var firstExits = transitions
            .GroupBy(t => t.EntityId)
            .Select(g => g.OrderBy(t => t.ExitAtUtc).First())
            .ToDictionary(e => e.EntityId, e => e);
        var entityIds = firstExits.Keys.ToList();

        var renewals = await db.Renewals
            .Include(r => r.Account)
            .Where(r => entityIds.Contains(r.Id))
            .Select(r => new
            {
                r.Id,
                r.CreatedAt,
                AccountName = r.Account.Name,
                r.AssignedToUserId,
            })
            .ToListAsync(ct);

        var userIds = renewals.Select(r => r.AssignedToUserId).Distinct().ToList();
        Dictionary<Guid, string?> users = userIds.Count == 0
            ? []
            : await db.UserProfiles
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName })
                .ToDictionaryAsync(u => u.Id, u => (string?)u.DisplayName, ct);

        return renewals.Select(r =>
        {
            var exit = firstExits[r.Id];
            var daysToExit = Math.Max(0, (int)(exit.ExitAtUtc - r.CreatedAt).TotalDays);
            users.TryGetValue(r.AssignedToUserId, out var displayName);

            return new OpportunityMiniCardDto(
                r.Id,
                r.AccountName,
                null,
                daysToExit,
                GetInitials(displayName),
                displayName);
        }).ToList();
    }

    private static List<string> GetOutcomeStatuses(string outcomeKey, string workflowType) =>
        (workflowType, outcomeKey) switch
        {
            ("submission", "bound") => ["Bound"],
            ("submission", "no_quote") => ["NotQuoted"],
            ("submission", "declined") => ["Declined"],
            ("submission", "expired") => ["Expired"],
            ("submission", "lost_competitor") => ["Lost", "Withdrawn"],
            ("renewal", "bound") => ["Bound"],
            ("renewal", "no_quote") => ["NotRenewed"],
            ("renewal", "declined") => [],
            ("renewal", "expired") => ["Expired", "Lapsed"],
            ("renewal", "lost_competitor") => ["Lost", "Withdrawn"],
            _ => [],
        };

    private static string? MapOutcomeKey(string workflowType, string statusCode) =>
        (workflowType, statusCode) switch
        {
            ("submission", "Bound") => "bound",
            ("submission", "NotQuoted") => "no_quote",
            ("submission", "Declined") => "declined",
            ("submission", "Expired") => "expired",
            ("submission", "Lost") => "lost_competitor",
            ("submission", "Withdrawn") => "lost_competitor",
            ("renewal", "Bound") => "bound",
            ("renewal", "NotRenewed") => "no_quote",
            ("renewal", "Expired") => "expired",
            ("renewal", "Lapsed") => "expired",
            ("renewal", "Lost") => "lost_competitor",
            ("renewal", "Withdrawn") => "lost_competitor",
            _ => null,
        };

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

    private static readonly (string Key, string Label, int Min, int Max)[] AgingBuckets =
    [
        ("0-2", "0\u20132 days", 0, 2),
        ("3-5", "3\u20135 days", 3, 5),
        ("6-10", "6\u201310 days", 6, 10),
        ("11-20", "11\u201320 days", 11, 20),
        ("21+", "21+ days", 21, int.MaxValue),
    ];

    public async Task<OpportunityAgingDto> GetOpportunityAgingAsync(
        string entityType,
        int periodDays,
        CancellationToken ct = default)
    {
        if (periodDays <= 0) periodDays = 180;
        if (periodDays > 730) periodDays = 730;

        var normalizedEntityType = entityType.Trim().ToLowerInvariant();

        List<StatusMeta> statuses;
        List<EntityAgingEntry> entities;

        if (normalizedEntityType == "submission")
        {
            statuses = await db.ReferenceSubmissionStatuses
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new StatusMeta(s.Code, s.DisplayName, s.IsTerminal, s.DisplayOrder, s.ColorGroup))
                .ToListAsync(ct);

            var candidates = await db.Submissions
                .Select(s => new { s.Id, s.CurrentStatus, s.CreatedAt })
                .ToListAsync(ct);

            var candidateIds = candidates.Select(c => c.Id).ToList();
            var transitions = await db.WorkflowTransitions
                .Where(wt => wt.WorkflowType == "Submission" && candidateIds.Contains(wt.EntityId))
                .Select(wt => new { wt.EntityId, wt.ToState, wt.OccurredAt })
                .ToListAsync(ct);

            var transitionLookup = transitions
                .GroupBy(wt => wt.EntityId)
                .ToDictionary(g => g.Key, g => g.ToList());

            entities = candidates.Select(c =>
            {
                var enteredCurrent = transitionLookup.TryGetValue(c.Id, out var txns)
                    ? txns.Where(t => t.ToState == c.CurrentStatus).Select(t => t.OccurredAt).DefaultIfEmpty(c.CreatedAt).Max()
                    : c.CreatedAt;
                return new EntityAgingEntry(c.CurrentStatus, (int)(DateTime.UtcNow - enteredCurrent).TotalDays);
            }).ToList();
        }
        else if (normalizedEntityType == "renewal")
        {
            statuses = await db.ReferenceRenewalStatuses
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new StatusMeta(s.Code, s.DisplayName, s.IsTerminal, s.DisplayOrder, s.ColorGroup))
                .ToListAsync(ct);

            var candidates = await db.Renewals
                .Select(r => new { r.Id, r.CurrentStatus, r.CreatedAt })
                .ToListAsync(ct);

            var candidateIds = candidates.Select(c => c.Id).ToList();
            var transitions = await db.WorkflowTransitions
                .Where(wt => wt.WorkflowType == "Renewal" && candidateIds.Contains(wt.EntityId))
                .Select(wt => new { wt.EntityId, wt.ToState, wt.OccurredAt })
                .ToListAsync(ct);

            var transitionLookup = transitions
                .GroupBy(wt => wt.EntityId)
                .ToDictionary(g => g.Key, g => g.ToList());

            entities = candidates.Select(c =>
            {
                var enteredCurrent = transitionLookup.TryGetValue(c.Id, out var txns)
                    ? txns.Where(t => t.ToState == c.CurrentStatus).Select(t => t.OccurredAt).DefaultIfEmpty(c.CreatedAt).Max()
                    : c.CreatedAt;
                return new EntityAgingEntry(c.CurrentStatus, (int)(DateTime.UtcNow - enteredCurrent).TotalDays);
            }).ToList();
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(entityType), "entityType must be 'submission' or 'renewal'.");
        }

        var groupedByStatus = entities
            .GroupBy(e => e.Status)
            .ToDictionary(g => g.Key, g => g.ToList());

        var agingStatuses = statuses.Select(s =>
        {
            var statusEntities = groupedByStatus.GetValueOrDefault(s.Code, []);
            var buckets = AgingBuckets.Select(b =>
            {
                var count = statusEntities.Count(e => e.DaysInStatus >= b.Min && e.DaysInStatus <= b.Max);
                return new OpportunityAgingBucketDto(b.Key, b.Label, count);
            }).ToList();

            return new OpportunityAgingStatusDto(
                s.Code, s.DisplayName, s.ColorGroup ?? "intake", s.DisplayOrder, buckets, statusEntities.Count);
        }).ToList();

        return new OpportunityAgingDto(normalizedEntityType, periodDays, agingStatuses);
    }

    public async Task<OpportunityHierarchyDto> GetOpportunityHierarchyAsync(
        int periodDays,
        CancellationToken ct = default)
    {
        if (periodDays <= 0) periodDays = 180;
        if (periodDays > 730) periodDays = 730;

        // Submissions — include all statuses (active + terminal) for composition views
        var submissionStatuses = await db.ReferenceSubmissionStatuses
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new StatusMeta(s.Code, s.DisplayName, s.IsTerminal, s.DisplayOrder, s.ColorGroup))
            .ToListAsync(ct);

        var submissionCounts = await db.Submissions
            .GroupBy(s => s.CurrentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count, ct);

        // Renewals — include all statuses (active + terminal) for composition views
        var renewalStatuses = await db.ReferenceRenewalStatuses
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new StatusMeta(s.Code, s.DisplayName, s.IsTerminal, s.DisplayOrder, s.ColorGroup))
            .ToListAsync(ct);

        var renewalCounts = await db.Renewals
            .GroupBy(r => r.CurrentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count, ct);

        var submissionChildren = BuildHierarchyChildren("submission", submissionStatuses, submissionCounts);
        var renewalChildren = BuildHierarchyChildren("renewal", renewalStatuses, renewalCounts);

        var submissionTotal = submissionChildren.Sum(c => c.Count);
        var renewalTotal = renewalChildren.Sum(c => c.Count);

        var root = new OpportunityHierarchyNodeDto(
            "root", "All Opportunities", submissionTotal + renewalTotal,
            Children:
            [
                new OpportunityHierarchyNodeDto("submission", "Submissions", submissionTotal, "entityType", Children: submissionChildren),
                new OpportunityHierarchyNodeDto("renewal", "Renewals", renewalTotal, "entityType", Children: renewalChildren),
            ]);

        return new OpportunityHierarchyDto(periodDays, root);
    }

    private static List<OpportunityHierarchyNodeDto> BuildHierarchyChildren(
        string entityType,
        List<StatusMeta> statuses,
        Dictionary<string, int> counts)
    {
        return statuses
            .GroupBy(s => s.ColorGroup ?? "intake")
            .Select(colorGrouping =>
            {
                var statusChildren = colorGrouping
                    .Select(s => new OpportunityHierarchyNodeDto(
                        $"{entityType}:{colorGrouping.Key}:{s.Code}",
                        s.DisplayName,
                        counts.GetValueOrDefault(s.Code, 0),
                        "status",
                        colorGrouping.Key))
                    .ToList();

                var groupLabel = char.ToUpperInvariant(colorGrouping.Key[0]) + colorGrouping.Key[1..];

                return new OpportunityHierarchyNodeDto(
                    $"{entityType}:{colorGrouping.Key}",
                    groupLabel,
                    statusChildren.Sum(c => c.Count),
                    "colorGroup",
                    colorGrouping.Key,
                    statusChildren);
            })
            .ToList();
    }

    private sealed record EntityAgingEntry(string Status, int DaysInStatus);
    private sealed record ExitTransition(Guid EntityId, string ExitStatus, DateTime ExitAtUtc);
    private sealed record OutcomeExitEntry(string OutcomeKey, int DaysToExit);
    private sealed record OutcomeDefinition(string Key, string Label, string BranchStyle);

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
