# F0002 — Broker & MGA Relationship Management — Status

**Overall Status:** In Progress
**Last Updated:** 2026-03-07

## Story Checklist

| Story | Title | Status | Notes |
|-------|-------|--------|-------|
| F0002-S0001 | Create Broker | Done | Confirmed in code + integration tests |
| F0002-S0002 | Search Brokers | Done | Confirmed in code + integration tests |
| F0002-S0003 | Read Broker (Broker 360 View) | Done | Confirmed in code + integration tests |
| F0002-S0004 | Update Broker | Done | Confirmed in code + integration tests (ETag/optimistic concurrency) |
| F0002-S0005 | Delete Broker (Deactivate) | Done | Confirmed in code + integration tests |
| F0002-S0006 | Manage Broker Contacts | Done | Confirmed in code + ContactEndpointTests |
| F0002-S0007 | View Broker Activity Timeline | Done | Confirmed in code + integration tests |
| F0002-S0008 | Reactivate Broker | In Progress | Fully implemented in code (backend ReactivateAsync + POST /{id}/reactivate endpoint, frontend ReactivateBrokerAction wired into BrokerDetailPage, BrokerReactivated audit event, all edge cases). Missing: (1) integration test for POST /brokers/{id}/reactivate; (2) endpoint not defined in nebula-api.yaml (only referenced in a description comment) |

## Open Gaps (Cross-Cutting)

1. **Missing Casbin authorization checks on broker/contact CRUD endpoints** — all broker (list, create, get, update, delete) and all contact (list, create, get, update, delete) operations rely on `RequireAuthorization()` (authentication only). Explicit `IAuthorizationService` Casbin policy checks (`broker:create`, `broker:update`, `broker:delete`, `contact:create`, etc.) are absent. Only `ReactivateBroker` has an explicit check.
2. **Deactivation status mismatch / PII exposure** — `BrokerService.DeleteAsync` sets `IsDeleted=true` but does not set `Status=Inactive`. `MaskPii` masks PII only when `Status == "Inactive"`. Result: Admin viewing a deactivated broker (bypasses global query filter) sees unmasked email/phone if the broker was Active before deactivation. API contract (`nebula-api.yaml:154`) says deactivation sets `Status=Inactive` — implementation must align.

## Resolved (F0009 Complete)

- Broker and contact endpoint authorization parity with `policy.csv` implemented in F0009.
- BrokerUser tenant isolation and field-boundary filtering implemented in F0009-S0004 and confirmed in BrokerService/ContactService.
