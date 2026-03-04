# F0009 — Implementation Contract (Architecture Refinement)

Purpose: define the mandatory "How" decisions for F0009 so implementation agents can build with no requirement ambiguity.

## 1. Authentication Flow Contract

- OIDC flow: Authorization Code + PKCE.
- Frontend OIDC client: `oidc-client-ts`.
- Required frontend routes:
  - `/login`
  - `/auth/callback`
  - `/unauthorized`
- Callback route validation is fail-closed:
  - missing/invalid `state` or callback errors -> clear transient auth state and redirect to `/login?error=callback_failed`.
- `dev-auth.ts` may remain only behind explicit fallback flag `VITE_AUTH_MODE=dev` for local transition.
- Required behavior for `VITE_AUTH_MODE=oidc`:
  - frontend must not call `getDevToken()`.

## 2. Session Lifecycle Contract

- Session source of truth: active OIDC user from `oidc-client-ts` and unexpired access token.
- On app bootstrap:
  - if valid session exists -> continue.
  - if no valid session -> redirect to `/login`.
- Silent renew is out of scope for F0009 Phase 1.
- Expired session behavior:
  - route access -> redirect `/login?reason=session_expired`.
  - clear in-memory session and any persisted OIDC user state.

## 3. Route Guard and Navigation Contract

- Protected routes require both:
  - authenticated session
  - role/resource authorization
- Deterministic unauthorized behavior:
  - route-level deny -> navigate to `/unauthorized`.
  - API-level `403` -> keep context and show permission-safe state with `traceId` when available.
  - API-level `401` -> clear session and redirect to `/login`.
- Role precedence for multi-role users:
  - `Admin` > `DistributionManager` > `DistributionUser` > `Underwriter` > `BrokerUser`.

## 4. Landing Route Contract

- `DistributionUser` -> `/`
- `Underwriter` -> `/`
- `BrokerUser` -> `/brokers`

Notes:
- Underwriter-specific dashboard route is deferred; Underwriter uses dashboard at `/` with API-level authorization.
- BrokerUser landing uses existing broker list surface with broker-scoped data filtering.

## 5. Claims and Identity Contract

- Required claims:
  - `sub`
  - `iss`
  - `email`
  - `nebula_roles` (one or more)
- Role resolution input is only `nebula_roles`.
- If `nebula_roles` is missing, malformed, or unsupported:
  - sign-in may complete at IdP
  - Nebula must treat user as unauthorized and route to `/unauthorized`.

## 6. Broker Scope Resolution Contract

- BrokerUser scope anchor: authenticated user's `email` claim.
- Resolution rule:
  - find exactly one active broker where `Broker.Email == user.email` (case-insensitive).
  - zero matches -> deny all broker-protected resources.
  - more than one match -> deny all broker-protected resources.
- Default deny when scope linkage cannot be resolved.

## 7. Authorization Policy Contract

- `planning-mds/security/authorization-matrix.md` section 2.10 is authoritative for BrokerUser scope.
- `planning-mds/security/policies/policy.csv` must be updated to include explicit BrokerUser policy lines for all allow/deny decisions in 2.10.
- Release is blocked if matrix and policy.csv are out of sync for BrokerUser resources/actions.

## 8. Broker Field Visibility Contract

- Field boundary list in `BROKER-VISIBILITY-MATRIX.md` is mandatory.
- Server-side response shaping is required for BrokerUser; UI hiding is defense-in-depth only.
- If an endpoint cannot guarantee field-level filtering for BrokerUser, that endpoint remains denied for BrokerUser in Phase 1.

## 9. Seed Identity Contract (Non-Production)

- Required test identities:
  - `lisa.wong@nebula.local` -> `DistributionUser`
  - `john.miller@nebula.local` -> `Underwriter`
  - `broker001@example.local` -> `BrokerUser`
- authentik blueprint/seeding must include:
  - `BrokerUser` group
  - user assignment for all three test identities
  - `nebula_roles` claim emission
- Seeding must be idempotent.

## 10. Test and Acceptance Contract

Minimum release validation must include:

1. Login redirect/callback happy path for each required test identity.
2. Session-expired redirect behavior and stale-state cleanup.
3. Route guard behavior (`401` and `403`) with deterministic UI outcomes.
4. Broker cross-scope denial tests (list + detail).
5. Broker field filtering tests (no `InternalOnly` fields in BrokerUser responses).
6. Matrix vs policy consistency check for BrokerUser actions.

## 11. Deferred Items (Explicitly Not in F0009)

- Silent token renewal.
- Cross-application single logout.
- Broker self-service submission/renewal workflows.
- Multi-role landing preference customization.
