---
template: feature-registry
version: 1.0
applies_to: product-manager
---

# Feature Registry Template

Tracks all features by ID, name, and status. Place as `REGISTRY.md` at `planning-mds/features/REGISTRY.md`.

---

# Feature Registry

**Next Available Feature Number:** F{NNNN}

**Planning Views:**
- Roadmap sequencing (`Now / Next / Later`): `planning-mds/features/ROADMAP.md`
- Story rollup index: `planning-mds/features/STORY-INDEX.md`
- Governance contract: `planning-mds/features/TRACKER-GOVERNANCE.md`

## Active Features

| Feature ID | Name | Status | Phase | Folder |
|------------|------|--------|-------|--------|
| F0001 | [Feature name] | [Draft / In Progress / Complete] | [MVP / Phase 1 / ...] | `F0001-{slug}/` |
| F0002 | [Feature name] | [Draft / In Progress / Complete] | [MVP / Phase 1 / ...] | `F0002-{slug}/` |

## Archived Features

| Feature ID | Name | Archived Date | Folder |
|------------|------|---------------|--------|
| [F{NNNN}] | [Feature name] | [YYYY-MM-DD] | `archive/F{NNNN}-{slug}/` |

## Numbering Rules

- Feature IDs use a 4-digit zero-padded format: `F0001`, `F0002`, ..., `F9999`
- Numbers are assigned sequentially — never reuse a retired number
- Story IDs within a feature follow `F{NNNN}-S{NNNN}` (e.g., `F0001-S0001`)
- Update **Next Available Feature Number** whenever a new feature is added

## Sync Rules

- Update REGISTRY whenever a feature is created, renamed, re-scoped, marked done, or archived.
- Keep folder paths exact and valid (`F{NNNN}-{slug}/` for active, `archive/F{NNNN}-{slug}/` for archived).
- After registry edits, regenerate story index and run tracker validation.
