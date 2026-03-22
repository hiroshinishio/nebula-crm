---
template: feature
version: 1.1
applies_to: product-manager
---

# F0017: Broker/MGA Hierarchy, Producer Ownership & Territory Management

**Feature ID:** F0017
**Feature Name:** Broker/MGA Hierarchy, Producer Ownership & Territory Management
**Priority:** High
**Phase:** CRM Release MVP+

## Feature Statement

**As a** distribution leader or relationship manager
**I want** broker hierarchy, producer ownership, and territory visibility
**So that** I can manage channels, accountability, and regional performance accurately

## Business Objective

- **Goal:** Reflect real commercial P&C distribution structure inside Nebula.
- **Metric:** Ownership clarity, territory reporting coverage, and hierarchy-based workflow accuracy.
- **Baseline:** Broker records exist, but advanced hierarchy and producer ownership are limited.
- **Target:** Nebula supports channel-aware structure and assignment rules.

## Problem Statement

- **Current State:** Advanced broker hierarchy and producer ownership are not fully modeled.
- **Desired State:** MGAs, sub-brokers, producers, and territories are represented explicitly.
- **Impact:** Better governance, reporting, and relationship accountability.

## Scope & Boundaries

**In Scope:**
- MGA to broker hierarchy
- Producer ownership and territory assignment
- Hierarchy-aware visibility and reporting
- Relationship and production rollups

**Out of Scope:**
- Commission calculation
- External producer portal
- Carrier-side appointment detail

## Success Criteria

- Users can model and navigate broker hierarchy accurately.
- Ownership and territory logic support downstream workflows and reporting.
- Hierarchy supports operational and analytical use cases without manual workarounds.

## Risks & Assumptions

- **Risk:** Hierarchy design becomes overly complex before core CRM workflows are stable.
- **Assumption:** Producer ownership is more valuable once account, submission, and reporting foundations exist.
- **Mitigation:** Sequence this feature after CRM Release MVP core workflow delivery.

## Dependencies

- F0002 Broker & MGA Relationship Management
- F0023 Global Search, Saved Views & Operational Reporting

## Related User Stories

- To be defined during refinement
