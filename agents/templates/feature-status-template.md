---
template: feature-status
version: 1.0
applies_to: product-manager
---

# Feature STATUS Template

Tracks completion progress for a feature. Place as `STATUS.md` inside each feature folder. Used to determine when a feature is complete and ready for archival.

---

# F{NNNN} — [Feature Name] — Status

**Overall Status:** [Draft | In Progress | Done | Archived]
**Last Updated:** [YYYY-MM-DD]

## Story Checklist

| Story | Title | Status |
|-------|-------|--------|
| F{NNNN}-S0001 | [Story title] | [ ] Not Started / [ ] In Progress / [x] Done |
| F{NNNN}-S0002 | [Story title] | [ ] Not Started / [ ] In Progress / [x] Done |

## Backend Progress

- [ ] Entities and EF configurations
- [ ] Repository implementations
- [ ] Service layer with business logic
- [ ] API endpoints (controllers / minimal API)
- [ ] Authorization policies
- [ ] Unit tests passing
- [ ] Integration tests passing

## Frontend Progress

- [ ] Page components created
- [ ] API hooks / data fetching
- [ ] Form validation
- [ ] Routing configured
- [ ] Responsive layout verified
- [ ] Visual regression tests (if applicable)

## Cross-Cutting

- [ ] Seed data (if applicable)
- [ ] Migration(s) applied
- [ ] API documentation updated
- [ ] No TODOs remain in code

## Deferred Non-Blocking Follow-ups (Optional)

Use this section only when the feature is still `Done` and deferred items are explicitly non-blocking.

| Follow-up | Why deferred | Tracking link | Owner |
|-----------|--------------|---------------|-------|
| [Item] | [Rationale] | [Issue/Story/Doc] | [Role/Name] |

## Tracker Sync Checklist

- [ ] `planning-mds/features/REGISTRY.md` status/path aligned
- [ ] `planning-mds/features/ROADMAP.md` section aligned (`Now/Next/Later/Completed`)
- [ ] `planning-mds/features/STORY-INDEX.md` regenerated
- [ ] `planning-mds/BLUEPRINT.md` feature/story status links aligned

## Archival Criteria

All items above must be checked before moving this feature folder to `planning-mds/features/archive/`.
