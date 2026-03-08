# Feature Assembly Plan (F0001 + F0002 + F0009)

**Owner:** Architect
**Status:** Approved
**Last Updated:** 2026-02-21

## Goal

Define the build order, role handoffs, and integration checkpoints for F0001 (Dashboard), F0002 (Broker Relationship Management), and F0009 (Authentication + Role-Based Login).

---

## F0001 — Dashboard

### Dependencies
- Dashboard endpoints (`/dashboard/*`, `/my/tasks`, `/timeline/events`)
- Task entity + indexes (`planning-mds/architecture/data-model.md`)
- Timeline event query support (ActivityTimelineEvent)
- ABAC enforcement for dashboard queries

### Backend Assembly Steps
1. Implement Task entity + repository (Tasks table, indexes per data-model.md).
2. Implement ActivityTimelineEvent read query with ABAC scoping.
3. Implement dashboard aggregation endpoints:
   - `/dashboard/kpis`
   - `/dashboard/pipeline`
   - `/dashboard/pipeline/{entityType}/{status}/items`
   - `/dashboard/nudges`
   - `/my/tasks`
   - `/timeline/events`
4. Enforce request/response schema validation for dashboard payloads.

### Frontend Assembly Steps
1. Build Dashboard shell and five widgets (KPI, Pipeline, Tasks, Activity Feed, Nudges).
2. Integrate API calls and empty/error states per stories.
3. Ensure role‑aware rendering and degrade gracefully on unavailable widgets.

### QA/Integration
- Validate p95 targets for endpoints.
- Verify ABAC scope filtering across widgets.
- Verify edge cases (empty states, unknown actor, partial data).

**Checkpoint F0001‑A:** Dashboard loads with real data for authorized user.

---

## F0002 — Broker Relationship Management

### Dependencies
- Broker + Contact entities, soft delete rules
- ActivityTimelineEvent write on mutations
- ABAC enforcement per authorization matrix
- Broker/Contact OpenAPI + JSON Schemas

### Backend Assembly Steps
1. Implement Broker CRUD endpoints per OpenAPI (create/read/update/delete).
2. Enforce license immutability + global uniqueness; 409 on conflict.
3. Implement deactivation guard: block broker deactivation if active submissions/renewals exist (409 `active_dependencies_exist`). Implement reactivation endpoint (F0002-S0008) — restore Status to Active; emit BrokerReactivated timeline event; reject already-Active brokers with 409 `already_active`.
4. Implement Contact CRUD endpoints per OpenAPI (list/create/read/update/delete).
5. Enforce required email/phone and validation rules; return ProblemDetails on validation error.
6. Emit ActivityTimelineEvent for broker/contact create/update/delete.
7. Mask broker/contact email/phone on **all** broker and contact API responses (`GET /brokers`, `GET /brokers/{id}`, `GET /contacts`, `GET /contacts/{id}`) when `Broker.Status = Inactive`. Return `null` as the masking sentinel; see Broker and Contact schema descriptions in `nebula-api.yaml`.

### Frontend Assembly Steps
1. Broker List screen with search, filters, and status badges.
2. Broker 360 view with profile, contacts, timeline panel.
3. Contact create/update/delete flows within Broker 360.
4. Edit broker, deactivate broker flows with confirmation and error handling.

### QA/Integration
- Verify license immutability enforcement.
- Verify deactivation guard (`active_dependencies_exist`) when active submissions/renewals exist.
- Verify reactivation (F0002-S0008): Active→reject, Inactive→Active, unauthorized→403, not found→404.
- Verify masking behavior for inactive brokers on both list and detail endpoints (brokers and contacts).
- Verify ABAC scope on broker/contact reads and mutations.

**Checkpoint F0002‑A:** Broker 360 flow complete end‑to‑end.

---

## F0009 — Authentication + Role-Based Login

**Updated:** 2026-03-05 — Detailed implementation assembly plan (F0009 build pass)

### Dependencies
- F0005 authentik baseline and claim normalization (complete)
- F0009 implementation contract and broker visibility matrix:
  - `planning-mds/features/F0009-authentication-and-role-based-login/IMPLEMENTATION-CONTRACT.md`
  - `planning-mds/features/F0009-authentication-and-role-based-login/BROKER-VISIBILITY-MATRIX.md`
- BrokerUser matrix rules in `planning-mds/security/authorization-matrix.md` section 2.10
- BrokerUser policy rows in `planning-mds/security/policies/policy.csv`

### Pre-Existing Artifacts (Do Not Re-Implement)

The following are already implemented and correct as of the F0009 planning pass:

