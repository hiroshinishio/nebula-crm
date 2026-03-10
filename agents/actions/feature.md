# Action: Feature

## User Intent

Build a single feature as a complete vertical slice (backend + frontend + tests + deployability checks) that can be deployed and tested independently. Ideal for incremental delivery.

## Agent Flow

```
Architect (Implementation Orchestration)
  ↓
(Backend Developer + Frontend Developer + AI Engineer [if AI scope] + Quality Engineer + DevOps)
  ↓ [Parallel Implementation]
[SELF-REVIEW GATE: Each agent validates their work]
  ↓
Code Reviewer + Security
  ↓ [Parallel Reviews]
[Review Gate: resolve critical findings]
  ↓
[APPROVAL GATE: User reviews and approves]
  ↓
Feature Complete
```

**Flow Type:** Mixed (architect-led orchestration kickoff, parallel implementation including deployability checks, parallel code+security reviews, single approval gate; AI Engineer runs when feature includes AI scope)

---

## Runtime Execution Boundary

- The builder runtime orchestrates feature flow and gates; keep it stack-agnostic.
- Stack-specific compile/test/lint/security execution must run in application runtime containers (or CI jobs built from those container definitions).
- Review and approval decisions must cite evidence produced by those application runtime executions.

---

## Execution Steps

### Step 0: Architect-Led Feature Assembly Planning

**Execution Instructions:**

1. **Activate Architect agent** by reading `agents/architect/SKILL.md`
2. **Read context:**
   - Feature stories in `planning-mds/features/F{NNNN}-{slug}/F{NNNN}-S{NNNN}-{slug}.md`
   - `planning-mds/BLUEPRINT.md` scope and constraints
   - `planning-mds/architecture/SOLUTION-PATTERNS.md`
   - `planning-mds/api/` contracts for this feature
3. **Produce feature assembly plan:**
   - Required backend/frontend/AI changes for this feature only
   - Integration checkpoints and dependency order
   - Test and release checklist for the vertical slice
4. **Output artifact:**
   - `planning-mds/architecture/feature-assembly-plan.md` (use `agents/templates/feature-assembly-plan-template.md`)

**Completion Criteria for Step 0:**
- [ ] Feature assembly plan exists
- [ ] Feature scope and handoffs are explicit
- [ ] Integration/test checkpoints defined

---

### Step 0.5: Assembly Plan Validation

**Execution Instructions:**

Validate the feature assembly plan before parallel implementation:

- [ ] Scope split matches feature story requirements
- [ ] Dependencies between agents are identified
- [ ] Integration checkpoints are feasible
- [ ] No missing or conflicting artifact ownership

Validator:
- Code Reviewer or a second Architect review (lightweight checklist is sufficient)

---

### Step 1: Parallel Feature Implementation

**Execution Instructions:**

Execute these agents **in parallel** for the specific feature. Run AI Engineer when the feature touches `neuron/`, LLM workflows, prompts, or MCP.
All stack-specific execution (compile/tests/scans) must run in application runtime containers produced for this project.

**AI Scope Checklist — include AI Engineer if ANY apply:**
- [ ] Story mentions LLM, AI, or machine learning behavior
- [ ] Story requires MCP server/tool/resource work
- [ ] Story involves prompts, agent behavior, or tool orchestration
- [ ] Story changes files under `neuron/`
- [ ] Story requires model selection, cost controls, or guardrails

#### 1a. Backend Developer (Feature Scope)
1. **Activate Backend Developer agent** by reading `agents/backend-developer/SKILL.md`
2. **Read context:**
   - `planning-mds/BLUEPRINT.md` Section 4 (architecture for this feature)
   - `planning-mds/architecture/SOLUTION-PATTERNS.md`
   - User stories for THIS FEATURE ONLY
3. **Execute responsibilities (feature-scoped):**
   - Implement domain entities for this feature
   - Create or update EF Core entities
   - Generate migration (if schema changes needed)
   - Implement API endpoints for this feature
   - Write application services for feature business logic
   - Create unit tests for feature domain logic
   - Write integration tests for feature API endpoints
4. **Follow SOLUTION-PATTERNS.md:**
   - Casbin ABAC for authorization
   - ActivityTimelineEvent for mutations
   - ProblemDetails for errors
   - Clean architecture layers
   - Audit fields, soft delete
5. **Outputs (feature-specific):**
   - Domain entities (created or updated)
   - EF Core migration (if needed)
   - API endpoints (controllers)
   - Application services
   - Unit tests
   - Integration tests

