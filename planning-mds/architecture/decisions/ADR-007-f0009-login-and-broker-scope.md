# ADR-007: F0009 Login Flow and Broker Scope Enforcement

**Status:** Accepted  
**Date:** 2026-03-04  
**Owners:** Architect + Security

## Context

F0009 introduces real login and an external BrokerUser role. Existing MVP architecture assumed internal users and dev-token bootstrap. Implementation must be deterministic across routing, session handling, and authorization behavior while preventing broker cross-tenant exposure.

## Decision

1. **OIDC pattern**
   - Use Authorization Code + PKCE with `oidc-client-ts`.
   - Required frontend routes: `/login`, `/auth/callback`, `/unauthorized`.

2. **Deterministic auth outcomes**
   - Unauthenticated access -> `/login`
   - Unauthorized route access -> `/unauthorized`
   - API `401` -> clear session and redirect `/login`
   - API `403` -> stay in context and render permission-safe message

3. **BrokerUser scope resolution**
   - Resolve broker scope via authenticated `email` claim.
   - Match rule: exactly one active broker where `Broker.Email == email` (case-insensitive).
   - No match or ambiguous match -> deny by default.

4. **Policy and matrix parity**
   - `authorization-matrix.md` section 2.10 is normative.
   - `policy.csv` must carry explicit BrokerUser allow rows for those resources/actions.
   - Any mismatch is release-blocking.

5. **Field-level boundaries**
   - BrokerUser response filtering is server-side mandatory.
   - `InternalOnly` fields must never be emitted to BrokerUser.

## Architecture Sketch (ASCII)

```text
[User] -> [/login] -> [authentik] -> [/auth/callback] -> [Session OK]
   |                                              |
   |                                              v
   +----------------> [API request with token] -> [JWT validate]
                                                 -> [ABAC policy check]
                                                 -> [Broker scope resolve by email]
                                                 -> [Field filter by visibility class]
                                                 -> [200 | 401 | 403]
```

## Companion Sequence (Mermaid)

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant IDP as authentik
    participant API as Nebula API
    participant DB as Postgres

    FE->>IDP: auth code + PKCE
    IDP-->>FE: callback(code, state)
    FE->>FE: validate state, establish session
    FE->>API: request with bearer token
    API->>API: validate JWT + roles
    API->>DB: resolve broker scope by email
    API->>API: enforce ABAC + field filtering
    API-->>FE: 200 or 401/403 ProblemDetails
```

## Consequences

Positive:

- Predictable UX and test behavior for login failures and access denial.
- Strong default-deny posture for external BrokerUser.
- Explicit policy artifact parity requirement reduces authorization drift.

Tradeoffs:

- Broker scope currently depends on email linkage quality.
- Silent renew deferred, so long-lived sessions require explicit re-auth.

## Follow-up

- Implement seeded authentik BrokerUser identities and group mappings.
- Add policy parity validation to CI for matrix vs `policy.csv`.
- Revisit silent renew in future auth hardening story.

