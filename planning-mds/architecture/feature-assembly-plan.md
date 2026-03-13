# Feature Assembly Plan (F0001 + F0002 + F0009 + F0010 + F0011)

**Owner:** Architect
**Status:** Approved
**Last Updated:** 2026-03-12

## Goal

Define the build order, role handoffs, and integration checkpoints for F0001 (Dashboard), F0002 (Broker Relationship Management), F0009 (Authentication + Role-Based Login), F0010 (Dashboard Opportunities Refactor), and F0011 (Dashboard Opportunities Flow-First Modernization).

---

## F0011 — Dashboard Opportunities Flow-First Modernization (Connected Pipeline + Terminal Outcomes)

**Updated:** 2026-03-12 — Planning assembly pass

### Dependencies

- F0010 baseline opportunities implementation (Pipeline Board + Heatmap/Treemap/Sunburst)
- Existing opportunities endpoints:
  - `GET /dashboard/opportunities`
  - `GET /dashboard/opportunities/flow`
  - `GET /dashboard/opportunities/{entityType}/{status}/items`
- Existing ABAC policy coverage for `dashboard_pipeline`

### Architecture Notes

**This slice is a dashboard opportunities refactor with additive aggregate contract work.**

No workflow-state taxonomy changes are expected. Focus is on:
- Flow-first connected rendering model
- Terminal outcomes rail aggregates and drilldowns
- Visual hierarchy refresh and responsive interaction parity

#### Backend Scope (Expected)

- Extend opportunities payloads (or add dedicated aggregate endpoint) to support:
  - deterministic stage sequence metadata for connected rendering
  - terminal outcome summary nodes (count, percent, average days to exit)
  - stable drilldown target identifiers for stage and outcome nodes
- Preserve `dashboard_pipeline` authorization behavior for all opportunities routes.

#### Frontend Scope (Expected)

File placement remains under `experience/src/features/opportunities/`.

Primary additions/changes:
- `OpportunitiesSummary.tsx` -> promote connected flow canvas as default
- new flow canvas component(s) for stage node + ribbon rendering
- terminal outcomes rail component
- secondary mini-view strip (aging + radial-inspired summaries)
- responsive simplification for mobile layout (stacked stage cards + bottleneck/outcome list)

### Backend Assembly Steps

1. Define or update DTOs for stage sequence and outcome summaries.
2. Add/extend repository methods to compute terminal outcomes and stage ordering metadata.
3. Add/extend service methods for flow and outcome aggregates.
4. Add/extend dashboard endpoints for outcomes aggregate access.
5. Add unit tests for aggregate calculations and ordering guarantees.
6. Add integration tests for endpoint behavior, authorization, and validation.

### Frontend Assembly Steps

1. Add/update opportunities types for stage/outcome aggregate contracts.
2. Add/update hooks for connected flow and outcomes rail data.
3. Implement connected flow canvas default view.
4. Implement outcomes rail and outcome drilldown interactions.
5. Apply visual system updates (reduced border noise, warm-to-cool rhythm, stage emphasis).
6. Implement secondary mini-view strip and contextual expand interactions.
7. Apply responsive and accessibility behavior across breakpoints.
8. Add component and interaction tests; run lint/build/test gates.

### QA Assembly Steps

1. Create test plan with story-to-test mapping.
2. E2E: flow-default render, period switching, stage/outcome drilldowns.
3. E2E: mini-view expand behavior and return-to-flow context.
4. E2E: breakpoint parity (desktop/tablet/phone) and error isolation.
5. Validate accessibility requirements for keyboard/screen-reader flows.

### DevOps Assembly Steps

1. Confirm no new runtime services or env-var contracts are introduced.
2. Run backend/frontend runtime smoke checks in application runtime containers.
3. Capture deployability evidence and document deviations if discovered.

### Dependency Order

```
Step 1 (Backend):  DTO/contract updates -> repository aggregates -> service -> endpoint -> tests
Step 1 (Frontend): types/hooks -> connected flow -> outcomes rail -> visual system -> mini-views -> responsive/a11y -> tests
Step 2 (QA):       E2E + accessibility validation after backend/frontend merge
Step 2 (DevOps):   deployability smoke checks and evidence capture
```

### Integration Checklist

- [x] API contract touchpoints identified
- [x] Frontend contract touchpoints identified
- [ ] AI contract compatibility validated (if in scope) — N/A
- [x] Test cases mapped to acceptance criteria (planning test plan)
- [x] Run/deploy instructions drafted in feature getting-started docs

### Risks and Blockers

