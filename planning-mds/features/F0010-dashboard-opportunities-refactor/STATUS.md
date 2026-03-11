# F0010 — Dashboard Opportunities Refactor (Pipeline Board + Insight Views) — Status

**Overall Status:** Not Started
**Last Updated:** 2026-03-08

## Story Checklist

| Story | Title | Status |
|-------|-------|--------|
| F0010-S0001 | Replace Sankey default with Pipeline Board | [ ] Not Started / [ ] In Progress / [ ] Done |
| F0010-S0002 | Add Opportunities Aging Heatmap view | [ ] Not Started / [ ] In Progress / [ ] Done |
| F0010-S0003 | Add Opportunities Composition Treemap view | [ ] Not Started / [ ] In Progress / [ ] Done |
| F0010-S0004 | Add Opportunities Hierarchy Sunburst view | [ ] Not Started / [ ] In Progress / [ ] Done |
| F0010-S0005 | Unify drilldown, responsive layout, and accessibility | [ ] Not Started / [ ] In Progress / [ ] Done |

## Backend Progress

- [ ] Opportunities summary contract updated for Pipeline Board default data needs
- [ ] Opportunities insights contract defined for Heatmap/Treemap/Sunburst aggregates
- [ ] Authorization coverage validated for all opportunities endpoints
- [ ] Unit tests passing
- [ ] Integration tests passing

## Frontend Progress

- [ ] Opportunities widget layout updated to Pipeline Board default
- [ ] View mode toggle implemented (Pipeline, Heatmap, Treemap, Sunburst)
- [ ] Drilldown popovers wired consistently across views
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
| Quality Engineer | Yes | Baseline acceptance and regression coverage for dashboard view-mode workflows. | Architect | TBD |
| Code Reviewer | Yes | Baseline independent implementation review before completion/archive transition. | Architect | TBD |
| Security Reviewer | TBD | Set to Yes when authorization scope or sensitive exposure changes are introduced. | Architect | TBD |
| DevOps | TBD | Set to Yes when deploy/runtime/env-contract changes are introduced. | Architect | TBD |
| Architect | TBD | Set to Yes when architecture exceptions require explicit acceptance. | Architect | TBD |

## Story Signoff Provenance

| Story | Role | Reviewer | Verdict | Evidence | Date | Notes |
|-------|------|----------|---------|----------|------|-------|
| F0010-S0001 | Quality Engineer | - | N/A | - | - | Populate when story reaches review. |
| F0010-S0001 | Code Reviewer | - | N/A | - | - | Populate when story reaches review. |

## Archival Criteria

All items above must be checked before moving this feature folder to `planning-mds/features/archive/`.
