# F0004 — Task Center UI + Manager Assignment — Status

**Overall Status:** Implementation-Ready
**Last Updated:** 2026-03-21

## Story Checklist

| Story | Title | Status |
|-------|-------|--------|
| F0004-S0001 | Paginated task list API with filters and views | Pending |
| F0004-S0002 | User search API for assignee picker | Pending |
| F0004-S0003 | Cross-user task authorization (assign, reassign, creator access) | Pending |
| F0004-S0004 | Task Center list + filter UI | Pending |
| F0004-S0005 | Task create + edit UI with assignment | Pending |
| F0004-S0006 | Task detail panel + mobile view | Pending |

## Recommended Implementation Order

1. **F0004-S0003** — Cross-user task authorization (backend foundation, unblocks all other stories)
2. **F0004-S0001** — Task list API endpoint (powers the list UI)
3. **F0004-S0002** — User search API endpoint (powers the assignee picker)
4. **F0004-S0004** — Task Center list + filter UI (main frontend shell)
5. **F0004-S0005** — Task create + edit UI with assignment (form components)
6. **F0004-S0006** — Task detail panel + mobile view (detail experience)

Backend stories (S0001–S0003) can be implemented in parallel. Frontend stories (S0004–S0006) depend on the backend stories.

## Required Signoff Roles

| Role | Required | Why Required | Set By | Date |
|------|----------|--------------|--------|------|
| Quality Engineer | Yes | Full coverage: API endpoint tests, authorization matrix coverage, UI component tests, E2E flows, responsive behavior, accessibility. | Architect | 2026-03-21 |
| Code Reviewer | Yes | Cross-user authorization logic and Casbin policy changes require independent review for correctness and security. | Architect | 2026-03-21 |
| Security Reviewer | Yes | Cross-user task assignment introduces new ABAC conditions, creator-based access, and a user search endpoint. Security review required to validate no privilege escalation, no data leakage, and correct enforcement order. | Architect | 2026-03-21 |
| DevOps | No | No new infrastructure, no new containers, no environment changes. Only a new DB index migration. | Architect | 2026-03-21 |
| Architect | Yes | New Casbin policy conditions (`r.obj.creator == r.sub.id`), new API endpoints, and authorization model extension require architectural sign-off. | Architect | 2026-03-21 |

## Story Signoff Provenance

| Story | Role | Reviewer | Verdict | Evidence | Date | Notes |
|-------|------|----------|---------|----------|------|-------|
| F0004-S0001 | Quality Engineer | — | — | — | — | Populate when implementation begins |
| F0004-S0001 | Code Reviewer | — | — | — | — | |
| F0004-S0002 | Quality Engineer | — | — | — | — | |
| F0004-S0002 | Code Reviewer | — | — | — | — | |
| F0004-S0003 | Quality Engineer | — | — | — | — | |
| F0004-S0003 | Code Reviewer | — | — | — | — | |
| F0004-S0003 | Security Reviewer | — | — | — | — | Cross-user authz requires security sign-off |
| F0004-S0004 | Quality Engineer | — | — | — | — | |
| F0004-S0004 | Code Reviewer | — | — | — | — | |
| F0004-S0005 | Quality Engineer | — | — | — | — | |
| F0004-S0005 | Code Reviewer | — | — | — | — | |
| F0004-S0006 | Quality Engineer | — | — | — | — | |
| F0004-S0006 | Code Reviewer | — | — | — | — | |

## Feature-Level Signoff

| Role | Reviewer | Verdict | Date | Notes |
|------|----------|---------|------|-------|
| Architect | — | — | — | Required before merge to main |
| Security Reviewer | — | — | — | Required before merge to main |