| Artifact | Location | Notes |
|----------|----------|-------|
| Auth event bus | `experience/src/features/auth/authEvents.ts` | `session_expired`, `broker_scope_unresolvable` |
| Session teardown hook | `experience/src/features/auth/useSessionTeardown.ts` | §2.1 teardown contract |
| Auth event handler | `experience/src/features/auth/useAuthEventHandler.ts` | Mounted in AppInner |
| OIDC UserManager singleton | `experience/src/features/auth/oidcUserManager.ts` | oidc-client-ts |
| Auth feature index | `experience/src/features/auth/index.ts` | Public surface |
| UnauthorizedPage | `experience/src/pages/UnauthorizedPage.tsx` | reason param support |
| App.tsx auth wiring | `experience/src/App.tsx` | useAuthEventHandler, /unauthorized route |
| API 401/403 interceptor | `experience/src/services/api.ts` | emits auth events |
| Vite auth-mode guard plugin | `experience/vite.config.ts` | §13 build guard |
| authModeGuard unit tests | `experience/src/features/auth/tests/authModeGuard.test.ts` | §13 coverage |
| POST /auth/logout | `engine/src/Nebula.Api/Endpoints/AuthEndpoints.cs` | §2.1 |
| ICurrentUserService.BrokerTenantId | `engine/src/Nebula.Application/Common/ICurrentUserService.cs` | Interface |
| HttpCurrentUserService.BrokerTenantId | `engine/src/Nebula.Api/Services/HttpCurrentUserService.cs` | broker_tenant_id claim |
| policy.csv §2.10 | `planning-mds/security/policies/policy.csv` | BrokerUser policy rows |
| AuditBrokerUserRead helpers | BrokerService, DashboardService, TimelineService, TaskService | Audit logging |

### Backend Assembly Steps

1. **(A) ActivityTimelineEvent.BrokerDescription migration**
   - Add nullable `string? BrokerDescription` to `ActivityTimelineEvent` entity
   - Generate and apply EF Core migration `20260305_F0009_BrokerDescription`
   - Update `ActivityTimelineEventConfiguration.cs` if needed

2. **(B) Broker scope resolution infrastructure**
   - Add `GetIdByBrokerTenantIdAsync(string tenantId)` to `IBrokerRepository` + `BrokerRepository`
   - Create `BrokerScopeUnresolvableException` in `Nebula.Application`
   - Register global exception middleware mapping to `broker_scope_unresolvable` ProblemDetails (§6.1)
   - Create `BrokerScopeResolver` service that reads `ICurrentUserService.BrokerTenantId` and calls the new repo method

3. **(C, D) BrokerService scope + DTO filtering**
   - `ListAsync`: if `user.Roles.Contains("BrokerUser")`, scope query to `BrokerTenantId`-resolved broker only
   - `GetByIdAsync`: verify resolved broker ID matches requested broker ID; throw `BrokerScopeUnresolvableException` if not
   - Create `BrokerBrokerUserDto` (excludes `RowVersion`, `IsDeactivated`) for BrokerUser responses

4. **(E) ContactService BrokerUser scope + DTO**
   - Scope contact reads to broker scope resolved from `BrokerTenantId`
   - Create `ContactBrokerUserDto` (excludes `RowVersion`)

5. **(F, G) TimelineService + BrokerDescription population**
   - `BrokerService` mutations: populate `BrokerDescription` using templates from BROKER-VISIBILITY-MATRIX.md for approved event types
   - `TimelineService.ListEventsAsync` for BrokerUser: filter to approved event types; return `BrokerDescription` instead of `EventDescription` in response DTO

6. **(H) TaskService BrokerUser scope filter**
   - For BrokerUser: filter tasks where `LinkedEntityType='Broker'` AND `LinkedEntityId` = resolved broker ID
   - Return task DTO subset: `id`, `title`, `status`, `priority`, `dueDate`, `linkedEntityType`, `linkedEntityId` (omit `assignedToUserId`, audit timestamps)

7. **(I) DashboardService/Repository nudge BrokerUser scope filter**
   - For BrokerUser: filter nudges to `nudgeType='OverdueTask'` AND `linkedEntityType='Broker'` AND `linkedEntityId IN resolved broker scope`
   - Empty result → return empty array (not 403); 403 only if scope resolution fails

8. **(J) DevSeedData broker tenant mapping**
   - Add seed row linking `broker001@example.local`'s `broker_tenant_id` to an existing test Broker entity

### Frontend Assembly Steps