#### 1b. Frontend Developer (Feature Scope)
1. **Activate Frontend Developer agent** by reading `agents/frontend-developer/SKILL.md`
2. **Read context:**
   - `planning-mds/BLUEPRINT.md` Section 3 (screens for this feature)
   - `planning-mds/architecture/SOLUTION-PATTERNS.md`
   - API contracts for THIS FEATURE ONLY
   - `agents/frontend-developer/references/ux-audit-ruleset.md`
3. **Execute responsibilities (feature-scoped):**
   - Create React components for feature screens
   - Implement forms for this feature (React Hook Form + AJV with JSON Schema)
   - Set up TanStack Query hooks for feature API calls
   - Add routing for feature screens
   - Style with Tailwind + shadcn/ui
   - Write component tests
   - Apply and pass UX rule-set checks for this feature's UI changes
4. **Follow SOLUTION-PATTERNS.md:**
   - React Hook Form for forms
   - AJV + JSON Schema for validation
   - TanStack Query for API
   - Tailwind + shadcn/ui for styling
   - UX rule-set compliance (`agents/frontend-developer/references/ux-audit-ruleset.md`)
5. **Outputs (feature-specific):**
   - React components (feature screens)
   - Form implementations
   - TanStack Query hooks
   - Routing updates
   - Component tests
   - UX audit evidence for this feature (command output + dark/light verification notes)

#### 1c. AI Engineer (Feature Scope, if AI scope)
1. **Activate AI Engineer agent** by reading `agents/ai-engineer/SKILL.md`
2. **Read context:**
   - AI-related user stories for THIS FEATURE
   - `planning-mds/architecture/SOLUTION-PATTERNS.md`
   - Existing `neuron/` code and interfaces
3. **Execute responsibilities (feature-scoped):**
   - Implement AI workflow/prompt/tool logic for this feature
   - Add/modify MCP resources/tools if the feature requires them
   - Add runtime guardrails (validation, retries, error handling)
   - Add tests for AI behavior and integrations
4. **Follow SOLUTION-PATTERNS.md:**
   - No hardcoded secrets
   - Explicit integration contracts with backend/frontend
   - Observable AI behavior (logs/metrics)
5. **Outputs (feature-specific):**
   - `neuron/` feature implementation
   - AI tests
   - Prompt/config updates

#### 1d. Quality Engineer (Feature Scope)
1. **Activate Quality Engineer agent** by reading `agents/quality-engineer/SKILL.md`
2. **Read context:**
   - User stories for THIS FEATURE with acceptance criteria
   - Workflows for THIS FEATURE
   - `planning-mds/architecture/SOLUTION-PATTERNS.md`
3. **Execute responsibilities (feature-scoped):**
   - Create test plan for this feature
   - Write E2E test for feature happy path
   - Write E2E test for feature error scenarios
   - Validate feature acceptance criteria coverage
   - Generate coverage report for feature code
4. **Follow SOLUTION-PATTERNS.md:**
   - Developers own unit/component and endpoint integration tests
   - QE validates coverage and closes critical cross-tier gaps
   - E2E tests for feature workflows
5. **Outputs (feature-specific):**
   - Test plan for feature
   - E2E tests (happy path + errors)
   - Feature test coverage report

#### 1e. DevOps (Feature Deployability Check)
1. **Activate DevOps agent** by reading `agents/devops/SKILL.md`
2. **Read context:**
   - Feature assembly plan (`planning-mds/architecture/feature-assembly-plan.md`)
   - Existing deployment artifacts (`docker-compose*.yml`, Dockerfiles, runtime configs)
   - Feature-specific runtime requirements from backend/frontend/AI outputs
3. **Execute responsibilities (feature-scoped):**
   - Verify feature can run in application runtime containers without breaking existing services
   - Update runtime/deployment configuration when feature introduces new dependencies
   - Validate environment-variable contract updates for this feature
   - Run feature-level container build/start smoke checks and capture evidence paths
4. **Outputs (feature-specific):**
   - Deployment/runtime config updates (if required)
   - Feature deployability check summary with executed command evidence
   - Updated env var documentation for new feature requirements

**Completion Criteria for Step 1:**
- [ ] All required agents completed feature implementation (Backend, Frontend, Quality, DevOps, and AI Engineer if AI scope)
- [ ] Feature code compiles/builds successfully in application runtime containers
- [ ] No critical errors

---

### Step 2: SELF-REVIEW GATE (Agent Validation)

