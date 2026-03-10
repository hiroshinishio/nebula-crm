# Tracker Governance Contract

This document defines how planning trackers stay current and trustworthy.

## Why This Exists

- `REGISTRY.md`, `ROADMAP.md`, `STORY-INDEX.md`, `BLUEPRINT.md`, and per-feature `STATUS.md` are operational controls, not optional docs.
- Any feature/story state change must update tracker state in the same change set.

## Authoritative Tracker Roles

- `planning-mds/features/REGISTRY.md`: authoritative feature inventory, status, and folder paths.
- `planning-mds/features/ROADMAP.md`: authoritative sequencing view (`Now / Next / Later / Completed`).
- `planning-mds/features/STORY-INDEX.md`: auto-generated story rollup from strict story filenames.
- `planning-mds/features/F{NNNN}-{slug}/STATUS.md`: authoritative feature execution state and deferred follow-ups.
- `planning-mds/BLUEPRINT.md`: baseline strategy snapshot; must not contradict tracker state.

## Ownership

- Product Manager: updates tracker docs during planning changes.
- Architect: validates tracker consistency at planning-to-build handoff.
- Implementers: update feature `STATUS.md` whenever story state changes.
- Code Reviewer: blocks approval when tracker drift is detected.

## Lifecycle Rules

- Feature lifecycle states: `Draft` -> `In Progress` -> `Done` -> `Archived`.
- `Done` may include a `Deferred Non-Blocking Follow-ups` section in `STATUS.md`; deferments must not change overall completion state.
- Archived features must:
  - live under `planning-mds/features/archive/`
  - be listed under `Archived Features` in `REGISTRY.md`
  - appear in `ROADMAP.md` `Completed` section, not `Now/Next/Later`.

## Story File Rules

- Story files must follow `F{NNNN}-S{NNNN}-{slug}.md`.
- Non-story documents in feature folders must NOT start with `F{NNNN}-S{NNNN}`.
- Story IDs in file content must match filename prefix.

## Mandatory Sync Triggers

Update trackers immediately when any of the following occurs:

1. A feature is created, renamed, moved, or archived.
2. A story is added, removed, renamed, or moved.
3. A feature/story status changes (including done/archive transitions).
4. Roadmap prioritization or sequencing changes.
5. Blueprint feature/story status text changes.

## Required Validation Commands

Run these before declaring planning or feature execution complete:

```bash
python3 agents/product-manager/scripts/validate-stories.py planning-mds/features/F{NNNN}-{slug}/
python3 agents/product-manager/scripts/generate-story-index.py planning-mds/features/
python3 agents/product-manager/scripts/validate-trackers.py
```

## Definition of Fresh Trackers

All conditions must pass:

- [ ] Every `REGISTRY.md` folder path exists and points to the correct active/archive location.
- [ ] `ROADMAP.md` links resolve and align with current feature state.
- [ ] `STORY-INDEX.md` story count and links match current strict story files.
- [ ] `BLUEPRINT.md` linked feature/story paths resolve and match archive status.
- [ ] No non-story file is parsed as a story.