1. **(K) LoginPage.tsx** at `/login`
   - Sign-in button triggers `oidcUserManager.signinRedirect()` (PKCE)
   - If OIDC config is missing (empty authority/clientId/redirectUri): disable button, show deterministic error
   - If IdP unavailable (signinRedirect throws): show deterministic retry guidance
   - Under `VITE_AUTH_MODE=dev`: redirect to `/` immediately (preserve existing dev workflow)

2. **(L) AuthCallbackPage.tsx** at `/auth/callback`
   - Calls `oidcUserManager.signinRedirectCallback()`
   - On success: resolve role from `nebula_roles` claim, redirect to role landing route
   - On failure (state/nonce/code validation error): clear stale state, redirect to `/login?error=callback_failed`
   - Missing/unsupported `nebula_roles`: redirect to `/unauthorized`
   - BrokerUser without `broker_tenant_id`: redirect to `/unauthorized`

3. **(M) ProtectedRoute component**
   - If no valid OIDC session: redirect to `/login`
   - If session exists but role not in allowedRoles: redirect to `/unauthorized`
   - Renders `<Outlet />` on success

4. **(N) useCurrentUser hook**
   - Reads OIDC user from `oidcUserManager.getUser()`
   - Returns `{ user, roles, isBrokerUser, isAuthenticated }`

5. **(O) api.ts resolveToken update**
   - Branch on `import.meta.env.VITE_AUTH_MODE`:
     - `'oidc'` or unset: `(await oidcUserManager.getUser())?.access_token ?? ''`
     - `'dev'`: `getDevToken()` (existing path unchanged)

6. **(P) App.tsx route wiring**
   - Add `/login` → `<LoginPage />`
   - Add `/auth/callback` → `<AuthCallbackPage />`
   - Wrap protected routes in `<ProtectedRoute>`
   - `/login` and `/auth/callback` are public (no ProtectedRoute wrapper)

### Infra Assembly Steps

1. **(Q) authentik blueprint update** (`docker/authentik/blueprints/nebula-dev.yaml`)
   - Add `BrokerUser` group
   - Add `broker_tenant_id` scope mapping expression
   - Add `lisa.wong@nebula.local` → DistributionUser group
   - Add `john.miller@nebula.local` → Underwriter group
   - Add `broker001@example.local` → BrokerUser group (with `broker_tenant_id` attribute)
   - Add `broker_tenant_id` scope mapping to the OAuth2 provider's property_mappings
   - All entries idempotent (use `identifiers:` correctly)

2. **(R) CI assertion** (`.github/workflows/frontend-ui.yml`)
   - Add step BEFORE `Build frontend` step: assert `VITE_AUTH_MODE != 'dev'` (per §13)

3. **(S) Env templates**
   - `experience/.env.example` — add `VITE_AUTH_MODE=oidc` with comment
   - `experience/.env.staging` — `VITE_AUTH_MODE=oidc`
   - `experience/.env.production` — `VITE_AUTH_MODE=oidc`
   - `experience/.env.development.local.example` — `VITE_AUTH_MODE=dev`

### QA Integration Steps

1. **(T) Test plan document** — `planning-mds/features/F0009-authentication-and-role-based-login/TEST-PLAN.md`
2. **(U) Backend unit tests**: scope resolver, BrokerDescription templates, policy deny
3. **(V) Frontend component tests**: LoginPage error states, AuthCallbackPage failure paths, ProtectedRoute guard behavior
4. **(W) Backend integration tests**: BrokerUser field exclusion, cross-broker deny, timeline event type filter
5. **(X) E2E tests** (Playwright): happy path login for all 3 seeded users, session expiry, 403 in-context

### Dependency Order

```
Step 1 (Backend): (A) migration → (B) scope resolver → (C–I) service layer [parallel]
Step 2 (Backend): (J) DevSeedData
Step 1 (Frontend): (N) useCurrentUser → (K) LoginPage → (L) AuthCallbackPage
                   (O) api.ts → (M) ProtectedRoute → (P) App.tsx wiring [sequential]
Step 1 (Infra):   (Q) blueprint + (R) CI + (S) env templates [parallel, independent]
Step 2 (QA):      (T–X) tests [depends on all above]
```

Backend, Frontend, and Infra steps proceed in parallel.

### QA/Integration Validation (from IMPLEMENTATION-CONTRACT.md §10)

- [ ] Login redirect/callback happy path for `lisa.wong`, `john.miller`, `broker001`
- [ ] Session-expired redirect + stale-state cleanup
- [ ] Route guard: 401 → teardown → `/login`; 403 → in-context error
- [ ] BrokerUser cross-scope denial (list + detail)
- [ ] BrokerUser field filtering (no `InternalOnly` fields in responses)
- [ ] Matrix vs policy parity check for BrokerUser actions
- [ ] Missing/invalid `broker_tenant_id` claim deny

