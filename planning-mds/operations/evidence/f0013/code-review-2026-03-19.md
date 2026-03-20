# Code Quality Review Report

Scope: F0013 (`planning-mds/features/F0013-dashboard-framed-storytelling-canvas/`)  
Date: 2026-03-19

## Summary
- Assessment: REJECTED
- Files reviewed: backend + frontend F0013 implementation paths, tests, and planning/status artifacts
- Total issues: 5 (1 Critical, 3 High, 1 Medium)

## Runtime and Automation Evidence

Runtime health:
- `docker compose ps --all` => PASS (all required services up/healthy)

Automation scripts:
- `python3 agents/code-reviewer/scripts/check-code-quality.py engine/src` => PASS (warnings only)
- `python3 agents/code-reviewer/scripts/check-code-quality.py experience/src` => PASS (warnings only)
- `python3 agents/code-reviewer/scripts/check-code-quality.py planning-mds/features/F0013-dashboard-framed-storytelling-canvas` => FAIL (`TODO` token count exceeded; long-line noise remains in story docs)
- `sh agents/code-reviewer/scripts/check-lint.sh --strict` => FAIL (`eslint` missing in environment; strict script also expects `format` script in `experience/package.json`)
- `sh agents/code-reviewer/scripts/check-pr-size.sh --base main --max 500` => PASS (`7` files changed, `261` insertions, `52` deletions)
- `sh agents/code-reviewer/scripts/check-test-coverage.sh --min 80 --auto` => FAIL (no coverage artifact detected)
- `python3 agents/product-manager/scripts/validate-trackers.py` => FAIL (required PASS/APPROVED provenance still missing for all F0013 stories and all required roles)

Targeted regression commands:
- `dotnet test engine/tests/Nebula.Tests/Nebula.Tests.csproj -v minimal --filter "FullyQualifiedName~DashboardRepositoryBreakdownAndAgingTests|FullyQualifiedName~DashboardRepositoryKpiTests|FullyQualifiedName~DashboardScopeFilteringTests|FullyQualifiedName~LineOfBusinessValidationTests|FullyQualifiedName~DashboardEndpointTests.GetOpportunityBreakdown" --logger "trx;LogFileName=f0013-code-review-2026-03-19-backend.trx"` => FAIL (`MSB3021` access denied copying assemblies into `engine/src/Nebula.Api/bin/Debug/net10.0/`)
- `CI=true pnpm --dir experience exec vitest run src/features/opportunities/tests/OpportunitiesSummary.test.tsx` => FAIL (`vitest` binary not found via `pnpm exec` in the current workspace state)

Prior gate evidence reviewed:
- `planning-mds/operations/evidence/f0013/qe-2026-03-17.md`
- `planning-mds/operations/evidence/f0013/security-2026-03-17.md`
- `planning-mds/operations/evidence/f0013/code-review-2026-03-17.md`

## Findings by Severity

### Critical Issues (must fix before approval)

1. Light-theme KPI label contrast is below the required threshold, so S0000/S0005 acceptance is not met.
   - Location:
     - `experience/src/index.css:57`
     - `experience/src/features/kpis/components/KpiCard.tsx:12`
     - `experience/tests/visual/theme-smoke.spec.ts:61`
   - Impact:
     - The last successful visual gate still fails on the KPI label contrast assertion (`labelContrast=2.998...` vs expected `> 3`), which blocks the responsive/a11y closeout story.
   - Recommendation:
     - Increase light-theme muted text contrast and keep the Playwright contrast assertion green before re-running the visual gate.

### High Priority (should fix)