| Item | Severity | Mitigation | Owner |
|------|----------|------------|-------|
| Terminal outcome category mapping ambiguity | Medium | Confirm mapping before implementation; provide fallback grouping in contract | Product + Backend |
| Stage emphasis rule source ambiguity | Medium | Decide backend flag vs frontend-derived rule before build | Product + Frontend |
| Breakpoint complexity for connected flow | Medium | Explicitly test desktop/tablet/phone parity in QE plan | Frontend + QE |
| Visual refresh impacts readability | Low | Keep labels/counts first; enforce contrast checks | Frontend + QE |

### Signoff Role Matrix

| Role | Required | Rationale |
|------|----------|-----------|
| Quality Engineer | Yes | Baseline acceptance criteria and cross-device parity coverage |
| Code Reviewer | Yes | Baseline independent implementation review |
| Security Reviewer | Yes | Opportunities aggregate and drilldown authorization verification |
| DevOps | No | No expected infrastructure/env-contract changes |
| Architect | No | No planned architecture exceptions |

**Checkpoint F0011-A:** Connected opportunities flow default + outcomes rail + responsive accessibility parity are implemented and validated with ABAC-preserving drilldowns.

---

## F0010 — Dashboard Opportunities Refactor (Pipeline Board + Insight Views)

**Updated:** 2026-03-11 — Detailed implementation assembly plan (F0010 build pass)

### Dependencies
- Existing dashboard opportunities widget shell and period controls (F0001)
- Opportunities summary data (`GET /dashboard/opportunities`)
- Opportunities flow data (`GET /dashboard/opportunities/flow`)
- Opportunities drilldown mini-cards (`GET /dashboard/opportunities/{entityType}/{status}/items`)
- ABAC policy coverage for `dashboard_pipeline` resource

### Architecture Notes

**This is primarily a frontend refactor with two new backend aggregation endpoints.**

No new domain entities, no EF Core migrations, no Casbin policy changes. All data is read-only aggregation from existing Submission/Renewal/WorkflowTransition tables.

#### New Backend Endpoints

**`GET /dashboard/opportunities/aging`** (S0002)
- Query params: `entityType` (submission|renewal, required), `periodDays` (1–730, default 180)
- Returns: status × aging-bucket matrix. Fixed buckets: `0-2`, `3-5`, `6-10`, `11-20`, `21+`
- Response shape:
  ```json
  {
    "entityType": "submission",
    "periodDays": 180,
    "statuses": [{
      "status": "Received", "label": "Received", "colorGroup": "intake",
      "displayOrder": 1,
      "buckets": [
        { "key": "0-2", "label": "0–2 days", "count": 5 },
        { "key": "3-5", "label": "3–5 days", "count": 3 },
        { "key": "6-10", "label": "6–10 days", "count": 8 },
        { "key": "11-20", "label": "11–20 days", "count": 2 },
        { "key": "21+", "label": "21+ days", "count": 1 }
      ],
      "total": 19
    }]
  }
  ```
- Authorization: `dashboard_pipeline` (same as existing)
- Implementation: Query non-terminal entities, compute `daysInStatus` (same logic as mini-card items — last WorkflowTransition to current status, fallback to CreatedAt), group by status × bucket.

**`GET /dashboard/opportunities/hierarchy`** (S0003/S0004)
- Query params: `periodDays` (1–730, default 180)
- Returns: tree-structured data for Treemap and Sunburst views
- Response shape:
  ```json
  {
    "periodDays": 180,
    "root": {
      "id": "root", "label": "All Opportunities", "count": 142,
      "children": [{
        "id": "submission", "label": "Submissions", "count": 85,
        "levelType": "entityType",
        "children": [{
          "id": "submission:intake", "label": "Intake", "count": 20,
          "levelType": "colorGroup", "colorGroup": "intake",
          "children": [{
            "id": "submission:intake:Received", "label": "Received",
            "count": 20, "levelType": "status", "colorGroup": "intake"
          }]
        }]
      }]
    }
  }
  ```
- Authorization: `dashboard_pipeline`
- Implementation: Query non-terminal submissions + renewals, group by EntityType → ColorGroup → Status. Reuses reference status tables for labels and display ordering.

#### Frontend Component Architecture

File placement: All new files in `experience/src/features/opportunities/`.

