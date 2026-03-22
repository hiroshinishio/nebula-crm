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

## Related User Stories

- To be defined during refinement
