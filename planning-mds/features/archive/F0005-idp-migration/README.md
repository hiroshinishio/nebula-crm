# F0005 — IdP Migration: Keycloak → authentik

**Status:** Draft | **Phase:** Infrastructure Patch | **Priority:** Must-complete before backend implementation

## What Is This?

This feature replaces Keycloak with **authentik** as the OIDC identity provider and introduces an IdP-agnostic **internal `UserId`** layer in the data model. It is a pre-production architectural patch applied before the backend engine is implemented.

## Why Now?

- No production data exists, making this the zero-cost migration window.
- Setting the `UserId` principal key pattern now prevents all future IdP-subject coupling in entity fields.
- authentik's simpler admin UX and first-class Blueprint support reduce ongoing ops burden.

## Key Documents

| Document | Purpose |
|----------|---------|
| [ADR-006](../../architecture/decisions/ADR-006-authentik-idp-migration.md) | Full architectural decision and design |
| [PRD.md](PRD.md) | Goals, non-goals, acceptance criteria |
| [STATUS.md](STATUS.md) | Current implementation state |
| [GETTING-STARTED.md](GETTING-STARTED.md) | How to run and test the migration |

## Stories

| ID | Title | Status |
|----|-------|--------|
| F0005-S0001 | Replace authentik infrastructure | Draft |
| F0005-S0002 | Claims normalization + principal key (backend) | Draft |
| F0005-S0003 | Frontend OIDC flow update | Draft |
| F0005-S0004 | Data model principal key rename | Draft |

## Impact Summary

| Area | Change |
|------|--------|
| `docker-compose.yml` | Remove `keycloak`; add `authentik-server`, `authentik-worker`, `redis` |
| `BLUEPRINT.md` | Tech stack: Keycloak → authentik; data model principal fields renamed |
| `ADR-Auth-Strategy.md` | Superseded by ADR-006 |
| `SOLUTION-PATTERNS.md` | Auth pattern, env var examples, UserProfile sync updated |
| `data-model.md` | All `*Subject` / `AssignedTo` fields renamed to `*UserId (uuid)` |
| Backend | New `IClaimsPrincipalNormalizer`; `UserProfile` table restructured |
| Frontend | Replace `keycloak-js` with `oidc-client-ts`; update OIDC URLs |
| Casbin | **No changes** — model.conf and policy.csv unchanged |
