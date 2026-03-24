# ADR-011: Standardize CRM Workflow State Machines and Append-Only Transition History

**Status:** Proposed
**Date:** 2026-03-23
**Owners:** Architect
**Related Features:** F0006, F0007, F0019, F0024, F0026

## Context

Multiple planned features introduce business workflows with explicit states, guarded transitions, ownership changes, and auditable status history. Without a shared rule set, each feature could model transitions differently, weakening audit consistency and increasing implementation drift.

Nebula already documents workflow transitions as append-only in solution patterns, but the planned CRM feature set now depends on this becoming an explicit architecture decision rather than an implied convention.

## Decision

Standardize business workflows on explicit state machines with:

- declared valid transitions
- guard and authorization checks before transition
- append-only transition history
- immutable timestamps, actor identity, and transition reason

Immediate user-driven transitions remain application-service concerns. Durable orchestration engines such as Temporal may trigger those transitions, but they do not replace the state-machine contract.

## Scope

This ADR governs:

- submission workflow transitions
- renewal workflow transitions
- service and reconciliation workflows where explicit state exists
- transition audit history and correlation expectations

## Consequences

### Positive

- Workflow rules stay explicit, testable, and auditable.
- Invalid state changes can be rejected consistently.
- Reporting and analytics can rely on structured transition history.

### Negative

- Features must invest in transition modeling rather than ad hoc status edits.
- Shared workflow terminology and validation contracts must be maintained carefully.

## Follow-up

- Create workflow specifications for each major domain process.
- Reference this ADR from workflow-bearing feature PRDs.
- Keep transition history append-only across modules.
