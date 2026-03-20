# F0013 — Dashboard Framed Storytelling Canvas — Status

**Overall Status:** Done (Decision A override; active folder retained)
**Last Updated:** 2026-03-19
**Corrects:** F0012 (archived), F0011 (abandoned), F0010 (abandoned)

## Story Checklist

| Story | Title | Status |
|-------|-------|--------|
| F0013-S0000 | Editorial palette refresh — dark & light themes | [x] Done (Decision A override) |
| F0013-S0001 | Restore framed canvas identity with three-layer visual hierarchy | [x] Done (Decision A override) |
| F0013-S0002 | Build vertical timeline with connected stage nodes and terminal outcome branches | [x] Done (Decision A override) |
| F0013-S0003 | Add contextual mini-visualizations at each timeline stage node | [x] Done (Decision A override) |
| F0013-S0004 | Connect chapter controls as uniform override for timeline visualizations | [x] Done (Decision A override) |
| F0013-S0005 | Ensure responsive, accessibility, and performance parity | [x] Done (Decision A override) |

## Architecture Review

- [x] Architecture review complete (2026-03-14)
- [x] ADR-009 written: LOB classification + SLA configuration
- [x] Feature assembly plan updated with F0013 section
- [x] F0010/F0011/F0012 supersession status updated in assembly plan
- [x] Backend scope confirmed: LOB field, SLA table, breakdown endpoint, aging SLA enhancement

## Architect Closeout (Session 3)

- [x] Required signoff role coverage verified for every F0013 story (`S0000..S0005`) in Story Signoff Provenance
- [x] Integration checklist status verified against `planning-mds/architecture/feature-assembly-plan.md` (F0013 section)
- [x] ADR-009 implementation alignment verified

**Architect Exceptions (2026-03-17):**
1. Required role satisfaction is incomplete:
   - `Quality Engineer`, `Security Reviewer`, and `Code Reviewer` entries exist for every story, but story verdicts are not all PASS.
   - `DevOps` is marked as required in Required Signoff Roles, but no story-level DevOps provenance entries exist.
2. F0013 integration checklist is not fully complete:
   - Incomplete items remain (notably component decomposition targets, responsive/a11y E2E completion, and run/deploy closeout evidence).
3. ADR-009 core contract is implemented (LOB fields, SLA thresholds, aging SLA payload), but feature closeout is blocked by unresolved QE/Code Review findings.

**Architect Closeout Verdict:** `EXCEPTIONS RECORDED — NOT READY FOR DONE/ARCHIVE`

## Backend Progress

- [x] Validate existing F0012 backend changes (periodDays on KPIs, avgDwellDays + emphasis on flow nodes) are deployed and working
- [x] EF Migration: Add `LineOfBusiness` (string, nullable) to Submission and Renewal entities
- [x] EF Migration: Create `WorkflowSlaThreshold` table with seed data
- [x] Update Submission/Renewal DTOs and request schemas with LOB field
- [x] Implement breakdown endpoint: `GET /dashboard/opportunities/{entityType}/{status}/breakdown`
- [x] Enhance aging endpoint: add SLA bands (`sla` object) per status
- [x] Backfill OpenAPI spec: aging + hierarchy endpoint definitions (existing tech debt)
- [x] Update OpenAPI spec: breakdown endpoint, LOB fields, SLA response schema
- [x] Update dev seed data with LOB values on test submissions/renewals
- [x] Unit tests (breakdown groupBy, SLA band computation, LOB validation)
- [ ] Integration tests (breakdown endpoint, enhanced aging, LOB on CRUD)
- [ ] Full backend suite parity vs Session 0 baseline (blocked: new failures detected on 2026-03-16)

## Frontend Progress

- [x] Editorial palette tokens applied (dark: deep navy + coral + steel blue; light: warm gray + coral + steel blue)
- [x] Data visualization palette tokens defined (6 semantic colors)
- [x] Glass-card and glow utilities updated with new accent colors
- [ ] WCAG AA contrast verified for both themes
- [x] Three-layer visual hierarchy applied (glass-card restored on nudges, activity, tasks)
- [x] Glass-card depth and soft hover/focus glow on nudge cards
- [x] Glass-card depth and soft hover/focus glow on Activity panel
- [x] Glass-card depth and soft hover/focus glow on My Tasks panel
- [x] Story canvas zone (KPIs, flow, chapters) remains flat and borderless
- [x] Timeline bar replaces flat rectangular stage cells
- [x] Timeline stage nodes connected by proportional flow ribbons
- [x] Terminal outcome branches render at timeline end
- [x] Radial/donut chart popovers render on stage node hover/click
- [x] Radial center shows count, segments show composition
- [x] Mini-visual on one side, narrative callout (2-3 data-driven bullets) on the other per stop
- [x] Per-stop alternate view toggles on mini-visualizations (S0003)
- [x] Chapter controls (Flow/Friction/Outcomes) switch timeline emphasis and override mini-visuals
- [x] Collapsible left nav and right Neuron rail with adaptive canvas width
- [x] Legacy chapter overlays removed (`AgingOverlay`, `MixOverlay`) after S0004/S0005 consolidation
- [ ] Responsive layouts verified (desktop, tablet landscape, tablet portrait, phone)
- [x] F0013 opportunities component/integration tests passing (`OpportunitiesSummary.test.tsx`)
- [ ] Full frontend suite parity vs Session 0 baseline (blocked: pre-existing auth test failures)
- [ ] Frontend dependency/toolchain stable for repeatable gate reruns (blocked on 2026-03-19: `eslint`, `stylelint`, `tsc`, `vitest`, and `playwright` unavailable from `experience`)

