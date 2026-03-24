---
template: feature
version: 1.1
applies_to: product-manager
---

# F0019: Submission Quoting, Proposal & Approval Workflow

**Feature ID:** F0019
**Feature Name:** Submission Quoting, Proposal & Approval Workflow
**Priority:** Critical
**Phase:** CRM Release MVP

## Feature Statement

**As an** underwriter or distribution user
**I want** to move submissions through quoting, proposal, approval, and bind decisions
**So that** Nebula supports the full commercial P&C submission journey instead of only intake

## Business Objective

- **Goal:** Complete the core submission operating workflow inside Nebula.
- **Metric:** Quote turnaround time, approval-cycle time, quote-to-bind ratio, and workflow visibility.
- **Baseline:** Intake may exist, but the downstream quote and approval lifecycle is incomplete.
- **Target:** Users can manage submission progression from intake through decision with auditability.

## Problem Statement

- **Current State:** Submission handling is incomplete if Nebula stops at intake and triage.
- **Desired State:** Quote, proposal, approval, and final decision handling are structured and visible.
- **Impact:** Better underwriting control, faster status answers, and stronger workflow traceability.

## Scope & Boundaries

**In Scope:**
- Submission progression from triage to quote and bind decision
- Approval checkpoints and approval visibility
- Proposal or quote package status handling
- Final decision states and auditable workflow transitions

**Out of Scope:**
- Carrier system rating integration
- Billing and issuance accounting
- External broker self-service quoting

## Success Criteria

- Users can track submission status all the way through quote and bind decision.
- Approval bottlenecks become visible and auditable.
- Submission workflow supports timely broker updates and internal accountability.

## Risks & Assumptions

- **Risk:** Workflow scope expands into carrier-side processing or document generation too early.
- **Assumption:** Intake, policy, and document foundations will exist before or alongside this feature.
- **Mitigation:** Keep scope centered on internal workflow orchestration and decision visibility.

## Dependencies

- F0006 Submission Intake Workflow
- F0020 Document Management & ACORD Intake
- F0018 Policy Lifecycle & Policy 360

## Architecture & Solution Design

### Solution Components

- Extend the submission domain with dedicated quoting, proposal, approval, and decision-handling services rather than treating everything as generic status changes.
- Add an approval component that supports underwriting checkpoints, decision ownership, and future maker-checker or authority-limit logic.
- Introduce proposal or quote-package composition services that assemble documents, pricing context, and approval status into a coherent outbound working set.
- Keep final policy issuance and billing creation as downstream handoff points rather than embedding them directly into this workflow module.

### Data & Workflow Design

- Expand the submission state machine to cover downstream states such as `InReview`, `Quoted`, `BindRequested`, `Bound`, `Declined`, and `Withdrawn`, with business-approved transitions and guards.
- Record approval requests, approval decisions, bind decisions, and workflow transitions as append-only history to preserve traceability and internal accountability.
- Store approval metadata such as approver, authority reason, decision timestamp, and blocking conditions as first-class records rather than free-form comments.
- Design for human-in-the-loop processing where workflows can wait on broker response, underwriter action, or approval authority without losing auditability.

### API & Integration Design

- Expose explicit transition, approval, quote-package, and bind-action endpoints instead of allowing unrestricted field edits on the submission record.
- Integrate with F0020 for proposal artifacts and document completeness, and with F0018 for downstream policy creation or policy-link correlation after bind.
- Emit workflow events for approval pending, approval granted, quote ready, bind requested, and final decision so notifications and reporting remain decoupled.
- Keep carrier rating, issuance accounting, and external broker self-service outside the initial API contract even if later phases integrate with them.

### Security & Operational Considerations

- Enforce authorization with role, assignment, and approval-authority context so only authorized actors can move a submission through sensitive transitions.
- Maintain separation between ordinary workflow edits and approval decisions because those actions may have different audit and review requirements.
- Track workflow latency, approval-cycle time, and stuck-in-state conditions as first-class operational metrics.
- Ensure approval and bind actions are idempotent enough to handle retries or duplicate user submissions without double-processing the business outcome.

## Architecture Traceability

**Taxonomy Reference:** [Feature Architecture Traceability Taxonomy](../../architecture/feature-architecture-traceability-taxonomy.md)

| Classification | Artifact / Decision | ADR |
|----------------|---------------------|-----|
| Introduces: Feature-Local Component | Quote, proposal, approval, and bind-decision handlers | PRD only |
| Reuses: Established Component/Pattern | CRM workflow state machine plus append-only workflow and approval audit history | [ADR-011](../../architecture/decisions/ADR-011-crm-workflow-state-machines-and-transition-history.md) (Proposed) |
| Extends: Cross-Cutting Component | Quote and proposal artifacts rely on the shared document architecture | [ADR-012](../../architecture/decisions/ADR-012-shared-document-storage-and-metadata-architecture.md) (Proposed) |

## Related User Stories

- To be defined during refinement
