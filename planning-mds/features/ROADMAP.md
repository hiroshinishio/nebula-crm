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
| [F0004 — Task Center UI + Manager Assignment](./F0004-task-center-ui-and-assignment/README.md) | Phase 1 | UI rollout follows now that F0003 task API scope is implemented and stabilized. |
| [F0014 — DevOps Smoke Test Automation](./F0014-devops-smoke-test-automation/README.md) | Infrastructure | Reduces DevOps verification friction — blueprint fixes and automation scripts. |

## Next

| Feature | Phase | Why Next |
|---------|-------|----------|
| F0006 — Submission Intake Workflow | MVP (planned) | Core workflow foundation needed before renewal and broker insights. |

## Later

| Feature | Phase | Why Later |
|---------|-------|-----------|
| F0007 — Renewal Pipeline | MVP (planned) | Follows submission workflow foundation and shared transition patterns. |
| F0008 — Broker Insights | MVP (planned) | Higher-value once core workflow and auth foundations are stable. |

## Abandoned

| Feature | Superseded By | Rationale |
|---------|---------------|-----------|
| [F0010 — Dashboard Opportunities Refactor](./archive/F0010-dashboard-opportunities-refactor/README.md) | F0013 | F0010's Pipeline Board, Heatmap, Treemap, and Sunburst views are replaced by F0013's vertical timeline with contextual mini-visualizations. The insight views no longer fit the storytelling canvas direction. |
| [F0011 — Dashboard Opportunities Flow-First Modernization](./archive/F0011-dashboard-opportunities-flow-modernization/README.md) | F0013 | F0011's connected flow and terminal outcomes concepts live on in F0013 but with a fundamentally different visual approach (vertical timeline + narrative callouts instead of connected flow cells). |

## Completed

| Feature | Phase | Completion State |
|---------|-------|------------------|
| [F0003 — Task Center + Reminders (API-only MVP)](./archive/F0003-task-center/README.md) | MVP | Done and archived |
| [F0013 — Dashboard Framed Storytelling Canvas](./archive/F0013-dashboard-framed-storytelling-canvas/README.md) | MVP | Done and archived |
| [F0001 — Dashboard](./archive/F0001-dashboard/README.md) | MVP | Done and archived |
| [F0002 — Broker & MGA Relationship Management](./archive/F0002-broker-relationship-management/README.md) | MVP | Done and archived (post-MVP hardening follow-ups tracked) |
| [F0005 — IdP Migration: Keycloak → authentik](./archive/F0005-idp-migration/README.md) | Foundation | Done and archived |
| [F0009 — Authentication + Role-Based Login](./archive/F0009-authentication-and-role-based-login/README.md) | Phase 1 | Done and archived |
| [F0012 — Dashboard Storytelling Infographic Canvas (Flat Canvas + Collapsible Rails)](./archive/F0012-dashboard-storytelling-infographic-canvas/README.md) | MVP | Done and archived |

## Notes

- This roadmap is the authoritative Now/Next/Later view.
- `REGISTRY.md` remains the authoritative feature inventory and ID tracker.
- `BLUEPRINT.md` remains the baseline product/architecture source of truth.
- Tracker sync policy is defined in `TRACKER-GOVERNANCE.md`.
- Archived features are tracked under `planning-mds/features/archive/`.