## Cross-Cutting

- [x] Screen specification created (`planning-mds/screens/S-DASH-002-framed-storytelling-canvas.md`)
- [ ] Feature test plan executed
- [x] Deployability check evidence recorded (`planning-mds/operations/evidence/f0013/devops-2026-03-19.md`)
- [ ] No TODOs remain in implementation code

## Doc/Spec Remediation (Session 5)

- [x] Created `planning-mds/screens/S-DASH-002-framed-storytelling-canvas.md`
- [x] Updated `GETTING-STARTED.md` to reflect the current route, vertical timeline, responsive drilldown behavior, synchronized story queries, and lazy breakdown loading
- [x] Updated `F0013-S0005` to remove stale horizontal-timeline / eager-load assumptions and align responsive expectations with the implemented UI
- [ ] PM/QE/Code Review re-review still required after doc/spec remediation

## Gate Rerun (Session 6, 2026-03-19)

- [x] Recorded refreshed QE evidence (`planning-mds/operations/evidence/f0013/qe-2026-03-19.md`)
- [x] Recorded refreshed Code Review evidence (`planning-mds/operations/evidence/f0013/code-review-2026-03-19.md`)
- [x] Recorded refreshed Security evidence (`planning-mds/operations/evidence/f0013/security-2026-03-19.md`)
- [x] Recorded first DevOps evidence (`planning-mds/operations/evidence/f0013/devops-2026-03-19.md`)

**Gate Rerun Summary (2026-03-19):**
- QE remains blocked / non-pass:
  - Backend targeted regression is blocked by `MSB3021` access-denied errors under `engine/src/Nebula.Api/bin/Debug/net10.0/`.
  - Frontend host validation is blocked by missing `experience` tool binaries (`eslint`, `stylelint`, `tsc`, `vitest`, `playwright`).
  - Approved host/container install retries progressed but did not complete cleanly within the session window.
- Code Review remains `REJECTED`:
  - Light-theme KPI contrast is still not proven fixed.
  - Timeline/story decomposition remains below the assembly-plan target.
  - Residual direct `nebula-violet` utility usage and hardcoded proxy target remain.
- Security remains `CONDITIONAL PASS`:
  - No active authz/data-exposure flaw was surfaced in reviewed F0013 paths.
  - Scanner coverage is still incomplete (`gitleaks`, `semgrep`, `DAST_TARGET_URL`, dependency scan network reachability).
  - Targeted authz regression rerun is blocked by the same backend `MSB3021` output lock.
- DevOps remains `FAIL`:
  - Compose runtime health is green, and migration/seed artifacts are present.
  - Clean frontend/backend build proof is still missing in this workspace (`tsc` unavailable; backend build blocked by `MSB3021`).
- PM final closeout is still pending re-review after the refreshed gate evidence.

## Product Manager Closeout

### Session 4 (2026-03-19)

- [x] Re-reviewed F0013 against the current repo state and post-gate commits (`09d23be`, `7482207`)
- [x] Confirmed tracker state remains Decision A override only (`Done`, active folder retained)
- [x] Recorded PM review evidence (`planning-mds/operations/evidence/f0013/pm-2026-03-19.md`)

**PM Verification Summary (2026-03-19):**
- F0013 is **not ready** for final closeout / archive:
  - At review time, latest QE / Code Review / Security evidence still predated implementation changes on 2026-03-18 and 2026-03-19.
  - Required story-level `PASS` coverage remains incomplete (`Quality Engineer`, `Code Reviewer`, `Security Reviewer`, `DevOps`).
  - At review time, package drift remained unresolved (`planning-mds/screens/S-DASH-002-framed-storytelling-canvas.md` was missing; `GETTING-STARTED.md` was still out of sync with the implemented timeline/data-loading behavior).
  - At review time, fresh verification was partially blocked by environment/tooling instability (`pnpm` install rename `EACCES`; `dotnet test` `MSB3021` access denied into `Nebula.Api/bin`).
