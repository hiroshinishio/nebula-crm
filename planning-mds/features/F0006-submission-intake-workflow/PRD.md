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

## Related User Stories

- To be defined during refinement