**Checkpoint F0009‑A:** End-to-end login + broker boundary enforcement passes for all required seeded users.

---

## MVP Navigation Constraints

Several F0001 dashboard widgets reference click-through navigation to screens that are not in F0001/F0002 scope. The table below defines which targets are available and how unavailable targets degrade.

### Target Screen Availability

| Target Screen | In Scope? | Source | Notes |
|---------------|-----------|--------|-------|
| Broker 360 | Yes | F0002-S0003 | Fully available for click-through |
| Submission Detail | No | Future feature | Not in F0001/F0002 MVP |
| Renewal Detail | No | Future feature | Not in F0001/F0002 MVP |
| Submission List | No | Future feature | Not in F0001/F0002 MVP |
| Renewal List | No | Future feature | Not in F0001/F0002 MVP |
| Task Center | No | Future (F0003) | Not in F0001/F0002 MVP |

### Degradation Rules

When a navigation target is unavailable, the frontend must degrade gracefully:

1. **Links to unavailable screens render as plain text** — no `<a>` tag, no click handler, no pointer cursor. The entity name or label is still displayed for context but is not interactive.
2. **CTA buttons for unavailable targets are hidden** — if a nudge card's CTA would navigate to an unavailable screen, the CTA button is omitted; the card still displays its title, description, and urgency indicator.
3. **"View all" links to unavailable screens are hidden** — do not render "View all N" when the target list screen does not exist.
4. **No disabled/greyed-out links** — avoid confusing users with interactive-looking elements that do nothing. Omit rather than disable.
5. **No route stubs or placeholder pages** — do not create empty `/submissions` or `/renewals` routes. Routes are added when their feature is implemented.

### Per-Story Impact

| Story | Element | Target | Degradation |
|-------|---------|--------|-------------|
| F0001-S0002 | Mini-card click | Submission/Renewal Detail | Render entity name as plain text (not clickable) |
| F0001-S0002 | "View all N" link | Submission/Renewal List | Hide the link entirely |
| F0001-S0003 | Task row click (Broker) | Broker 360 | Works — F0002-S0003 in scope |
| F0001-S0003 | Task row click (Submission/Renewal/Account) | Detail screens | Render entity name as plain text (not clickable) |
| F0001-S0003 | Task row click (no linked entity) | Task Center | No navigation; row is informational only |
| F0001-S0003 | "View all tasks" link | Task Center | Hide the link entirely |
| F0001-S0004 | Feed item click | Broker 360 | Works — F0002-S0003 in scope |
| F0001-S0005 | CTA "Review Now" (Broker-linked task) | Broker 360 | Works — F0002-S0003 in scope |
| F0001-S0005 | CTA "Review Now" (non-Broker task) | Task Center / Detail | Hide CTA button |
| F0001-S0005 | CTA "Take Action" | Submission Detail | Hide CTA button |
| F0001-S0005 | CTA "Start Outreach" | Renewal Detail | Hide CTA button |

### Implementation Note

Navigation availability should be driven by a route registry check (e.g., `canNavigateTo(entityType)`) rather than hardcoded booleans. When the relevant future features are implemented and their routes registered, dashboard click-through will automatically activate without modifying F0001 code.

---

## F0003 Scope Decision (Task Write Endpoints)

**Decision:** F0003 task write endpoints are **out of scope** for the F0001/F0002 implementation pass.

**Rationale:** F0001 dashboard widgets (My Tasks, Nudge Cards) only *read* task data. No F0001 or F0002 story requires creating, updating, or deleting tasks via API. Task data for dashboard testing will be provided via a dev seed migration alongside Submission and Renewal seed data.

**Impact:**
- `POST /tasks`, `PUT /tasks/{taskId}`, `DELETE /tasks/{taskId}` — routes not registered, return 404.
- `GET /my/tasks`, `GET /tasks/{taskId}` — implemented as part of F0001.
- Task entity, table, and indexes — created in Phase 1 (Data Model + Migrations) since F0001 queries depend on them.
- F0003-S0001, F0003-S0002, and F0003-S0003 stories remain in the story index at MVP priority for future activation.

---

## Cross‑Feature Integration

- Dashboard broker activity feed must surface broker mutations from F0002.
- Timeline events must be consistent across dashboard and Broker 360 view.
- Ensure consistent ProblemDetails error codes for conflicts (invalid_transition, missing_transition_prerequisite, active_dependencies_exist, already_active, concurrency_conflict). See `planning-mds/architecture/error-codes.md` for the authoritative list.

