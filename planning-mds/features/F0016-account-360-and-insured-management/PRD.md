---
template: feature
version: 1.1
applies_to: product-manager
---

# F0016: Account 360 & Insured Management

**Feature ID:** F0016
**Feature Name:** Account 360 & Insured Management
**Priority:** Critical
**Phase:** CRM Release MVP

## Feature Statement

**As an** underwriter, distribution user, or relationship manager
**I want** a complete insured-centered account record
**So that** I can understand all related activity, submissions, policies, renewals, and relationships without switching systems

## Business Objective

- **Goal:** Make the account the primary insured context surface inside Nebula.
- **Metric:** Time to gather account context and number of workflows supported from account view.
- **Baseline:** Account context is fragmented or implicit rather than first-class.
- **Target:** Users can navigate the full insured relationship from one account workspace.

## Problem Statement

- **Current State:** Users lack a dedicated account 360 view with related operational history.
- **Desired State:** Accounts become a first-class CRM record with related people, workflow, and policy context.
- **Impact:** Better underwriting decisions, faster service, and less time lost searching.

## Scope & Boundaries

**In Scope:**
- Account CRUD and account profile
- Account 360 view with related records
- Account contacts and relationship context
- Account activity timeline and operational summaries

**Out of Scope:**
- Claims servicing detail
- Full billing and finance operations
- External self-service access

## Success Criteria

- Users can access complete insured context from a dedicated account surface.
- Account relationships to submissions, renewals, and policies are visible and navigable.
- Account 360 supports underwriting and distribution workflows directly.

## Risks & Assumptions

- **Risk:** Account scope becomes a dumping ground for unrelated features.
- **Assumption:** Policy, renewal, and submission features will connect to the account as their shared context root.
- **Mitigation:** Keep account scope focused on master record, relationships, and 360 visibility.

## Dependencies

- F0002 Broker & MGA Relationship Management
- F0018 Policy Lifecycle & Policy 360

## Architecture & Solution Design

### Solution Components

- Introduce `Account` as a first-class insured aggregate with dedicated CRUD services, contact management coordination, and 360 composition services.
- Add an account summary projection that composes submissions, policies, renewals, activities, and key metrics without making the account module own those downstream records.
- Keep relationship-context rendering in a dedicated read/composition layer so account screens can evolve without duplicating logic across every dependent feature.
- Treat account as the primary insured-centric navigation root for commercial CRM workflows.

### Data & Workflow Design

- Model account-to-broker, account-to-policy, account-to-submission, and account-to-contact relationships explicitly, with durable identifiers that other modules can safely reference.
- Preserve account-level activity and audit history as append-only timeline records rather than relying on inferred history from child objects alone.
- Use denormalized account summary fields or projections for high-value metrics such as active policy count, submission count, renewal due count, and last activity date.
- Keep service cases, claims detail, and finance records linked to account context but owned by their respective modules to avoid aggregate bloat.

### API & Integration Design

- Expose account CRUD plus account-scoped related-record endpoints and 360 summary endpoints that provide a stable entry point for the frontend.
- Reuse contact and activity services where possible rather than creating account-specific duplicates of generic supporting capabilities.
- Design the 360 contract for progressive loading so overview data, related lists, and timeline history can be paged independently.
- Preserve cross-linking to broker, policy, submission, and renewal modules through IDs and deep links instead of embedding duplicated record snapshots everywhere.

### Security & Operational Considerations

- Apply authorization using broker ownership, territory rules, and role scope because accounts are central relationship records with broad downstream reach.
- Treat insured contact and account data as sensitive business data with strong auditability for create, update, merge, and delete operations.
- Keep related-list queries pageable and indexed because account 360 screens can become accidental join-heavy hotspots.
- Ensure account merges or identity corrections are planned carefully because many downstream modules will depend on stable account keys.

## Architecture Traceability

**Taxonomy Reference:** [Feature Architecture Traceability Taxonomy](../../architecture/feature-architecture-traceability-taxonomy.md)

| Classification | Artifact / Decision | ADR |
|----------------|---------------------|-----|
| Introduces: Feature-Local Component | Account aggregate, Account 360 composition layer, and account summary projections | PRD only |
| Reuses: Established Component/Pattern | Timeline-driven relationship context and related-record composition | PRD only |
| PRD-Only Traceability | No additional cross-cutting ADR is required unless Account 360 logic becomes a shared platform abstraction | None currently required |

## Related User Stories

- To be defined during refinement
