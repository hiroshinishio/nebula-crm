---
template: feature-status
version: 1.0
applies_to: product-manager
---

# Feature STATUS Template

Tracks completion progress for a feature. Place as `STATUS.md` inside each feature folder. Used to determine when a feature is complete and ready for archival.

Completion has two distinct checkpoints:
- `Implementation Done`: implementers completed scope and tests.
- `Approved for Archive`: required reviewers signed off with evidence.

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
- [ ] Component/integration tests added or updated for changed behavior
- [ ] Accessibility validation recorded (if frontend in scope)
- [ ] Coverage artifact recorded (if coverage is part of project validation)
- [ ] Responsive layout verified
- [ ] Visual regression tests (if applicable)

## Cross-Cutting

- [ ] Seed data (if applicable)
- [ ] Migration(s) applied
- [ ] API documentation updated
- [ ] Runtime validation evidence recorded
- [ ] No TODOs remain in code

## Required Signoff Roles (Set in Planning)

Architect sets this matrix during feature planning. Mark only truly required roles as `Yes`.

| Role | Required | Why Required | Set By | Date |
|------|----------|--------------|--------|------|
| Quality Engineer | Yes | [Acceptance criteria and test coverage validation] | Architect | [YYYY-MM-DD] |
| Code Reviewer | Yes | [Independent code quality and regression review] | Architect | [YYYY-MM-DD] |
| Security Reviewer | No | [Set Yes when authn/authz/data-boundary/security-sensitive scope exists] | Architect | [YYYY-MM-DD] |
| DevOps | No | [Set Yes when deploy/runtime/env-contract changes are in scope] | Architect | [YYYY-MM-DD] |
| Architect | No | [Set Yes when architecture-risk exceptions require explicit approval] | Architect | [YYYY-MM-DD] |

## Story Signoff Provenance

Complete this before moving `Overall Status` to `Done`/`Archived`.
Every story in scope must have passing evidence for every role marked `Required = Yes`.
`Evidence` must reference solution artifacts, not `agents/**` guidance files.

| Story | Role | Reviewer | Verdict | Evidence | Date | Notes |
|-------|------|----------|---------|----------|------|-------|
| F{NNNN}-S0001 | Quality Engineer | [Name/Agent] | [PASS | FAIL] | [file path(s) or report path(s)] | [YYYY-MM-DD] | [optional] |
| F{NNNN}-S0001 | Code Reviewer | [Name/Agent] | [PASS | FAIL] | [file path(s) or report path(s)] | [YYYY-MM-DD] | [optional] |
| F{NNNN}-S0001 | Security Reviewer | [Name/Agent] | [PASS | FAIL | N/A] | [file path(s) or report path(s)] | [YYYY-MM-DD] | [optional] |
| F{NNNN}-S0002 | Quality Engineer | [Name/Agent] | [PASS | FAIL] | [file path(s) or report path(s)] | [YYYY-MM-DD] | [optional] |
| F{NNNN}-S0002 | Code Reviewer | [Name/Agent] | [PASS | FAIL] | [file path(s) or report path(s)] | [YYYY-MM-DD] | [optional] |

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
- [ ] Every required signoff role has story-level `PASS` entries with reviewer, date, and evidence

## Archival Criteria

All items above must be checked before moving this feature folder to `planning-mds/features/archive/`.