**Execution Instructions:**

Each agent validates their feature work:

1. **Backend Developer self-review:**
   - [ ] Feature API endpoints implemented per contracts
   - [ ] Feature domain logic complete and tested
   - [ ] Unit tests passing for feature logic
   - [ ] Integration tests passing for feature endpoints
   - [ ] SOLUTION-PATTERNS.md followed
   - [ ] Feature acceptance criteria met
   - [ ] Migration applies successfully (if created)

2. **Frontend Developer self-review:**
   - [ ] Feature screens implemented per specs
   - [ ] Feature forms work with validation
   - [ ] API integration works for feature
   - [ ] Component tests passing
   - [ ] SOLUTION-PATTERNS.md followed
   - [ ] UX rule-set checks passed for this feature (`agents/frontend-developer/references/ux-audit-ruleset.md`)
   - [ ] `pnpm --dir experience lint`, `lint:theme`, `build`, and `test` passed
   - [ ] `pnpm --dir experience test:visual:theme` passed when style/theme changed
   - [ ] Feature acceptance criteria met

3. **AI Engineer self-review (if AI scope):**
   - [ ] AI feature behavior meets acceptance criteria
   - [ ] AI tests passing in AI runtime container
   - [ ] MCP/tool interfaces validated (if used)
   - [ ] Safety/cost/observability controls in place

4. **Quality Engineer self-review:**
   - [ ] Feature test plan complete
   - [ ] E2E tests passing for feature in application runtime containers
   - [ ] Coverage adequate for feature code
   - [ ] All feature acceptance criteria testable

5. **DevOps self-review:**
   - [ ] Feature deployability checks executed in application runtime containers
   - [ ] Runtime config/env-var changes documented and versioned
   - [ ] No runtime orchestration regressions introduced by the feature

**If any self-review fails:**
- Agent fixes issues
- Re-runs self-review
- Repeats until passing

**Gate Criteria:**
- [ ] Architect confirms feature output matches Step 0 plan
- [ ] All required agents pass self-review for feature
- [ ] All feature tests passing in application runtime containers
- [ ] Feature deployability evidence recorded by DevOps
- [ ] Feature works end-to-end

---

### Step 3: Execute Reviews (Parallel)

**Execution Instructions:**

Run these review agents in parallel:

#### 3a. Code Reviewer

1. **Activate Code Reviewer agent** by reading `agents/code-reviewer/SKILL.md`

2. **Read context:**
   - Feature code produced in Step 1
   - Application runtime validation outputs (test, lint, SAST, dependency scan reports)
   - `planning-mds/BLUEPRINT.md` (feature requirements)
   - `planning-mds/architecture/SOLUTION-PATTERNS.md`
   - Feature user stories with acceptance criteria

3. **Execute code review (feature-focused):**
   - Review feature code structure
   - Check SOLID principles in feature code
   - Validate clean architecture boundaries
   - Review feature test coverage and quality
   - Identify code smells in feature
   - Validate feature acceptance criteria met
   - Check SOLUTION-PATTERNS.md compliance
   - Assess vertical slice completeness

4. **Produce feature code review report:**
   ```markdown
   # Feature Code Review Report

   Feature: [Feature Name]

   ## Summary
   - Assessment: [APPROVED / APPROVED WITH RECOMMENDATIONS / REJECTED]
   - Files reviewed: [count]
   - Issues found: [count by severity]

   ## Vertical Slice Completeness
   - [ ] Backend complete (API endpoints functional)
   - [ ] Frontend complete (screens functional)
   - [ ] AI layer complete (if AI scope)
   - [ ] Tests complete (unit, integration, E2E)
   - [ ] Can be deployed independently

   ## Findings
   - Critical: [list]
   - High: [list]
   - Medium: [list]
   - Low: [list]

   ## Pattern Compliance
   - [ ] Clean architecture respected
   - [ ] SOLID principles followed
   - [ ] SOLUTION-PATTERNS.md applied
   - [ ] Test coverage ≥80% for feature logic

   ## Acceptance Criteria
   - [ ] All feature ACs met
   - [ ] Edge cases handled
   - [ ] Error scenarios covered

   ## Recommendation
   [APPROVE / REQUEST CHANGES / REJECT]
   ```

**Code Review Outputs:**
- Feature code review report
- Approval or rejection

#### 3b. Security

1. **Activate Security agent** by reading `agents/security/SKILL.md`

