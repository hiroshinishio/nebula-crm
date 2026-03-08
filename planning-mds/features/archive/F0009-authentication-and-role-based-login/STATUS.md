# F0009 — Authentication + Role-Based Login — Status

**Overall Status:** Done
**Last Updated:** 2026-03-07

## Story Checklist

| Story | Title | Status | Notes |
|-------|-------|--------|-------|
| F0009-S0001 | Login Screen and OIDC Redirect | Done | LoginPage, ProtectedRoute, Vite build guard, CI gate, env templates, tests |
| F0009-S0002 | OIDC Callback and Session Bootstrap | Done | AuthCallbackPage, useSessionTeardown, POST /auth/logout backend endpoint, tests |
| F0009-S0003 | Role-Based Entry and Protected Navigation | Done | Role-based landing (AuthCallbackPage), authEvents bus, useAuthEventHandler, UnauthorizedPage with reason param, api.ts 401/403 handling |
| F0009-S0004 | BrokerUser Access Boundaries | Done | BrokerScopeResolver, BrokerScopeUnresolvableException, tenant isolation + field filtering in all 5 services, BrokerUser DTOs, audit logging, integration tests |
| F0009-S0005 | Seeded User Access Validation Matrix | Done | All three dev identities provisioned in authentik blueprint; broker_tenant_id linked in DevSeedData; integration tests cover BrokerUser boundary checks |

## Prerequisite Tracking

| Prerequisite | Status | Notes |
|-------------|--------|-------|
| F0005 IdP migration remains stable | Ready | authentik stack and claim normalization available |
| BrokerUser role added to authorization matrix | Ready | matrix section 2.10 exists |
| BrokerUser role added to policy.csv | Ready (artifact updated) | query-layer scope + field filtering implemented in code |
| Broker-visible data boundary list approved | Ready | `BROKER-VISIBILITY-MATRIX.md` added |
| Real-login frontend mode available (without `dev-auth.ts`) | Ready | VITE_AUTH_MODE=oidc is default; dev-auth.ts gated behind explicit VITE_AUTH_MODE=dev flag |
| Required authentik seeded identities provisioned | Ready | All three dev identities in blueprint: lisa.wong (DistributionUser), john.miller (Underwriter), broker001 (BrokerUser) |
| BrokerUser `broker_tenant_id` claim contract implemented | Ready | Scope mapping in blueprint, BrokerScopeResolver in engine, broker_tenant_id linked in DevSeedData |

## Release-Blocking Requirement Gaps

None. All prerequisite gaps are resolved.

## Coordination Notes

- F0001/F0002/F0005 status docs now explicitly mark auth/scope/runtime enforcement items as deferred to F0009.
- No cross-feature status entry should treat role/tenant enforcement as complete until F0009 stories are implemented and validated.

## Architecture-Ready Artifacts

- [x] PRD with mandatory "How" decisions
- [x] `IMPLEMENTATION-CONTRACT.md`
- [x] `BROKER-VISIBILITY-MATRIX.md`
- [x] `planning-mds/security/F0009-security-review-checklist.md`
- [x] Story-level deterministic acceptance criteria
