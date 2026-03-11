# Frontend UX Evidence

Use this directory for UX audit evidence files tied to frontend UI changes.

## Required when

- Any user-facing UI files change under `experience/src/` (`.tsx/.jsx/.css`), or
- `experience/index.html` changes.

## File naming

- `ux-audit-YYYY-MM-DD.md`

## Required contents

- Command evidence (checked):
  - `pnpm --dir experience lint`
  - `pnpm --dir experience lint:theme`
  - `pnpm --dir experience build`
  - `pnpm --dir experience test`
  - `pnpm --dir experience test:visual:theme`
  - If a command is unavailable, include:
    - `Command unavailable: \`<command>\``
    - `Equivalent command used for \`<command>\`: \`<replacement>\``
- Light and dark screenshot evidence
- UX checklist evidence from `agents/frontend-developer/references/ux-audit-ruleset.md`
- Deferred items (if any) with owner and due date

Start from:

- `planning-mds/operations/evidence/frontend-ux/TEMPLATE.md`

CI validator:

- `agents/frontend-developer/scripts/validate-frontend-ux-evidence.py`
