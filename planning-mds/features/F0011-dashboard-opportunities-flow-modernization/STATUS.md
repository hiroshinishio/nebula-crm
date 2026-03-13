# F0011 — Dashboard Opportunities Flow-First Modernization (Connected Pipeline + Terminal Outcomes) — Status

**Overall Status:** Draft
**Last Updated:** 2026-03-12

## Story Checklist

| Story | Title | Status |
|-------|-------|--------|
| F0011-S0001 | Replace Pipeline Board tiles with connected flow-first canvas default | [ ] Not Started |
| F0011-S0002 | Add terminal outcomes rail and outcome drilldowns | [ ] Not Started |
| F0011-S0003 | Apply modern opportunities visual system (dark depth + stage emphasis) | [ ] Not Started |
| F0011-S0004 | Rebalance secondary insights as mini-views | [ ] Not Started |
| F0011-S0005 | Ensure responsive and accessibility parity for new opportunities flow | [ ] Not Started |

## Backend Progress

- [ ] Opportunities summary/flow contract updates for connected stage flow metadata
- [ ] Terminal outcomes aggregate contract defined
- [ ] Authorization coverage validated for opportunities endpoints
- [ ] Unit tests passing
- [ ] Integration tests passing

## Frontend Progress

- [ ] Connected flow-first canvas implemented as opportunities default
- [ ] Outcomes rail and drilldown interaction wired
- [ ] Secondary mini-views integrated without replacing primary flow
- [ ] Responsive layouts verified (MacBook, iPad, iPhone)
- [ ] Component tests passing
- [ ] Visual regression coverage updated

## Cross-Cutting

- [ ] API documentation updated
- [ ] Feature test plan executed
- [ ] Deployability check evidence recorded
- [ ] No TODOs remain in implementation code

## Required Signoff Roles (Set in Planning)

| Role | Required | Why Required | Set By | Date |
|------|----------|--------------|--------|------|
| Quality Engineer | Yes | Baseline acceptance and regression coverage for opportunities flow/outcomes workflows. | Architect | 2026-03-12 |
| Code Reviewer | Yes | Baseline independent implementation review before completion/archive transition. | Architect | 2026-03-12 |
| Security Reviewer | Yes | Updated opportunities aggregates and drilldown entry points require authorization and data-boundary verification. | Architect | 2026-03-12 |
| DevOps | No | No expected new infra services or env-var contracts for this slice. | Architect | 2026-03-12 |
| Architect | No | Standard patterns expected; no architecture exceptions requested at planning time. | Architect | 2026-03-12 |

## Story Signoff Provenance

| Story | Role | Reviewer | Verdict | Evidence | Date | Notes |
|-------|------|----------|---------|----------|------|-------|
| F0011-S0001 | Quality Engineer | - | N/A | - | - | Populate after implementation and test execution. |
| F0011-S0001 | Code Reviewer | - | N/A | - | - | Populate after implementation review. |
| F0011-S0001 | Security Reviewer | - | N/A | - | - | Populate after security review. |

## Deferred Non-Blocking Follow-ups (Optional)

| Follow-up | Why deferred | Tracking link | Owner |
|-----------|--------------|---------------|-------|
| *(none)* | | | |

## Tracker Sync Checklist

- [x] `planning-mds/features/REGISTRY.md` status/path aligned
- [x] `planning-mds/features/ROADMAP.md` section aligned (`Now/Next/Later/Completed`)
- [x] `planning-mds/features/STORY-INDEX.md` regenerated
- [x] `planning-mds/BLUEPRINT.md` feature/story status links aligned
- [ ] Every required signoff role has story-level `PASS` entries with reviewer, date, and evidence

## Archival Criteria

All items above must be checked before moving this feature folder to `planning-mds/features/archive/`.
