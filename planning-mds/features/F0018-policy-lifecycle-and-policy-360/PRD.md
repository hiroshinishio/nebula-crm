---
template: feature
version: 1.1
applies_to: product-manager
---

# F0018: Policy Lifecycle & Policy 360

**Feature ID:** F0018
**Feature Name:** Policy Lifecycle & Policy 360
**Priority:** Critical
**Phase:** CRM Release MVP

## Feature Statement

**As an** underwriter, distribution user, or service user
**I want** policy records with lifecycle visibility and related context
**So that** Nebula can act as a credible commercial P&C system of record

## Business Objective

- **Goal:** Add policy truth to Nebula so the platform is not limited to broker and submission data.
- **Metric:** Policy visibility, linkage coverage to accounts and renewals, and servicing workflow support.
- **Baseline:** Policy detail is not a first-class feature surface today.
- **Target:** Users can manage and navigate policy context directly in Nebula.

## Problem Statement

- **Current State:** Policy information is implicit, incomplete, or external to Nebula.
- **Desired State:** Policies become first-class records with lifecycle events and related workflow.
- **Impact:** Better renewal handling, servicing context, and insurance-domain credibility.

## Scope & Boundaries

**In Scope:**
- Policy master records and policy 360
- Coverages, terms, carrier, premium, and status
- Versions, endorsements, cancellations, and renewal linkage
- Policy relationship to account, broker, and renewal records

**Out of Scope:**
- Full claims servicing
- Billing and commission accounting
- External carrier system synchronization

## Success Criteria

- Users can access policy history and current terms from Nebula.
- Policies link correctly to accounts, renewals, and related activity.
- Policy lifecycle supports downstream renewal and servicing workflows.

## Risks & Assumptions

- **Risk:** Policy scope expands too quickly into AMS-grade servicing.
- **Assumption:** A pragmatic policy record is enough for CRM Release MVP.
- **Mitigation:** Keep initial scope on policy truth, lifecycle events, and core visibility.

## Dependencies

- F0016 Account 360 & Insured Management
- F0020 Document Management & ACORD Intake

## Architecture & Solution Design

### Solution Components

- Introduce a `Policy` aggregate with supporting services for policy creation, term/version management, endorsements, cancellations, and renewal linkage.
- Add a policy 360 composition layer so policy detail, timeline, documents, billing hooks, and related account context can be rendered without tight coupling to every dependent module.
- Separate policy lifecycle logic from submission workflow logic even when one originated from the other, because issued policy state evolves under different business rules.
- Add policy timeline and history views as first-class read models rather than reconstructing lifecycle history from raw changes at query time.

### Data & Workflow Design

- Model effective dates, expiration dates, carrier, line of business, premium, status, and policy number as first-class policy attributes with strong uniqueness and indexing requirements.
- Represent endorsements, cancellations, reinstatements, and version changes as linked policy events or child records so issued history remains traceable.
- Link policies to accounts, brokers, submissions, renewals, and documents through stable foreign keys instead of duplicating context across modules.
- Keep immutable issued-state snapshots or version records where policy history must remain explainable for audit and servicing.

### API & Integration Design

- Expose policy CRUD, detail, related-document, and lifecycle action endpoints with explicit operations for endorsements, cancellations, and renewal linkage.
- Integrate with F0020 for document attachment and retrieval, but keep carrier synchronization and billing settlement outside the core policy module.
- Emit policy lifecycle events that downstream modules such as renewals, billing, and reporting can consume without directly coupling to policy internals.
- Support account and broker 360 navigation through consistent policy identifiers and list/filter contracts.

### Security & Operational Considerations

- Apply authorization based on broker, account, territory, and servicing scope because policy data is often more sensitive than pre-bind workflow records.
- Audit all policy lifecycle mutations and status changes, especially cancellations, reinstatements, and version corrections.
- Guard against race conditions on versioned updates with optimistic concurrency or equivalent write protection.
- Index policy lookup on policy number, account, expiration date, status, and carrier because these dimensions drive both daily operations and future finance workflows.

## Architecture Traceability

**Taxonomy Reference:** [Feature Architecture Traceability Taxonomy](../../architecture/feature-architecture-traceability-taxonomy.md)

| Classification | Artifact / Decision | ADR |
|----------------|---------------------|-----|
| Introduces: Feature-Local Component | Policy aggregate, version and endorsement services, and policy timeline | PRD only |
| Extends: Cross-Cutting Component | Policy records become a primary source for shared document linkage and renewal orchestration | [ADR-012](../../architecture/decisions/ADR-012-shared-document-storage-and-metadata-architecture.md) (Proposed), [ADR-010](../../architecture/decisions/ADR-010-temporal-durable-workflow-orchestration.md) (Proposed) |
| Reuses: Established Component/Pattern | Explicit lifecycle-state modeling with auditable transitions for policy operations | [ADR-011](../../architecture/decisions/ADR-011-crm-workflow-state-machines-and-transition-history.md) (Proposed) |

## Related User Stories

- To be defined during refinement