```
features/opportunities/
├── components/
│   ├── OpportunitiesSummary.tsx       ← MODIFY: add view mode state, render active view
│   ├── OpportunityChart.tsx           ← KEEP (Sankey, remove from default path)
│   ├── OpportunityPipelineBoard.tsx   ← NEW: Pipeline Board default (S0001)
│   ├── OpportunityHeatmap.tsx         ← NEW: Aging Heatmap (S0002)
│   ├── OpportunityTreemap.tsx         ← NEW: Composition Treemap (S0003)
│   ├── OpportunitySunburst.tsx        ← NEW: Hierarchy Sunburst (S0004)
│   ├── OpportunityViewSwitcher.tsx    ← NEW: View mode toggle bar
│   ├── OpportunityDrilldown.tsx       ← NEW: Unified drilldown (S0005)
│   ├── OpportunityPopover.tsx         ← KEEP (reused by drilldown)
│   ├── OpportunityMiniCard.tsx        ← KEEP
│   └── OpportunityPill.tsx            ← KEEP (reuse in Pipeline Board)
├── hooks/
│   ├── useDashboardOpportunities.ts   ← KEEP
│   ├── useOpportunityFlow.ts          ← KEEP
│   ├── useOpportunityItems.ts         ← KEEP
│   ├── useOpportunityAging.ts         ← NEW (S0002)
│   └── useOpportunityHierarchy.ts     ← NEW (S0003/S0004)
├── lib/
│   └── opportunity-colors.ts          ← KEEP
├── types.ts                           ← MODIFY: add new DTOs
└── index.ts                           ← KEEP
```

View mode state: `useState<'pipeline' | 'heatmap' | 'treemap' | 'sunburst'>('pipeline')`.

Responsive strategy (S0005):
- Desktop (≥1280px): Full visualization, side-by-side entity sections
- Tablet (768–1279px): Stacked entity sections, reduced chart heights
- Mobile (<768px): Single entity tab, horizontally scrollable board, compact charts

Accessibility (S0005):
- View switcher: `role="tablist"` / `role="tab"` with `aria-selected`
- SVG visualizations: `aria-label` on containers, text summary fallback
- Keyboard: Tab for view navigation, Enter/Space for target selection, Escape for drilldown close
- `prefers-reduced-motion`: disable chart transitions

Charting approach: Use custom SVG implementations. Heatmap = HTML table with intensity backgrounds. Treemap and Sunburst use `d3-hierarchy` for layout math with custom SVG rendering. Avoid heavy charting libraries.

### Backend Assembly Steps

1. **(A)** Add `OpportunityAgingDto`, `OpportunityAgingStatusDto`, `OpportunityAgingBucketDto` DTOs
2. **(B)** Add `OpportunityHierarchyDto`, `OpportunityHierarchyNodeDto` DTOs
3. **(C)** Add `GetOpportunityAgingAsync(entityType, periodDays)` to `IDashboardRepository` and implement in `DashboardRepository`
4. **(D)** Add `GetOpportunityHierarchyAsync(periodDays)` to `IDashboardRepository` and implement in `DashboardRepository`
5. **(E)** Add `GetOpportunityAgingAsync` and `GetOpportunityHierarchyAsync` to `DashboardService`
6. **(F)** Register `GET /dashboard/opportunities/aging` and `GET /dashboard/opportunities/hierarchy` endpoints in `DashboardEndpoints.cs`
7. **(G)** Add unit tests for aging bucket calculation and hierarchy tree construction
8. **(H)** Add integration tests for both new endpoints (200 OK, 401, 403, validation)

### Frontend Assembly Steps

1. **(I)** Add new types to `types.ts` (aging DTOs, hierarchy DTOs, view mode type)
2. **(J)** Create `useOpportunityAging.ts` and `useOpportunityHierarchy.ts` hooks
3. **(K)** Create `OpportunityViewSwitcher.tsx` — view mode toggle bar
4. **(L)** Create `OpportunityPipelineBoard.tsx` — Pipeline Board default view (S0001)
5. **(M)** Modify `OpportunitiesSummary.tsx` — add view mode state, render active view component
6. **(N)** Create `OpportunityHeatmap.tsx` — Aging Heatmap view (S0002)
7. **(O)** Create `OpportunityTreemap.tsx` — Composition Treemap view (S0003)
8. **(P)** Create `OpportunitySunburst.tsx` — Hierarchy Sunburst view (S0004)
9. **(Q)** Create `OpportunityDrilldown.tsx` — Unified drilldown popover (S0005)
10. **(R)** Apply responsive layout and accessibility across all views (S0005)
11. **(S)** Add component tests for all new components
12. **(T)** Run lint, build, test, lint:theme

### QA Assembly Steps

1. **(U)** Create test plan covering all 5 stories
2. **(V)** E2E test: default Pipeline Board load, period switching
3. **(W)** E2E test: view mode switching (all 4 views)
4. **(X)** E2E test: drilldown from Pipeline Board and Heatmap
5. **(Y)** E2E test: empty/error states
6. **(Z)** Validate ABAC scope preservation

