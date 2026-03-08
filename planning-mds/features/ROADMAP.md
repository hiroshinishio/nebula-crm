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
| [F0001 — Dashboard](./F0001-dashboard/README.md) | MVP | Core landing workflows for internal users are actively in progress. |
| [F0002 — Broker & MGA Relationship Management](./F0002-broker-relationship-management/README.md) | MVP | Core relationship system-of-record capability is actively in progress. |

## Next

| Feature | Phase | Why Next |
|---------|-------|----------|
| [F0009 — Authentication + Role-Based Login](./F0009-authentication-and-role-based-login/README.md) | Phase 1 | Closes explicit login gap and enables role-realistic acceptance testing, including BrokerUser pilot access. |
| [F0003 — Task Center + Reminders (API-only MVP)](./F0003-task-center/README.md) | MVP | Backend task lifecycle completion strengthens dashboard/task integrity. |

## Later

| Feature | Phase | Why Later |
|---------|-------|-----------|
| F0006 — Submission Intake Workflow | MVP (planned) | Depends on broader workflow hardening and prioritization against authentication and task completion work. |
| F0007 — Renewal Pipeline | MVP (planned) | Follows submission workflow foundation and shared transition patterns. |
| F0008 — Broker Insights | MVP (planned) | Higher-value once core workflow and auth foundations are stable. |
| [F0004 — Task Center UI + Manager Assignment](./F0004-task-center-ui-and-assignment/README.md) | Phase 1 | UI expansion should follow task API and role/login stabilization. |

## Notes

- This roadmap is the authoritative Now/Next/Later view.
- `REGISTRY.md` remains the authoritative feature inventory and ID tracker.
- `BLUEPRINT.md` remains the baseline product/architecture source of truth.
- Archived features are tracked under `planning-mds/features/archive/` (e.g., [F0005 — IdP Migration](./archive/F0005-idp-migration/README.md)).
