# F0005 — Status

**Last Updated:** 2026-03-05
**Overall Status:** Done (Foundation Complete; Runtime Enforcement Deferred to F0009)

## Stories

| Story | Status | Blocker |
|-------|--------|---------|
| F0005-S0001 — Replace authentik infrastructure | ✅ Done | — |
| F0005-S0002 — Claims normalization + principal key | ✅ Done | — |
| F0005-S0003 — Frontend OIDC flow | ⚠️ Foundation Done; runtime token wiring deferred to F0009 | F0009 owns `/login`, `/auth/callback`, and OIDC token-first API flow |
| F0005-S0004 — Data model principal key rename | ✅ Done | — |

## Architecture Gate

| Check | Status |
|-------|--------|
| ADR-006 written and accepted | ✅ Done |
| BLUEPRINT.md updated | ✅ Done |
| SOLUTION-PATTERNS.md updated | ✅ Done |
| data-model.md updated | ✅ Done |
| docker-compose.yml updated | ✅ Done (authentik-server + authentik-worker; Redis removed) |
| dev-auth.ts updated | ✅ Done |
| engine/ domain entities updated | ✅ Done (F0005-S0004) |
| engine/ EF configurations updated | ✅ Done |
| engine/ application services updated | ✅ Done |
| engine/ HttpCurrentUserService updated | ✅ Done (nebula_roles claim, UserId DB lookup) |
| engine/ appsettings.Development.json updated | ✅ Done |
| EF migration created | ✅ Done (20260301000000_F0005_IdpPrincipalRefactor) |
| AppDbContextModelSnapshot.cs regenerated | ⚠️ Pending — run `dotnet ef migrations add` to regenerate |
| authentik blueprint fully working | ✅ Done (2026-03-02) — fixed invalidation_flow + redirect_uris for authentik 2026.2 |

## Notes

F0005 is a prerequisite for all backend implementation. All F0001/F0002 backend stories (Phase C) must not begin until F0005-S0004 (principal key rename) is complete. The production auth/session behavior and BrokerUser boundary enforcement remain explicitly in F0009 scope.

**Blueprint root-cause (2026-03-02):** authentik 2026.2.0 added two required fields to `OAuth2Provider` that older blueprints omit: `invalidation_flow` (FK to an invalidation flow) and `redirect_uris` (now a list of `{url, matching_mode}` objects). The blueprint importer's `apply()` returns False silently in this case — the error only surfaces via `capture_logs()` inside the worker task.