### DevOps Assembly Steps

1. **(AA)** Verify no new infra/env-var dependencies
2. **(AB)** Run backend build + test in container
3. **(AC)** Run frontend build + test
4. **(AD)** Record deployability evidence

### Dependency Order

```
Step 1 (Backend):  (A,B) DTOs → (C,D) Repository [parallel] → (E) Service → (F) Endpoints → (G,H) Tests
Step 1 (Frontend): (I) Types → (J) Hooks → (K) ViewSwitcher → (L) PipelineBoard → (M) Summary refactor
                   → (N) Heatmap [depends on backend C] → (O) Treemap → (P) Sunburst [depends on backend D]
                   → (Q) Drilldown → (R) Responsive/A11y → (S,T) Tests
Step 2 (QA):       (U–Z) [depends on all implementation]
Step 2 (DevOps):   (AA–AD) [depends on all implementation]
```

Backend and Frontend (S0001/view switcher) can proceed in parallel. Frontend S0002-S0004 depend on their backend endpoints.

### Integration Checklist

- [x] API contract compatibility validated — existing endpoints unchanged, new endpoints follow REST patterns
- [x] Frontend contract compatibility validated — new hooks for new endpoints, existing hooks unchanged
- [ ] AI contract compatibility validated (if in scope) — N/A
- [ ] Test cases mapped to acceptance criteria
- [ ] Run/deploy instructions updated

### Risks and Blockers

| Item | Severity | Mitigation | Owner |
|------|----------|------------|-------|
| Charting library for Treemap/Sunburst | Medium | Use d3-hierarchy for layout math + custom SVG rendering. Minimal new dependency. | Frontend Developer |
| Aging bucket query performance | Low | Aggregate in DB query. Use same patterns as existing flow query. | Backend Developer |
| Responsive complexity (4 views × 3 breakpoints) | Medium | Test each combination explicitly in E2E. | Frontend Developer + QE |
| Sankey removal surprise | Low | Keep OpportunityChart.tsx in codebase, just not default. Can restore later. | Architect |

### Signoff Role Matrix

| Role | Required | Rationale |
|------|----------|-----------|
| Quality Engineer | Yes | Baseline: acceptance criteria coverage for 5 stories, E2E workflows |
| Code Reviewer | Yes | Baseline: independent implementation review |
| Security Reviewer | Yes | New backend endpoints require authorization verification |
| DevOps | No | No new infra, env vars, or deployment changes |
| Architect | No | Standard patterns, no architecture exceptions |

**Checkpoint F0010-A:** Opportunities widget defaults to Pipeline Board, optional insights render correctly, and drilldowns remain scoped and usable across breakpoints.

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

---

## F0002-S0009 — Native Casbin Enforcer Adoption

**Date:** 2026-03-08
**Owner:** Backend Developer Agent
**ADR:** `planning-mds/architecture/decisions/ADR-008-casbin-enforcer-adoption.md`

### Context

Stories S0001–S0008 are **Done**. The remaining work is S0009: replace the hand-rolled `PolicyAuthorizationService` with a native Casbin enforcer backed by `model.conf` + `policy.csv`. All existing endpoint authorization call sites (`HasAccessAsync` → `IAuthorizationService.AuthorizeAsync`) must continue to work unchanged. Frontend is unaffected — S0009 is a backend-internal change behind the existing `IAuthorizationService` interface.

### Scope Breakdown

| Layer | Required Work | Owner | Status |
|-------|---------------|-------|--------|
| Backend (`engine/`) | Replace `PolicyAuthorizationService` with `CasbinAuthorizationService`; add `Casbin.NET` NuGet; update DI; add startup validation; keep interface stable | Backend Developer | Planned |
| Frontend (`experience/`) | **None** — authorization change is behind existing API; no contract changes | — | N/A |
| AI (`neuron/`) | **None** | — | N/A |
| Quality | Unit tests for Casbin service; integration tests for policy matrix parity; startup failure tests | Backend Developer + Quality Engineer | Planned |
| DevOps/Runtime | Verify `policy.csv` + `model.conf` embedded resources resolve; no new infra dependencies | DevOps | Planned |

### Implementation Slices (Dependency Order)

#### Slice A — Safety Net (Baseline Tests)
1. Review existing `BrokerAuthorizationTests` — currently tests no-role 403 only.
2. Add positive authorization tests per role/action from `policy.csv` for F0002 resources:
   - Broker: create, read, search, update, delete, reactivate — per role matrix.
   - Contact: create, read, update, delete — per role matrix.
   - Timeline: read — per role matrix.