2. **Read context:**
   - Feature code produced in Step 1
   - Application runtime validation outputs (test, lint, SAST, dependency scan reports)
   - `planning-mds/BLUEPRINT.md` (feature requirements)
   - `planning-mds/architecture/SOLUTION-PATTERNS.md`
   - Feature user stories with acceptance criteria
   - Existing `planning-mds/security/` artifacts (if present)

3. **Execute security review (feature-focused):**
   - Check OWASP Top 10 risks relevant to this feature
   - Verify authorization coverage for feature endpoints/actions
   - Validate input/output validation and error leakage controls
   - Check secrets/config handling (no hardcoded secrets)
   - Validate audit logging coverage for mutations
   - Run dependency/container vulnerability scans in application runtime containers (or CI jobs built from them)

4. **Produce feature security review report:**
   ```markdown
   # Feature Security Review Report

   Feature: [Feature Name]

   ## Summary
   - Assessment: [PASS / PASS WITH RECOMMENDATIONS / FAIL]
   - Findings: [count by severity]

   ## Findings
   - Critical: [list]
   - High: [list]
   - Medium: [list]
   - Low: [list]

   ## Control Checks
   - [ ] Authorization coverage complete
   - [ ] Input validation enforced
   - [ ] No secrets in code
   - [ ] Auditability requirements met

   ## Recommendation
   [APPROVE / FIX CRITICAL / FIX HIGH / REJECT]
   ```

**Security Review Outputs:**
- Feature security review report
- Vulnerability findings and remediation guidance

---

### Step 4: APPROVAL GATE (Feature Review)

**Execution Instructions:**

1. **Present combined review results to user:**
   ```
   ═══════════════════════════════════════════════════════════
   Feature Reviews Complete
   ═══════════════════════════════════════════════════════════

   Feature: [Feature Name]
   Code Reviewer Status: [APPROVED / APPROVED WITH RECOMMENDATIONS / REJECTED]
   Security Status: [PASS / PASS WITH RECOMMENDATIONS / FAIL]

   ✓ Vertical Slice Completeness
     - Backend: [Complete/Incomplete]
     - Frontend: [Complete/Incomplete]
     - AI: [N/A/Complete/Incomplete]
     - Tests: [Complete/Incomplete]
     - Deployable: [Yes/No]

   Issues Found:
     - Critical: [count]
     - High: [count]
     - Medium: [count]
     - Low: [count]

   Security Findings:
     - Critical: [count]
     - High: [count]
     - Medium: [count]
     - Low: [count]

   ✓ Pattern Compliance
     - Clean Architecture: [Yes/No]
     - SOLID Principles: [Yes/No]
     - SOLUTION-PATTERNS.md: [Yes/No]
     - Test Coverage: [percentage]% (feature code)

   ✓ Acceptance Criteria
     - [count]/[total] feature ACs met
     - Edge cases: [Handled/Needs work]
     - Errors: [Covered/Needs work]

   ═══════════════════════════════════════════════════════════
   Review Details:
   [Link to feature code review report]
   [Link to feature security review report]
   ═══════════════════════════════════════════════════════════
   ```

2. **Present approval checklist:**
   ```
   Feature Approval Checklist:
   - [ ] Feature is a complete vertical slice
   - [ ] Backend implementation complete
   - [ ] Frontend implementation complete
   - [ ] AI implementation complete (if AI scope)
   - [ ] Tests cover feature completely with evidence from application runtime containers
   - [ ] No critical issues (approval blocked if any remain)
   - [ ] High-severity issues fixed OR approved with mitigation justification
   - [ ] SOLUTION-PATTERNS.md followed
   - [ ] All feature acceptance criteria met
   - [ ] Feature can be deployed independently
   ```

3. **Enforce approval gate based on combined findings severity:**
   ```
   total_critical = code_critical + security_critical
   total_high = code_high + security_high

   IF total_critical > 0:
     STATUS: ❌ BLOCKED
     OPTIONS: ["fix critical", "reject"]
     APPROVE_ENABLED: false

   ELSE IF total_high > 0:
     STATUS: ⚠️ WARNING
     OPTIONS: ["fix issues", "approve with justification", "reject"]
     APPROVE_ENABLED: true (requires justification)

   ELSE:
     STATUS: ✓ ACCEPTABLE
     OPTIONS: ["approve", "fix issues", "reject"]
     APPROVE_ENABLED: true
   ```

