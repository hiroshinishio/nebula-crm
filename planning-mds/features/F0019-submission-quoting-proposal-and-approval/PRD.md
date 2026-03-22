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

## Related User Stories

- To be defined during refinement