## Exit Criteria

- F0001, F0002, and F0009 stories pass acceptance criteria.
- API contract validation passes.
- ABAC policy enforcement verified for all roles in matrix (including BrokerUser phase-1 delta).

---

## F0001-S0005 Completion Pass — Nudge Cards Remaining Work

**Date:** 2026-03-07
**Owner:** Backend Developer + Frontend Developer + Quality Engineer
**Scope:** Fix the 5 open gaps in F0001-S0005 only. No schema migrations. No new routes. No AI scope.

### Scope Breakdown

| Layer | Required Work | Owner |
|-------|---------------|-------|
| Backend (`engine/`) | (1) Add `AssignedToUserId` scope filter to stale submission + upcoming renewal queries. (2) Replace `UpdatedAt`-based staleness with last `WorkflowTransition` date for submissions. (3) Raise nudge return cap from 3 to 10. | Backend Developer |
| Frontend (`experience/`) | (4) Add `role="alert"` to nudge card container div in `NudgeCard.tsx`. | Frontend Developer |
| Quality | (5) Add integration test asserting nudge priority ordering: overdue tasks fill before stale submissions, stale before upcoming renewals; cap at 10. | Quality Engineer |
| AI (`neuron/`) | Not in scope. | — |
| DevOps/Runtime | No new infra, no migration, no env-var changes. Confirm build + tests pass. | DevOps |

### Dependency Order

1. **Backend** — fix `DashboardRepository.GetNudgesAsync` (all three backend items are in the same method; implement together).
2. **Frontend** — add `role="alert"` (independent, can run in parallel with backend).
3. **Quality** — add integration test (depends on backend fix being in place).
4. **Self-review + CI** — lint, build, test all pass.

### Integration Checkpoints

- [ ] `DashboardRepository.GetNudgesAsync`: stale submissions filtered by `AssignedToUserId == userId`
- [ ] `DashboardRepository.GetNudgesAsync`: upcoming renewals filtered by `AssignedToUserId == userId`
- [ ] `DashboardRepository.GetNudgesAsync`: staleness days computed from last `WorkflowTransition.OccurredAt` where `ToState = submission.CurrentStatus`, not from `UpdatedAt`
- [ ] Backend returns up to 10 nudges total (overdue tasks fill first, then stale, then upcoming)
- [ ] `NudgeCard.tsx` card container has `role="alert"`
- [ ] Integration test asserts priority ordering and 10-item cap
- [ ] `dotnet test` passes
- [ ] `pnpm --dir experience lint && pnpm --dir experience build && pnpm --dir experience test` pass

### Implementation Notes

**WorkflowTransition-based staleness (backend):**
The canonical pattern already exists in `DashboardRepository.GetOpportunityItemsAsync` (lines 228–232). For nudge computation:
1. Fetch candidate submissions: non-terminal, `AssignedToUserId == userId`.
2. For each candidate, find the max `WorkflowTransition.OccurredAt` where `WorkflowType = "Submission"` AND `ToState = submission.CurrentStatus`. Fall back to `submission.CreatedAt` if no matching transition exists (new submission never transitioned).
3. Filter to candidates where `(DateTime.UtcNow - transitionDate).TotalDays > 5`.
4. Sort by most stale first. Take up to `(10 - nudges.Count)`.

**Scope filter pattern:**
Tasks already use `AssignedToUserId == userId`. Apply the same pattern to submissions and renewals.

**10-item cap pattern:**
Replace all `Take(3)` → `Take(10)` and `Take(3 - nudges.Count)` → `Take(10 - nudges.Count)`. Final return: `nudges.Take(10).ToList()`. Remove the intermediate early-return guards (or update them to `>= 10`).

**Frontend `role="alert"`:**
The card container `<div>` in `NudgeCard.tsx` receives `role="alert"` so screen readers announce new/updated nudge cards. This is the outer div, not the dismiss button.

### Risks and Blockers

| Item | Severity | Mitigation |
|------|----------|------------|
| WorkflowTransition staleness query: submissions without any transitions use `CreatedAt` as fallback — may produce inaccurate staleness for very new submissions | Low | Acceptable for MVP; documented in code comment |
| ABAC-scope for stale/upcoming: using `AssignedToUserId == userId` as the scope proxy rather than full Casbin per-row check | Medium | Per-row Casbin check is too expensive for a nudge aggregation query; `AssignedToUserId` is the established ownership pattern for tasks and is the correct approximation here |