4. **Handle user response:**
   - **If "fix critical":**
     - Identify critical issues to fix
     - Agents fix issues
     - Return to Step 3 (re-run code and security reviews)

   - **If "fix issues":**
     - Identify selected issues to fix
     - Agents fix issues
     - Return to Step 3 (re-run code and security reviews)

   - **If "approve with justification":**
     - Capture explicit mitigation justification for remaining high issues
     - Log decision with mitigation plan
     - Proceed to Step 4.5 (Tracker Sync Gate)

   - **If "approve":**
     - Proceed to Step 4.5 (Tracker Sync Gate)

   - **If "reject":**
     - Capture feedback
     - Return to Step 0 (re-plan and re-implement feature)

   - **If user input is not in current state's allowed options:**
     - Do not transition
     - Re-present current state and allowed options

**Gate Criteria:**
- [ ] Code + security critical issues = 0 before approval is enabled
- [ ] High issues fixed or approved with explicit mitigation justification
- [ ] Feature is complete vertical slice
- [ ] User decision recorded with rationale when required

---

### Step 4.5: TRACKER SYNC GATE (Mandatory)

**Execution Instructions:**

Before declaring feature completion, update and validate planning trackers:

1. Update feature and planning trackers:
   - `planning-mds/features/F{NNNN}-{slug}/STATUS.md` (feature completion state)
   - `planning-mds/features/REGISTRY.md` (status/path transitions, including archive moves)
   - `planning-mds/features/ROADMAP.md` (Now/Next/Later/Completed placement)
   - `planning-mds/BLUEPRINT.md` (feature/story status labels and links, if changed)

2. Regenerate story rollup when story files changed:
   - `python3 agents/product-manager/scripts/generate-story-index.py planning-mds/features/`

3. Validate consistency:
   - `python3 agents/product-manager/scripts/validate-trackers.py`

4. If validation fails:
   - Treat as a blocking issue
   - Fix tracker drift and re-run validation before completion

**Gate Criteria:**
- [ ] Feature STATUS reflects final approved state
- [ ] REGISTRY/ROADMAP/BLUEPRINT are synchronized
- [ ] STORY-INDEX regenerated if story files changed
- [ ] Tracker validation passes

---

### Step 5: Feature Complete

**Execution Instructions:**

Present completion summary:

```
═══════════════════════════════════════════════════════════
Feature Complete! ✓
═══════════════════════════════════════════════════════════

Feature: [Feature Name]

Application Assembly:
  ✓ Architect
    - Feature assembly plan created
    - Dependencies and checkpoints validated

Implementation:
  ✓ Backend Developer
    - [count] entities created/updated
    - [count] API endpoints implemented
    - [count] unit tests passing
    - [count] integration tests passing

  ✓ Frontend Developer
    - [count] components created
    - [count] screens implemented
    - [count] forms with validation
    - Component tests passing

  ✓ Quality Engineer
    - Test plan complete
    - [count] E2E tests passing
    - [percentage]% coverage for feature code

  ✓ DevOps
    - Feature deployability checks passed
    - Runtime configuration updates verified
    - Evidence paths recorded

  ✓ AI Engineer (if AI scope)
    - [count] AI workflows/prompts delivered
    - [count] AI tests passing

Code Review:
  ✓ Code Reviewer: APPROVED
  ✓ Vertical slice complete
  ✓ Acceptance criteria met
  Status: APPROVED

Security Review:
  ✓ Security Agent: PASS
  ✓ No critical vulnerabilities (high findings fixed or justified)
  ✓ Authorization and validation checks complete
  Status: PASS

═══════════════════════════════════════════════════════════
Next Steps:
═══════════════════════════════════════════════════════════

Feature is ready to:
1. Merge to main branch
2. Deploy to staging
3. Get stakeholder feedback

To continue building:
- Run "feature" action for next feature
- Run "build" action for remaining features
- Run "document" action to update docs

Feature delivered! ✓
═══════════════════════════════════════════════════════════
```

---

## Validation Criteria

**Overall Feature Action Success:**
- [ ] Feature assembly plan created and followed
- [ ] Feature is complete vertical slice (backend + frontend + tests + DevOps deployability checks + AI when in scope)
- [ ] All feature tests passing in application runtime containers
- [ ] AI tests passing (if AI scope) in AI runtime container
- [ ] Code review approved
- [ ] Security review approved
- [ ] All feature acceptance criteria met
- [ ] Feature can be deployed independently
- [ ] User decision recorded per gate rules
- [ ] Tracker sync gate passed (REGISTRY/ROADMAP/STORY-INDEX/BLUEPRINT/STATUS)

