---
template: feature
version: 1.1
applies_to: product-manager
---

# F0026: Billing, Invoicing & Reconciliation

**Feature ID:** F0026
**Feature Name:** Billing, Invoicing & Reconciliation
**Priority:** Medium
**Phase:** Brokerage Platform Expansion

## Feature Statement

**As a** finance-facing operations user
**I want** billing, invoicing, and reconciliation support
**So that** Nebula can extend into brokerage operating workflows beyond relationship management

## Business Objective

- **Goal:** Add finance-adjacent operating visibility to Nebula.
- **Metric:** Billing workflow coverage, invoice visibility, and reconciliation completeness.
- **Baseline:** Nebula does not currently address billing operations.
- **Target:** Billing and policy-related financial transactions are visible and trackable in Nebula.

## Problem Statement

- **Current State:** Finance and reconciliation processes sit outside the current CRM experience.
- **Desired State:** Nebula can support billing-facing workflows linked to policies and production.
- **Impact:** Stronger operational continuity, but outside strict CRM scope.

## Scope & Boundaries

**In Scope:**
- Invoice and billing event records
- Reconciliation workflow visibility
- Policy-linked billing context
- Finance-facing operational tracking

**Out of Scope:**
- Full accounting platform replacement
- General ledger
- Bank integration

## Success Criteria

- Billing workflow can be modeled and tracked in Nebula.
- Finance-facing users can connect billing activity to policies and relationships.
- The feature stays clearly bounded from full accounting replacement.

## Risks & Assumptions

- **Risk:** This feature drives the product too deeply into AMS/accounting territory too early.
- **Assumption:** Billing visibility can be useful without replacing finance systems.
- **Mitigation:** Keep scope workflow-oriented, not ledger-oriented.

## Dependencies

- F0018 Policy Lifecycle & Policy 360
- F0025 Commission, Producer Splits & Revenue Tracking

## Related User Stories

- To be defined during refinement