- PM closeout verdict: `FAIL — keep Decision A override status; do not archive`

### Session 3 (2026-03-17)

- [x] PRD acceptance criteria reviewed against Session 2 evidence (`qe-2026-03-17`, `security-2026-03-17`, `code-review-2026-03-17`)
- [x] Tracker docs synchronized (`REGISTRY`, `ROADMAP`, `BLUEPRINT`, `STORY-INDEX`)
- [x] Tracker validation commands executed

**PM Verification Summary (2026-03-17):**
- PRD acceptance criteria are **not yet met** for release/closeout:
  - QE gate includes FAIL/BLOCKED outcomes (contrast and visual/runtime blockers).
  - Code Review gate is REJECTED with unresolved critical/high findings.
  - Security gate is CONDITIONAL PASS with scanner/tooling follow-ups pending.
- At review time, feature was blocked pending remediation + re-review.

## Decision Gate Outcome (2026-03-17)

- [x] Option A selected by user: **Mark F0013 Done and keep active folder**
- [x] Feature status updated to Done in active trackers (no archive move performed)
- [x] Unresolved gate findings retained in this document as explicit release risk record
- [x] Done status is recorded as a product-owner override with known exceptions

## Required Signoff Roles (Set in Planning)

| Role | Required | Why Required | Set By | Date |
|------|----------|--------------|--------|------|
| Quality Engineer | Yes | 6-story acceptance criteria, responsive/a11y, performance budgets. | Architect | 2026-03-14 |
| Code Reviewer | Yes | New entity, new endpoint, SVG component decomposition. | Architect | 2026-03-14 |
| Security Reviewer | Yes | Breakdown endpoint authorization, LOB data exposure. | Architect | 2026-03-14 |
| DevOps | Yes | EF Core migrations (LOB + SLA table), seed data. | Architect | 2026-03-14 |
| Architect | No | Patterns documented in ADR-009; no architecture exceptions. | - | - |

## Story Signoff Provenance

