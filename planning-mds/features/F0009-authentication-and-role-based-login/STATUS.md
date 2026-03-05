# F0009 — Authentication + Role-Based Login — Status

**Overall Status:** Draft (Requirements Refined)
**Last Updated:** 2026-03-04

## Story Checklist

| Story | Title | Status | Notes |
|-------|-------|--------|-------|
| F0009-S0001 | Login Screen and OIDC Redirect | Refined | Implementation contract added |
| F0009-S0002 | OIDC Callback and Session Bootstrap | Refined | Session lifecycle decisions resolved |
| F0009-S0003 | Role-Based Entry and Protected Navigation | Refined | Route/permission contract made deterministic |
| F0009-S0004 | BrokerUser Access Boundaries | Refined | Scope + field boundary contracts defined |
| F0009-S0005 | Seeded User Access Validation Matrix | Refined | Provisioning/test matrix contract defined |

## Prerequisite Tracking

| Prerequisite | Status | Notes |
|-------------|--------|-------|
| F0005 IdP migration remains stable | Ready | authentik stack and claim normalization available |
| BrokerUser role added to authorization matrix | Ready | matrix section 2.10 exists |
| BrokerUser role added to policy.csv | Ready (artifact updated) | implementation must enforce query-layer scope + field filtering |
| Broker-visible data boundary list approved | Ready | `BROKER-VISIBILITY-MATRIX.md` added |
| Real-login frontend mode available (without `dev-auth.ts`) | Not Started | implementation required |
| Required authentik seeded identities provisioned | Not Started | include `BrokerUser` group + user mappings |
| BrokerUser `broker_tenant_id` claim contract implemented | Not Started | required for stable tenant isolation |

## Release-Blocking Requirement Gaps

1. authentik blueprint missing `BrokerUser` group and required seeded identities.
2. frontend still dependent on `dev-auth.ts` in default flow.
3. backend query-layer scope + field filtering for BrokerUser not yet implemented in code.
4. BrokerUser `broker_tenant_id` claim mapping + tenant resolution not yet implemented in code.

## Architecture-Ready Artifacts

- [x] PRD with mandatory "How" decisions
- [x] `IMPLEMENTATION-CONTRACT.md`
- [x] `BROKER-VISIBILITY-MATRIX.md`
- [x] `planning-mds/security/F0009-security-review-checklist.md`
- [x] Story-level deterministic acceptance criteria
