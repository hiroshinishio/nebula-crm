---
template: feature
version: 1.1
applies_to: product-manager
---

# F0007: Renewal Pipeline

**Feature ID:** F0007
**Feature Name:** Renewal Pipeline
**Priority:** Critical
**Phase:** MVP

## Feature Statement

**As a** distribution or underwriting user
**I want** a renewal pipeline with timing, outreach, and status management
**So that** expiring business is worked early and retained more consistently

## Business Objective

- **Goal:** Make renewals visible and manageable before deadlines become urgent.
- **Metric:** Renewals worked 90/60/45 days ahead, retention rate, and overdue renewal count.
- **Baseline:** Renewal follow-up is inconsistent and often too late.
- **Target:** Renewal work is visible, owned, and advanced through Nebula with enough lead time.

## Problem Statement

- **Current State:** Teams lack a dedicated renewal operating surface with structured timing and follow-up.
- **Desired State:** Renewals are tracked from creation through outreach, review, quote, and outcome.
- **Impact:** Better retention, less rush work, and clearer accountability.

## Scope & Boundaries

**In Scope:**
- Renewal list and pipeline visibility
- Renewal statuses and ownership
- Time-based visibility windows and overdue flags
- Renewal workflow handoff between distribution and underwriting

**Out of Scope:**
- Full billing operations
- Automated carrier integration
- Claims servicing workflows

## Success Criteria

- Users can identify upcoming renewals before deadlines become critical.
- Renewal work is assigned, visible, and auditable.
- Renewal pipeline supports consistent outreach and quote preparation.

## Risks & Assumptions

- **Risk:** Renewal scope becomes impossible to separate from policy lifecycle.
- **Assumption:** Policy records and account context will be available or planned alongside this feature.
- **Mitigation:** Tie renewal scope explicitly to policy and account dependencies during refinement.

## Dependencies

- F0018 Policy Lifecycle & Policy 360
- F0022 Work Queues, Assignment Rules & Coverage Management

## Architecture & Solution Design

### Solution Components

- Introduce a `Renewal` aggregate or renewal application layer that links expiring policies to renewal work ownership, due windows, outcomes, and related submissions.
- Add a renewal orchestration component for reminder scheduling and escalation timing instead of embedding date math in UI code or ad hoc cron jobs.
- Provide renewal worklist and pipeline projections optimized for due-date windows, ownership, and escalation visibility.
- Keep renewal submission creation as a boundary action so the renewal module can trigger downstream intake or quoting work without duplicating those modules.

### Data & Workflow Design

- Model renewal lifecycle states explicitly, for example `Identified`, `Outreach`, `InReview`, `Quoted`, `Completed`, `Lost`, or equivalent business-approved states.
- Store policy expiration date, target outreach dates, renewal owner, and last-touch timestamps as first-class fields because they drive SLA and escalation behavior.
- Use Temporal for long-running reminder and escalation workflows, with workflow IDs stored for correlation to the renewal record.
- Record all renewal transitions and reminder actions in append-only audit history so the team can explain why a renewal was or was not advanced on time.

### API & Integration Design

- Expose renewal list/detail/transition endpoints plus filtered views by due window, status, owner, and overdue condition.
- Consume policy lifecycle data from F0018 as the authoritative source for effective dates, expiration dates, and policy relationships.
- Allow renewal workflows to emit tasks, notifications, or queue handoff signals while keeping external carrier automation out of scope for this release.
- Design the renewal workflow so scheduled reminders remain stable across restarts, retries, and deploys.

### Security & Operational Considerations

- Apply authorization based on account, broker, territory, and assigned team visibility, not only on the renewal owner field.
- Add observability for Temporal workflow execution, reminder delivery failures, overdue renewals, and escalation counts.
- Make reminder and escalation activities idempotent so retried workflows do not spam users or create duplicate tasks.
- Index renewal queries on expiration date, owner, and status because list performance is central to the operating model.

## Architecture Traceability

**Taxonomy Reference:** [Feature Architecture Traceability Taxonomy](../../architecture/feature-architecture-traceability-taxonomy.md)

| Classification | Artifact / Decision | ADR |
|----------------|---------------------|-----|
| Introduces: Feature-Local Component | Renewal aggregate, due-window worklists, and escalation handling | PRD only |
| Introduces: Cross-Cutting Component | Durable workflow orchestration for reminder scheduling and escalations | [ADR-010](../../architecture/decisions/ADR-010-temporal-durable-workflow-orchestration.md) (Proposed) |
| Introduces/Standardizes: Cross-Cutting Pattern | Renewal state machine with append-only transition and reminder audit history | [ADR-011](../../architecture/decisions/ADR-011-crm-workflow-state-machines-and-transition-history.md) (Proposed) |

## Related User Stories

- To be defined during refinement
