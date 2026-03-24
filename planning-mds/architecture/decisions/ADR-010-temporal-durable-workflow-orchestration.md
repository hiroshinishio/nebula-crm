# ADR-010: Adopt Temporal for Durable Long-Running CRM Workflows

**Status:** Proposed
**Date:** 2026-03-23
**Owners:** Architect
**Related Features:** F0007, F0019

## Context

Nebula's planned CRM workflows include long-running, time-based behavior that should survive deploys, retries, process restarts, and infrastructure interruptions. The clearest near-term example is the renewal pipeline, which needs scheduled reminders, escalation timing, and durable execution over long windows.

Ad hoc background jobs or cron-style scheduling would make workflow state, retries, and auditability harder to manage consistently as the platform grows.

## Decision

Adopt Temporal as the durable workflow orchestration engine for long-running CRM workflows that require timers, retries, workflow visibility, and external-event correlation.

Immediate use is expected in renewal reminders and escalations. Later submission or approval flows may reuse the same capability when durable waiting or external workflow signaling becomes necessary.

## Scope

This ADR governs:

- durable orchestration of long-running workflow steps
- timer-based reminders and escalations
- workflow correlation IDs stored on business records
- retry and observability expectations for Temporal-managed workflows

This ADR does not govern:

- immediate user-driven state transitions that remain in application services
- queue routing logic
- generic event delivery infrastructure

## Consequences

### Positive

- Durable timers and retries become first-class platform capabilities.
- Workflow execution state is visible and recoverable.
- Long-running CRM behavior is no longer coupled to process uptime.

### Negative

- Adds a new runtime dependency and operating surface.
- Requires worker deployment, monitoring, and Temporal-aware testing patterns.

## Follow-up

- Define workflow registration and worker-hosting conventions.
- Reference this ADR from renewal and other long-running workflow PRDs.
- Align `SOLUTION-PATTERNS.md` and runbooks with Temporal operating guidance.