1. F0013 decomposition contract remains incomplete: timeline/story logic is still concentrated in monolithic components instead of the planned split.
   - Location:
     - `experience/src/features/opportunities/components/StoryCanvas.tsx:9`
     - `experience/src/features/opportunities/components/StoryCanvas.tsx:155`
     - `experience/src/features/opportunities/components/ConnectedFlow.tsx:1`
     - `experience/src/features/opportunities/components/StageNodeStoryPanel.tsx:1`
     - `planning-mds/architecture/feature-assembly-plan.md:322`
     - `planning-mds/architecture/feature-assembly-plan.md:426`
   - Impact:
     - Maintainability and testability remain below the F0013 assembly expectations for timeline node, mini-visual, and narrative callout extraction.
   - Recommendation:
     - Extract timeline node/mini-visual/callout modules from `ConnectedFlow` and `StageNodeStoryPanel`, then move story-specific rendering logic into dedicated components.

2. Residual hardcoded palette class usage remains in F0013 opportunities UI.
   - Location:
     - `experience/src/features/opportunities/components/StoryCanvas.tsx:103`
     - `experience/src/features/opportunities/components/StoryCanvas.tsx:131`
     - `experience/src/features/opportunities/components/ConnectedFlow.tsx:348`
     - `experience/src/features/opportunities/components/StageNodeStoryPanel.tsx:1117`
   - Impact:
     - The UI still mixes semantic token usage with direct palette utilities, which weakens theme consistency and keeps the code harder to evolve.
   - Recommendation:
     - Replace direct `nebula-violet` utility classes with semantic token-backed classes for active pills, focus rings, and pagination dots.

3. Dev/test portability regression persists due hardcoded proxy target and non-deterministic visual mocks.
   - Location:
     - `experience/vite.config.ts:8`
     - `experience/tests/visual/theme-smoke.spec.ts:119`
   - Impact:
     - Containerized visual gates remain brittle, and the current frontend regression rerun is blocked because the local workspace cannot surface the `vitest` binary cleanly.
   - Recommendation:
     - Parameterize proxy target via environment and harden Playwright route mocks so visual checks do not depend on brittle proxy fallbacks.

### Medium Priority (nice to have)

1. Review automation signal is noisy/non-actionable for this feature scope.
   - Location:
     - `agents/code-reviewer/scripts/check-lint.sh` (`eslint` / `format` assumptions)
     - `agents/code-reviewer/scripts/check-test-coverage.sh` (no auto-discovered artifact)
     - `planning-mds/features/F0013-dashboard-framed-storytelling-canvas/STATUS.md:72` (`TODO` checklist text causes false positive)
   - Impact:
     - Gate output includes failures that are partly tooling- and docs-related rather than implementation-related, which slows triage.
   - Recommendation:
     - Calibrate review scripts for repo conventions and exclude planning checklist tokens from code-quality scans.

## Pattern Compliance
- [x] Clean architecture layers respected
- [ ] SOLID principles followed (frontend decomposition gap)
- [ ] SOLUTION-PATTERNS.md patterns applied (planned decomposition incomplete)
- [ ] Frontend UX rule-set checks passed (contrast gate still failing)
- [x] Naming conventions consistent
- [x] Error handling appropriate for reviewed F0013 code paths

## Test Quality
- Backend targeted regression is blocked by environment/bin-lock errors (`MSB3021`).
- Frontend focused opportunities suite could not rerun because `pnpm exec vitest` cannot resolve the binary in the current workspace state.
- Residual gaps for acceptance confidence:
  - deterministic visual E2E under container runtime
  - contrast guard remains failing in the last successful visual evidence

## Acceptance Criteria Mapping (Code Review Verdict)
- F0013-S0000: FAIL (contrast + residual hardcoded palette classes)
- F0013-S0001: FAIL (shared token/contrast blockers)
- F0013-S0002: FAIL (decomposition contract not completed)
- F0013-S0003: FAIL (mini-visual and callout extraction not completed per assembly plan)
- F0013-S0004: FAIL (chapter flow implemented on top of unresolved decomposition/palette issues)
- F0013-S0005: FAIL (contrast + visual gate portability instability)

## Recommendation
REJECT

## Action Items
1. Raise light-theme KPI label contrast above threshold and rerun the visual gate.
2. Remove remaining direct `nebula-violet` palette class usage from F0013 opportunities UI.
3. Complete the planned timeline component decomposition.
4. Make the Vite proxy target environment-configurable and harden the visual test mocks.