| Story | Role | Reviewer | Verdict | Evidence | Date | Notes |
|-------|------|----------|---------|----------|------|-------|
| F0013-S0000 | Quality Engineer | Codex (QE) | FAIL | `planning-mds/operations/evidence/f0013/qe-2026-03-19.md` | 2026-03-19 | Known light-theme KPI contrast failure remains unresolved; fresh visual rerun blocked by frontend toolchain instability. |
| F0013-S0000 | Security Reviewer | Codex (Security) | CONDITIONAL PASS | `planning-mds/operations/evidence/f0013/security-2026-03-19.md` | 2026-03-19 | No active authz/data-exposure flaw found; scanner coverage and targeted authz rerun remain incomplete. |
| F0013-S0000 | Code Reviewer | Codex (Code Review) | FAIL | `planning-mds/operations/evidence/f0013/code-review-2026-03-19.md` | 2026-03-19 | Contrast requirement and residual direct palette usage still block approval. |
| F0013-S0000 | DevOps | Codex (DevOps) | FAIL | `planning-mds/operations/evidence/f0013/devops-2026-03-19.md` | 2026-03-19 | Clean deployability proof is incomplete while frontend build tooling is unavailable and backend build hits `MSB3021`. |
| F0013-S0001 | Quality Engineer | Codex (QE) | BLOCKED | `planning-mds/operations/evidence/f0013/qe-2026-03-19.md` | 2026-03-19 | Full hierarchy E2E could not rerun with the frontend toolchain unavailable. |
| F0013-S0001 | Security Reviewer | Codex (Security) | CONDITIONAL PASS | `planning-mds/operations/evidence/f0013/security-2026-03-19.md` | 2026-03-19 | Story data paths remain scoped; residual risk is scanner/runtime incompleteness. |
| F0013-S0001 | Code Reviewer | Codex (Code Review) | FAIL | `planning-mds/operations/evidence/f0013/code-review-2026-03-19.md` | 2026-03-19 | Shared contrast/palette blockers still prevent approval. |
| F0013-S0001 | DevOps | Codex (DevOps) | FAIL | `planning-mds/operations/evidence/f0013/devops-2026-03-19.md` | 2026-03-19 | Feature runtime health is green, but release-proof build/deploy evidence is still incomplete. |
| F0013-S0002 | Quality Engineer | Codex (QE) | BLOCKED | `planning-mds/operations/evidence/f0013/qe-2026-03-19.md` | 2026-03-19 | Timeline E2E rerun remains blocked by frontend toolchain instability. |
| F0013-S0002 | Security Reviewer | Codex (Security) | CONDITIONAL PASS | `planning-mds/operations/evidence/f0013/security-2026-03-19.md` | 2026-03-19 | Timeline aggregates still inherit scoped query controls. |
| F0013-S0002 | Code Reviewer | Codex (Code Review) | FAIL | `planning-mds/operations/evidence/f0013/code-review-2026-03-19.md` | 2026-03-19 | Timeline/story implementation remains below the planned decomposition target. |
| F0013-S0002 | DevOps | Codex (DevOps) | FAIL | `planning-mds/operations/evidence/f0013/devops-2026-03-19.md` | 2026-03-19 | Runtime proof exists only partially; clean frontend/backend build evidence is missing. |
| F0013-S0003 | Quality Engineer | Codex (QE) | BLOCKED | `planning-mds/operations/evidence/f0013/qe-2026-03-19.md` | 2026-03-19 | Mini-visual behavior could not be revalidated end-to-end in this environment. |
| F0013-S0003 | Security Reviewer | Codex (Security) | CONDITIONAL PASS | `planning-mds/operations/evidence/f0013/security-2026-03-19.md` | 2026-03-19 | Breakdown endpoints still enforce scoped data filtering; scanner coverage remains partial. |
| F0013-S0003 | Code Reviewer | Codex (Code Review) | FAIL | `planning-mds/operations/evidence/f0013/code-review-2026-03-19.md` | 2026-03-19 | Mini-visual/callout extraction remains incomplete versus the assembly plan. |
| F0013-S0003 | DevOps | Codex (DevOps) | FAIL | `planning-mds/operations/evidence/f0013/devops-2026-03-19.md` | 2026-03-19 | Migration/seed artifacts are present, but deployability evidence is still not cleanly green. |
| F0013-S0004 | Quality Engineer | Codex (QE) | BLOCKED | `planning-mds/operations/evidence/f0013/qe-2026-03-19.md` | 2026-03-19 | Chapter override behavior could not be revalidated end-to-end in this environment. |
| F0013-S0004 | Security Reviewer | Codex (Security) | CONDITIONAL PASS | `planning-mds/operations/evidence/f0013/security-2026-03-19.md` | 2026-03-19 | Chapter-mode endpoints still use the same secured scoped repository paths. |
| F0013-S0004 | Code Reviewer | Codex (Code Review) | FAIL | `planning-mds/operations/evidence/f0013/code-review-2026-03-19.md` | 2026-03-19 | Chapter flow still sits on unresolved decomposition/palette issues. |
| F0013-S0004 | DevOps | Codex (DevOps) | FAIL | `planning-mds/operations/evidence/f0013/devops-2026-03-19.md` | 2026-03-19 | No clean build/deploy proof could be attached for the current repo state. |
| F0013-S0005 | Quality Engineer | Codex (QE) | FAIL | `planning-mds/operations/evidence/f0013/qe-2026-03-19.md` | 2026-03-19 | Responsive/a11y/perf parity still lacks fresh proof, and the known KPI contrast miss remains unresolved. |
| F0013-S0005 | Security Reviewer | Codex (Security) | CONDITIONAL PASS | `planning-mds/operations/evidence/f0013/security-2026-03-19.md` | 2026-03-19 | Security parity remains intact for implemented controls; full scanner gate is still pending. |
| F0013-S0005 | Code Reviewer | Codex (Code Review) | FAIL | `planning-mds/operations/evidence/f0013/code-review-2026-03-19.md` | 2026-03-19 | Contrast requirement and visual gate portability issues remain unresolved. |
| F0013-S0005 | DevOps | Codex (DevOps) | FAIL | `planning-mds/operations/evidence/f0013/devops-2026-03-19.md` | 2026-03-19 | Deployability closeout still lacks clean build proof for frontend and backend. |

## Deferred Non-Blocking Follow-ups (Optional)

| Follow-up | Why deferred | Tracking link | Owner |
|-----------|--------------|---------------|-------|
| Drilldown from radial popover to filtered list view | Not core to storytelling canvas — can be added as enhancement | - | - |

## Tracker Sync Checklist

- [x] `planning-mds/features/REGISTRY.md` status/path aligned
- [x] `planning-mds/features/ROADMAP.md` section aligned (`Now/Next/Later/Completed`)
- [x] `planning-mds/features/STORY-INDEX.md` regenerated
- [x] `planning-mds/BLUEPRINT.md` feature/story status links aligned
- [ ] Every required signoff role has story-level `PASS` entries with reviewer, date, and evidence

## Archival Criteria

All items above must be checked before moving this feature folder to `planning-mds/features/archive/`.
