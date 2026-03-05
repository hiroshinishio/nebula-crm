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

### Dependencies
- F0005 authentik baseline and claim normalization
- F0009 implementation contract and broker visibility matrix:
  - `planning-mds/features/F0009-authentication-and-role-based-login/IMPLEMENTATION-CONTRACT.md`
  - `planning-mds/features/F0009-authentication-and-role-based-login/BROKER-VISIBILITY-MATRIX.md`
- BrokerUser matrix rules in `planning-mds/security/authorization-matrix.md` section 2.10
- BrokerUser policy rows in `planning-mds/security/policies/policy.csv`

### Backend Assembly Steps
1. Add BrokerUser policy rows in Casbin policy artifact and verify matrix/policy parity.
2. Implement broker scope resolution (`email` -> exactly one active broker; else deny).
3. Apply scope filtering to all BrokerUser-allowed read endpoints (broker/contact/dashboard/timeline/task).
4. Apply server-side BrokerVisible/InternalOnly field filtering for BrokerUser responses.
5. Preserve deterministic ProblemDetails semantics for 401/403 outcomes.

### Frontend Assembly Steps
1. Add auth routes: `/login`, `/auth/callback`, `/unauthorized`.
2. Implement OIDC code+PKCE flow via `oidc-client-ts`.
3. Add deterministic route guard behavior and API 401/403 handling.
4. Add role-based landing logic with precedence:
   - `Admin > DistributionManager > DistributionUser > Underwriter > BrokerUser`
5. Make `dev-auth` fallback explicit via feature flag only; primary flow is OIDC.

### QA/Integration
- Validate login/callback flow for all seeded identities.
- Validate 401 redirect and 403 in-context error behavior.
- Validate BrokerUser cross-broker denies and InternalOnly field exclusions.
- Validate matrix vs policy parity for BrokerUser resources/actions.
- Run security gate checklist: `planning-mds/security/F0009-security-review-checklist.md`.
- Verify Phase 1 compensating controls (no-RLS): tenant query filters + ABAC checks + DTO filtering + audit logs.

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