3. Add negative tests for denied actions (e.g., Underwriter cannot search brokers, RelationshipManager cannot delete brokers).
4. These tests lock the **current expected behavior** before the switch.

#### Slice B — Casbin Enforcer Implementation
1. Add `Casbin.NET` NuGet package to `Nebula.Infrastructure.csproj`.
2. Add `model.conf` as embedded resource in `Nebula.Infrastructure.csproj`.
3. Create `CasbinAuthorizationService : IAuthorizationService` in `Nebula.Infrastructure/Authorization/`.
4. Initialize Casbin `Enforcer` from embedded `model.conf` + `policy.csv` streams.
5. Map `AuthorizeAsync(role, resourceType, action, attrs?)` to `Enforcer.Enforce(subObj, objObj, action)`:
   - `subObj` = record with `Role = role`, `Id = attrs?["subjectId"]` (defaults to empty string if absent).
   - `objObj` = record with `Type = resourceType`, `Assignee = attrs?["assignee"]` (defaults to empty string if absent).
6. Fail fast on policy/model loading errors (throw `InvalidOperationException` at construction, not at first request).

#### Slice C — DI Switch + Cleanup
1. Update `DependencyInjection.cs`: replace `PolicyAuthorizationService` → `CasbinAuthorizationService`.
2. Delete or rename `PolicyAuthorizationService.cs` (no runtime references remain).
3. `IAuthorizationService` interface is **unchanged** — zero endpoint code changes.

#### Slice D — Verification
1. Run all existing integration tests to confirm behavioral parity.
2. Run new positive/negative authorization matrix tests (Slice A).
3. Add unit test for `CasbinAuthorizationService` directly — verify condition-based policies.
4. Add startup failure test: corrupt model/policy → deterministic `InvalidOperationException`.
5. Verify BrokerUser scope-isolation path is unaffected (remains query-layer; Casbin not consulted for BrokerUser fast-path).

#### Slice E — Documentation + Status
1. Update `F0002/STATUS.md` — mark S0009 Done with implementation evidence.
2. Update `F0002/README.md` — mark S0009 Done.
3. Update `ADR-008` status from Proposed → Accepted.

### Key Design Decisions

**Casbin Request Object Mapping:**
The `model.conf` matcher uses structured sub-request access (`r.sub.role`, `r.obj.type`, `r.sub.id`, `r.obj.assignee`). The `CasbinAuthorizationService` passes C# objects to `Enforce()`:

```
Request:  Enforce({ Role: "Admin", Id: "" }, { Type: "broker", Assignee: "" }, "read")
Matcher:  r.sub.role == p.sub && r.obj.type == p.obj && r.act == p.act && eval(p.cond)
Policy:   p, Admin, broker, read, true
Result:   true (condition "true" evals to true)
```

For condition-based policies (task ownership):
```
Request:  Enforce({ Role: "DistributionUser", Id: "abc-123" }, { Type: "task", Assignee: "abc-123" }, "read")
Policy:   p, DistributionUser, task, read, r.obj.assignee == r.sub.id
eval():   r.obj.assignee ("abc-123") == r.sub.id ("abc-123") → true
```

**Interface Stability:** `IAuthorizationService.AuthorizeAsync` signature is unchanged. Endpoint code requires zero modifications.

**Singleton Lifecycle:** `CasbinAuthorizationService` remains singleton. The Casbin `Enforcer` is thread-safe for `Enforce()` calls.

### Integration Checklist

- [x] API contract compatibility validated — `IAuthorizationService` interface unchanged
- [x] Frontend contract compatibility validated — no contract changes (backend-internal)
- [ ] Test cases mapped to acceptance criteria — Slice A + D
- [ ] Run/deploy instructions updated — no new infra; embedded resources only

### Risks and Blockers

| Item | Severity | Mitigation |
|------|----------|------------|
| Casbin .NET `eval()` behavior for condition expressions may differ from hand-rolled evaluator | Medium | Pre-switch matrix tests lock expected behavior; run both before and after switch |
| Model.conf attribute access (`r.sub.role`) requires passing C# objects — Casbin .NET reflection may require specific property casing | Medium | Use concrete record types with matching property names; unit test verifies attribute access |
| Embedded resource resolution for `model.conf` | Low | Same pattern as existing `policy.csv`; tested at startup with fail-fast |
| WSL integration test path resolution (known limitation) | Low | Tests run from Windows or container; no new limitation |

### Checkpoint

**F0002-S0009-A:** All integration tests pass with native Casbin enforcer. Hand-rolled policy parser removed from runtime path. Behavioral parity confirmed.
