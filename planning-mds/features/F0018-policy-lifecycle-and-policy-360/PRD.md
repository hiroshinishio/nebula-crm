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

## Related User Stories

- To be defined during refinement
