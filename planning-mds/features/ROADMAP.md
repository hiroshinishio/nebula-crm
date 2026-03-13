# Feature Roadmap (Now / Next / Later)

This document is the working prioritization view for feature sequencing.

## Purpose

- Provide a current planning dashboard for sequencing decisions.
- Keep prioritization separate from feature metadata in `REGISTRY.md`.
- Avoid constant churn in `BLUEPRINT.md`, which should remain baseline strategy.

## Update Rules

- Update whenever feature priority or sequence changes.
- Keep entries at feature level (not story-level unless explicitly needed).
- Link each item to a feature folder and include rationale.

## Now

| Feature | Phase | Why Now |
|---------|-------|---------|
| [F0011 — Dashboard Opportunities Flow-First Modernization (Connected Pipeline + Terminal Outcomes)](./F0011-dashboard-opportunities-flow-modernization/README.md) | MVP | Direct follow-up to F0010: opportunities still need a connected flow and explicit terminal outcomes to reduce triage friction. |

## Next

| Feature | Phase | Why Next |
|---------|-------|----------|
| [F0003 — Task Center + Reminders (API-only MVP)](./F0003-task-center/README.md) | MVP | Backend task lifecycle completion strengthens dashboard/task integrity. |
| [F0004 — Task Center UI + Manager Assignment](./F0004-task-center-ui-and-assignment/README.md) | Phase 1 | UI rollout can follow once F0003 task API scope is implemented and stabilized. |

## Later

| Feature | Phase | Why Later |
|---------|-------|-----------|
| F0006 — Submission Intake Workflow | MVP (planned) | Depends on broader workflow hardening and prioritization against authentication and task completion work. |
| F0007 — Renewal Pipeline | MVP (planned) | Follows submission workflow foundation and shared transition patterns. |
| F0008 — Broker Insights | MVP (planned) | Higher-value once core workflow and auth foundations are stable. |

## Completed

| Feature | Phase | Completion State |
|---------|-------|------------------|
| [F0001 — Dashboard](./archive/F0001-dashboard/README.md) | MVP | Done and archived |
| [F0002 — Broker & MGA Relationship Management](./archive/F0002-broker-relationship-management/README.md) | MVP | Done and archived (post-MVP hardening follow-ups tracked) |
| [F0005 — IdP Migration: Keycloak → authentik](./archive/F0005-idp-migration/README.md) | Foundation | Done and archived |
| [F0009 — Authentication + Role-Based Login](./archive/F0009-authentication-and-role-based-login/README.md) | Phase 1 | Done and archived |
| [F0010 — Dashboard Opportunities Refactor (Pipeline Board + Insight Views)](./F0010-dashboard-opportunities-refactor/README.md) | MVP | Done |

## Notes

- This roadmap is the authoritative Now/Next/Later view.
- `REGISTRY.md` remains the authoritative feature inventory and ID tracker.
- `BLUEPRINT.md` remains the baseline product/architecture source of truth.
- Tracker sync policy is defined in `TRACKER-GOVERNANCE.md`.
- Archived features are tracked under `planning-mds/features/archive/`.
