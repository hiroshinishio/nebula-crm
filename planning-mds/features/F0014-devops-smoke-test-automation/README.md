# F0014 — DevOps Smoke Test Automation

**Status:** In Progress
**Phase:** Infrastructure
**Owner:** DevOps Agent

## Problem

DevOps verification of new features is high-friction. During F0003 verification, the following
issues caused significant debugging time:

1. **authentik blueprint gap**: The OAuth2Provider was missing `authentication_flow`, causing
   ROPC (password grant) to silently return `invalid_grant` even with valid credentials.
   This was an F0005 oversight that went undetected because there was no automated
   token-acquisition test in the deployment pipeline.

2. **authentik 2026.2 ROPC requires app-password tokens**: The password grant does NOT accept
   the user's login password — it requires a `Token` with `intent=app_password`. Blueprint
   had no token entries, so every DevOps run required manual `ak shell` intervention.

3. **No automated smoke test**: Each DevOps verification required hand-crafting curl commands,
   manually tracking task IDs across calls, and visually inspecting JSON responses.

4. **No "clean reset" workflow**: Verifying that blueprints and migrations apply cleanly on a
   fresh database required remembering the exact `docker compose down -v && up --build`
   sequence and health-check polling.

## Solution

### Blueprint Fixes (`docker/authentik/blueprints/nebula-dev.yaml`)

- Added `authentication_flow` to `OAuth2Provider` (required for ROPC grant)
- Added `authentik_core.token` entries for all dev users with `intent: app_password`
  and a shared dev key (`nebula-dev-token`)

### Automation Scripts

| Script | Purpose |
|---|---|
| `scripts/smoke-test.sh` | Automated 9-test API smoke suite (auth, CRUD, transitions, timeline) |
| `scripts/dev-reset.sh` | Clean teardown → rebuild → health wait → smoke test in one command |

### Usage

```bash
# Full clean verification (what DevOps agent should run)
./scripts/dev-reset.sh

# Just smoke test against running stack
./scripts/smoke-test.sh

# Test as different user
./scripts/smoke-test.sh --user john.miller

# Reset without smoke test
./scripts/dev-reset.sh --skip-smoke
```

### Dev User Credentials (ROPC)

All dev users share the same app-password token for simplicity:

| Username | Role | Password (token key) |
|---|---|---|
| lisa.wong | DistributionUser | `nebula-dev-token` |
| john.miller | Underwriter | `nebula-dev-token` |
| broker001 | BrokerUser | `nebula-dev-token` |
| akadmin | Admin | `nebula-dev-token` |

Token request:
```bash
curl -X POST http://localhost:9000/application/o/token/ \
  -d "grant_type=password&client_id=nebula&username=lisa.wong&password=nebula-dev-token&scope=openid profile email nebula_roles"
```

## Smoke Tests Covered

1. `GET /my/tasks` — auth + read
2. `POST /tasks` — create with self-assignment
3. `GET /tasks/{id}` — read by ID
4. `PUT /tasks/{id}` — Open → InProgress
5. `PUT /tasks/{id}` — InProgress → Done (completedAt set)
6. `PUT /tasks/{id}` — Open → Done (invalid transition → 409)
7. `DELETE /tasks/{id}` — soft delete → 204
8. `GET /tasks/{id}` — deleted → 404
9. Timeline events — verify 4 events recorded in DB

## Files Changed

- `docker/authentik/blueprints/nebula-dev.yaml` — blueprint fixes
- `scripts/smoke-test.sh` — new
- `scripts/dev-reset.sh` — new
- `planning-mds/features/REGISTRY.md` — feature registration
