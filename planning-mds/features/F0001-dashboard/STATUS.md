# F0001 — Dashboard — Status

**Overall Status:** Done
**Last Updated:** 2026-03-07

## Story Checklist

| Story | Title | Status | Notes |
|-------|-------|--------|-------|
| F0001-S0001 | View Key Metrics Cards | Done | Confirmed in code + integration tests |
| F0001-S0002 | View Pipeline Summary (Mini-Kanban) | Done | Click-through and "View all" links intentionally deferred per MVP constraints (F0006/F0007) |
| F0001-S0003 | View My Tasks and Reminders | Done | Non-broker task click-through and "View all" link deferred per MVP constraints (F0003) |
| F0001-S0004 | View Broker Activity Feed | Done | Confirmed in code + integration tests |
| F0001-S0005 | View Nudge Cards | Done | All gaps resolved: (1) ABAC scope filter (AssignedToUserId == userId) added to stale-submission and upcoming-renewal queries; (2) staleness now computed from last WorkflowTransition.OccurredAt where ToState == CurrentStatus, with CreatedAt fallback; (3) backend cap raised 3→10 enabling client dismiss-and-replace pool; (4) NudgeCard container has role="alert"; (5) NudgePriorityTests.cs integration tests cover priority ordering, 10-item cap, and WorkflowTransition staleness path |

## Resolved (F0009 Complete)

- Dashboard role/tenant enforcement implemented in F0009-S0004.
- BrokerUser nudge scope (OverdueTask only, broker-linked) implemented in F0009 and confirmed in DashboardService/DashboardRepository.
