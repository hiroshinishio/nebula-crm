# Action: Init

## User Intent

Bootstrap a new project with the proper directory structure, blueprint document template, and initial planning artifacts.

## Agent Flow

```
Product Manager (initialization mode)
```

**Flow Type:** Single agent

## Prerequisites

- [ ] Empty or new repository
- [ ] User has basic project context (domain, goals, target users)

## Inputs

### Required
- Project name
- Domain description (1-2 sentences)
- Target users (list of roles)
- Core entities (initial baseline list)

### Optional
- Tech stack preferences (defaults to .NET + React + PostgreSQL)
- Phase scope (MVP features to include/exclude)

## Outputs

### Root-Level Framework Files
```
lifecycle-stage.yaml          # Lifecycle stage declaration (from agents/templates/lifecycle-stage-template.yaml)
BOUNDARY-POLICY.md            # Generic/solution separation rules (from agents/templates/boundary-policy-template.md)
CONTRIBUTING.md               # Framework contribution guidelines (from agents/templates/contributing-template.md)
.github/workflows/
  ci-gates.yml                # Starter CI workflow (from agents/templates/ci-gates-template.yml)
```

### Planning Directory Structure
```
planning-mds/
├── BLUEPRINT.md              # Master specification (template populated)
├── README.md                 # Planning directory overview
├── domain/
│   └── glossary.md          # Domain-specific terminology (skeleton)
├── features/
│   ├── REGISTRY.md          # Feature number tracker + index
│   ├── ROADMAP.md           # Sequencing view (Now / Next / Later / Completed)
│   ├── STORY-INDEX.md       # Story rollup (generated)
│   ├── TRACKER-GOVERNANCE.md # Tracker synchronization contract
│   └── archive/             # (empty, completed features move here)
├── examples/
│   ├── personas/            # (empty, ready for PM)
│   ├── features/            # (empty, ready for PM)
│   └── stories/             # (empty, ready for PM)
├── architecture/
│   ├── SOLUTION-PATTERNS.md # Solution conventions for all implementation agents
│   └── decisions/           # (empty, ready for Architect)
└── security/                # (empty, ready for Security)
```

### Populated Files
- **`lifecycle-stage.yaml`** - Seeded from `agents/templates/lifecycle-stage-template.yaml` with `current_stage: framework-bootstrap`
- **`BOUNDARY-POLICY.md`** - Seeded from `agents/templates/boundary-policy-template.md`
- **`CONTRIBUTING.md`** - Seeded from `agents/templates/contributing-template.md`
- **`.github/workflows/ci-gates.yml`** - Seeded from `agents/templates/ci-gates-template.yml`
- **`planning-mds/BLUEPRINT.md`** - Sections 0-2 filled with user inputs, sections 3-6 as TODOs
- **`planning-mds/README.md`** - Overview of planning artifacts and how to use them
- **`planning-mds/domain/glossary.md`** - Domain glossary skeleton ready for population
- **`planning-mds/features/TRACKER-GOVERNANCE.md`** - Copied from `agents/templates/tracker-governance-template.md`
- **`planning-mds/architecture/SOLUTION-PATTERNS.md`** - Scaffolded from `agents/templates/solution-patterns-template.md`

## Agent Responsibilities

### Product Manager (Init Mode)
1. Interview user to gather required inputs (if not provided)
2. Scaffold root-level framework files (skip if they already exist):
   - Copy `agents/templates/lifecycle-stage-template.yaml` to `lifecycle-stage.yaml`
   - Copy `agents/templates/boundary-policy-template.md` to `BOUNDARY-POLICY.md`
   - Copy `agents/templates/contributing-template.md` to `CONTRIBUTING.md`
   - Copy `agents/templates/ci-gates-template.yml` to `.github/workflows/ci-gates.yml`
3. Create `planning-mds/` directory structure
4. Populate `BLUEPRINT.md` template with baseline information:
   - Section 0: Process and roles
   - Section 1: Product context (name, domain, purpose, users, entities, workflows)
   - Section 2: Technology baseline (if specified)
   - Sections 3-6: Marked as TODO with clear instructions
5. Create domain glossary skeleton
6. Initialize feature trackers:
   - Create `planning-mds/features/REGISTRY.md`
   - Create `planning-mds/features/ROADMAP.md`
   - Create `planning-mds/features/STORY-INDEX.md` (placeholder; regenerated when stories are added)
   - Copy `agents/templates/tracker-governance-template.md` to `planning-mds/features/TRACKER-GOVERNANCE.md`
7. Copy `agents/templates/solution-patterns-template.md` to `planning-mds/architecture/SOLUTION-PATTERNS.md`
8. Validate that all required inputs are captured

## Validation Criteria

- [ ] `lifecycle-stage.yaml` exists at repo root
- [ ] `BOUNDARY-POLICY.md` exists at repo root
- [ ] `CONTRIBUTING.md` exists at repo root
- [ ] `.github/workflows/ci-gates.yml` exists
- [ ] `planning-mds/BLUEPRINT.md` exists with Sections 0-2 populated
- [ ] Directory structure matches template
- [ ] Domain glossary skeleton created
- [ ] `planning-mds/features/TRACKER-GOVERNANCE.md` exists
- [ ] `planning-mds/architecture/SOLUTION-PATTERNS.md` exists
- [ ] No placeholder text remains (or is clearly marked as TODO)
- [ ] User can immediately proceed to Phase A (planning) or Phase B (architecture)

## Example Usage

### Scenario 1: B2B SaaS Platform
```
User: "Initialize a new B2B SaaS project called TeamSync"

Init Action:
  ↓
Product Manager prompts:
  - Domain: "Team collaboration and project management"
  - Users: "Project managers, team leads, administrators"
  - Core entities: "Project, Task, Team, Member, Activity"
  ↓
Creates planning-mds/ with populated BLUEPRINT.md
```

### Scenario 2: E-commerce Platform
```
User: "Bootstrap an e-commerce project called ShopFlow"

Init Action:
  ↓
Product Manager prompts:
  - Domain: "B2C E-commerce"
  - Users: "Customers, store administrators, fulfillment staff"
  - Core entities: "Product, Order, Cart, Customer, Inventory"
  ↓
Creates planning-mds/ with populated BLUEPRINT.md
```

## Post-Initialization Next Steps

After running init action:
1. Review `planning-mds/BLUEPRINT.md` sections 0-2
2. Refine domain glossary in `planning-mds/domain/glossary.md`
3. Ready to run **[plan action](./plan.md)** for Phase A + B

## Related Actions

- **Next:** [plan action](./plan.md) - Complete requirements and architecture
- **Alternative:** Manually populate `planning-mds/BLUEPRINT.md` if you prefer full control

## Notes

- Init action is idempotent - safe to run on existing projects (will skip existing files)
- If `planning-mds/BLUEPRINT.md` already exists, init will validate structure only
- User can always manually edit files after init completes
