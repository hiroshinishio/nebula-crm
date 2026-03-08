# Feature Registry

**Next Available Feature Number:** F0010

**Planning Views:**
- Roadmap sequencing (`Now / Next / Later`): `planning-mds/features/ROADMAP.md`
- Story rollup index: `planning-mds/features/STORY-INDEX.md`

## Active Features

| Feature ID | Name | Status | Phase | Folder |
|------------|------|--------|-------|--------|
| F0001 | Dashboard | In Progress | MVP | `F0001-dashboard/` |
| F0002 | Broker & MGA Relationship Management | In Progress | MVP | `F0002-broker-relationship-management/` |
| F0003 | Task Center + Reminders (API-only MVP) | Draft | MVP | `F0003-task-center/` |
| F0004 | Task Center UI + Manager Assignment | Draft | Phase 1 | `F0004-task-center-ui-and-assignment/` |
| F0009 | Authentication + Role-Based Login | Draft | Phase 1 | `F0009-authentication-and-role-based-login/` |

## Planned (Reserved IDs)

| Feature ID | Name | Status | Phase | Folder |
|------------|------|--------|-------|--------|
| F0006 | Submission Intake Workflow | Planned | MVP | `TBD` |
| F0007 | Renewal Pipeline | Planned | MVP | `TBD` |
| F0008 | Broker Insights | Planned | MVP | `TBD` |

## Archived Features

| Feature ID | Name | Archived Date | Folder |
|------------|------|---------------|--------|
| F0005 | IdP Migration: Keycloak → authentik | 2026-03-07 | `archive/F0005-idp-migration/` |

## Numbering Rules

- Feature IDs use a 4-digit zero-padded format: `F0001`, `F0002`, ..., `F9999`
- Numbers are assigned sequentially — never reuse a retired number
- Story IDs within a feature follow `F{NNNN}-S{NNNN}` (e.g., `F0001-S0001`)
- Update **Next Available Feature Number** whenever a new feature is added

## Legacy Mapping

| Legacy ID | New ID |
|-----------|--------|
| F0001 | F0001 |
| F0002 | F0002 |
| F0003 | F0003 |
| F0004 | F0004 |
