# F0015 — Frontend Quality Gates + Test Infrastructure — Getting Started

## Prerequisites

- [ ] `experience/` dependencies install cleanly in the chosen runtime path
- [ ] Container runtime is available for Playwright-aligned frontend validation
- [ ] Nebula app stack can be started when route-level validation requires API/auth integration
- [ ] Lifecycle stage remains aligned with the active implementation plan before gate activation

## Services to Run

```bash
# Local/frontend iteration
pnpm --dir experience install

# App/runtime verification when route-level validation is needed
docker compose up -d

# Frontend dev server
pnpm --dir experience dev
```

## Environment Variables

| Variable | Purpose | Default |
|----------|---------|---------|
| `VITE_AUTH_MODE` | Selects auth mode for frontend runtime validation | `oidc` |
| `NEBULA_API_PROXY_TARGET` | Vite proxy target for frontend routes | `http://localhost:5113` |
| `VITE_API_PROXY_TARGET` | Alternate frontend proxy override | `http://localhost:5113` |
| `VITE_OIDC_AUTHORITY` | OIDC authority for end-to-end auth validation | Nebula local IdP |
| `VITE_OIDC_CLIENT_ID` | OIDC client id for browser auth validation | `nebula` |
| `VITE_OIDC_REDIRECT_URI` | Redirect URI for browser auth validation | local frontend callback route |

## Seed Data

No new business seed data is required for feature planning. Implementation should reuse the current Nebula dev stack data needed to load dashboard, broker, and auth validation surfaces.

## How to Verify

1. Add or update the frontend scripts, shared harness, and lifecycle gate wiring defined by this feature.
2. Run the frontend validation flow in the approved runtime path, including component, integration, accessibility, coverage, and visual checks.
3. Generate and capture solution evidence for the full run under `planning-mds/operations/evidence/`.
4. Re-run `python3 agents/scripts/run-lifecycle-gates.py` after the Nebula gate wiring is updated.

## Key Files

| Layer | Path | Purpose |
|-------|------|---------|
| Planning | `planning-mds/features/F0015-frontend-quality-gates-and-test-infrastructure/` | Feature scope, stories, and status tracking |
| Planning | `planning-mds/BLUEPRINT.md` | Baseline feature inventory and testing expectations |
| Planning | `planning-mds/architecture/TESTING-STRATEGY.md` | Target frontend testing model to implement |
| Runtime | `lifecycle-stage.yaml` | Nebula lifecycle gate activation point |
| Frontend | `experience/package.json` | Frontend validation commands |
| Frontend | `experience/vite.config.ts` | Vitest configuration and coverage setup |
| Frontend | `experience/src/test-setup.ts` | Shared frontend test setup |
| Evidence | `planning-mds/operations/evidence/` | Required validation evidence output |

## Notes

- This feature is solution-specific. Any generic role/template/action changes under `agents/**` should ship separately.
- Existing host-side dependency issues on this Windows-mounted workspace are a known constraint; implementation should keep a containerized frontend validation path first-class.
