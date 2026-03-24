---
template: feature
version: 1.1
applies_to: product-manager
---

# F0006: Submission Intake Workflow

**Feature ID:** F0006
**Feature Name:** Submission Intake Workflow
**Priority:** Critical
**Phase:** MVP

## Feature Statement

**As a** Distribution user or coordinator
**I want** a structured submission intake flow with triage, validation, and ownership
**So that** new business enters Nebula cleanly and reaches the right underwriter quickly

## Business Objective

- **Goal:** Replace fragmented submission intake with a consistent workflow.
- **Metric:** Intake turnaround time, incomplete submission rate, and time-to-assignment.
- **Baseline:** Submissions arrive through fragmented channels with weak status visibility.
- **Target:** Most submissions are triaged, complete, and assigned through Nebula.

## Problem Statement

- **Current State:** Intake is fragmented across email, spreadsheets, and ad hoc follow-up.
- **Desired State:** Submission intake, triage, completeness, and assignment are handled in one place.
- **Impact:** Faster response to brokers and less rework for underwriting teams.

## Scope & Boundaries

**In Scope:**
- Submission creation and intake capture
- Intake status and triage workflow
- Completeness checks for required information
- Initial underwriting assignment and queue handoff

**Out of Scope:**
- Final quote approval workflow
- External broker portal submission entry
- Carrier-side integration and rating

## Success Criteria

- Intake users can create and triage submissions in Nebula.
- Underwriters receive clearer, more complete submission handoff.
- Submission status becomes visible and auditable from creation onward.

## Risks & Assumptions

- **Risk:** Intake scope expands into the full quoting workflow.
- **Assumption:** Document management and assignment rules will be staged alongside or shortly after intake.
- **Mitigation:** Keep intake focused on first-touch workflow and explicit downstream handoff points.

## Dependencies

- F0002 Broker & MGA Relationship Management
- F0009 Authentication + Role-Based Login
- F0020 Document Management & ACORD Intake

## Architecture & Solution Design

### Solution Components

- Introduce a `Submission` aggregate with intake-focused application services for create, triage, completeness validation, assignment, and timeline rendering.
- Add a submission workflow service that owns intake transitions and guard checks instead of scattering transition logic across controllers or UI.
- Add a completeness policy component that evaluates required fields and required document categories before a submission can advance to underwriting review.
- Treat queue handoff as an explicit boundary so F0006 can hand work to underwriting without embedding the full routing engine that belongs to F0022.

### Data & Workflow Design

- Model intake as an explicit state machine, starting with intake-focused states such as `Received`, `Triaging`, `WaitingOnBroker`, and `ReadyForUWReview`.
- Record all submission status changes in an append-only `WorkflowTransition` history that aligns with the existing solution pattern for auditable workflow state.
- Keep submission-to-broker, submission-to-account, and submission-to-document relationships first-class so intake completeness can be evaluated without denormalized guessing.
- Expose a read-side completeness projection so intake users can see missing fields, missing documents, and ownership gaps without recalculating every rule in the UI.

### API & Integration Design

- Plan REST endpoints for submission create/list/detail plus explicit transition and assignment actions rather than relying on implicit free-form status edits.
- Integrate with F0020 through document metadata linkage and required-document checks, but keep OCR, extraction, and carrier ingestion outside this feature boundary.
- Emit domain events or application notifications for assignment handoff and broker follow-up so downstream tasking and routing can evolve without rewriting the intake core.
- Preserve correlation IDs between submission actions, document uploads, and timeline events for future reporting and troubleshooting.

### Security & Operational Considerations

- Enforce Casbin-based authorization on submission read/write/transition actions using broker, account, and role context rather than role-only checks.
- Ensure intake mutations create immutable timeline and transition records to support compliance, SLA reporting, and later operational analytics.
- Design create and transition operations to be idempotent enough for retried requests and UI refresh flows.
- Keep list and queue-facing queries pageable and index-friendly because intake views will become high-volume operational screens.

## Architecture Traceability

**Taxonomy Reference:** [Feature Architecture Traceability Taxonomy](../../architecture/feature-architecture-traceability-taxonomy.md)

| Classification | Artifact / Decision | ADR |
|----------------|---------------------|-----|
| Introduces: Feature-Local Component | Submission aggregate, completeness policy, and intake workflow service | PRD only |
| Introduces/Standardizes: Cross-Cutting Pattern | CRM workflow state machine with append-only transition history for submission intake | [ADR-011](../../architecture/decisions/ADR-011-crm-workflow-state-machines-and-transition-history.md) (Proposed) |
| Extends: Cross-Cutting Component | Intake completeness and document linkage rely on the shared document architecture | [ADR-012](../../architecture/decisions/ADR-012-shared-document-storage-and-metadata-architecture.md) (Proposed) |

## Related User Stories

- To be defined during refinement