---

## Prerequisites

Before running feature action:
- [ ] Plan action completed for this feature
- [ ] Feature has clear user stories with acceptance criteria
- [ ] Feature scope is small (2-5 days of work)
- [ ] SOLUTION-PATTERNS.md exists
- [ ] Tracker governance contract available (`planning-mds/features/TRACKER-GOVERNANCE.md`)
- [ ] AI scope is explicit (if feature includes AI behavior)
- [ ] User is available for approval

---

## Vertical Slicing Best Practices

### What Makes a Good Vertical Slice?

1. **Complete:** Includes backend, frontend, tests, deployability checks, and AI layer changes when AI scope exists
2. **Deployable:** Can be released independently
3. **Testable:** Has clear acceptance criteria
4. **Small:** Can be completed in 2-5 days
5. **Valuable:** Delivers user value on its own

### Good Feature Examples
- ✅ "View customer list with pagination"
- ✅ "Create new order with required fields"
- ✅ "Search accounts by name"
- ✅ "View activity timeline for an account"

### Bad Feature Examples (Too Large - Break Down!)
- ❌ "Build entire customer management module"
- ❌ "Implement full order processing workflow"
- ❌ "Complete onboarding process"

### Not Vertical Slices
- ❌ "Create all database tables" (not vertical)
- ❌ "Build all backend APIs" (not vertical)
- ❌ "Create all forms" (not vertical)

---

## Example Usage

### Scenario 1: Customer List Feature
```
User: "Run the feature action for customer list with pagination"

Agent Runtime: [Reads feature.md]
Agent Runtime: "I'll implement the customer list feature as a vertical slice..."

[Step 0: Feature assembly planning]
  Architect:
    - Feature scope and dependencies mapped
    - Ownership assigned for backend/frontend/AI/test tasks

[Step 1: Parallel implementation]
  Backend Developer:
    - GET /api/customers endpoint with pagination
    - CustomerService.ListAsync method
    - Unit tests for service
    - Integration tests for endpoint

  Frontend Developer:
    - CustomerList component
    - TanStack Query hook for fetching customers
    - Pagination controls
    - Component tests

  Quality Engineer:
    - E2E test: View customer list
    - E2E test: Navigate pages
    - Test plan for customer list

  DevOps:
    - Container smoke test for feature runtime
    - Validate env var and compose changes
    - Record deployability evidence

[Step 2: Self-review - all pass]

[Step 3: Parallel reviews]
Agent Runtime: "Code and security reviews complete. Results:
  - Vertical slice: Complete
  - Issues: 2 medium (naming conventions)
  - Security findings: 0 critical, 0 high, 1 low
  - Test coverage: 85%
  - Acceptance criteria: All met
  - Recommendation: APPROVED WITH RECOMMENDATIONS"

[Step 4: Approval gate]
Agent Runtime: "Feature review complete. Do you approve? (approve/fix issues/reject)"

User: "approve"

Agent Runtime: "Feature complete! Customer list with pagination ready to merge."
```

### Scenario 2: Iteration After Rejection
```
User: "Run the feature action for customer search"

[Steps 0.5-3 execute]

[Step 4: Approval gate]
Agent Runtime: "Feature review results:
  - Issues: 1 critical (missing authorization check)
  - Recommendation: FIX CRITICAL"

User: "fix critical"

Agent Runtime: "Fixing authorization check..."
[Backend Developer adds authorization]

[Step 3: Re-review]
Agent Runtime: "Code and security reviews updated. Results:
  - Authorization: Fixed
  - Security status: PASS
  - Recommendation: APPROVED"

[Step 4: Re-approval]
Agent Runtime: "Do you approve now?"

User: "approve"

Agent Runtime: "Feature complete!"
```

---

## Related Actions

- **Before:** [plan action](./plan.md) - Plan the feature
- **Alternative:** [build action](./build.md) - Build multiple features together
- **After:** [document action](./document.md) - Document the feature
- **After:** [blog action](./blog.md) - Blog about the feature

---

## Notes

- Feature action is the **recommended way** to build incrementally
- Each feature should be merged to main after approval
- Features can build on each other (dependencies allowed)
- Prefer small, frequent features over large batches
- Feature action ensures true vertical slicing discipline
- Security review is part of the feature action (run `review` action separately for deeper audit scope when needed)
- DevOps deployability check is included; use `build` action for broader infra redesign across multiple features
- Critical findings block approval; high findings require explicit mitigation justification if approved
