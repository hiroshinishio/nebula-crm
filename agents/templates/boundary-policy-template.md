# Boundary Policy: Generic Agents vs Solution-Specific Content

**Date:** YYYY-MM-DD
**Status:** Active

---

## Purpose

This policy defines the boundary between generic, reusable agent roles (`agents/`) and solution-specific content (`planning-mds/`).

---

## Policy Rules

### Rule 1: agents/ is Generic and Reusable

**Principle:** Everything in `agents/` must be applicable to **any** software project, regardless of domain.

**What belongs in agents/:**
- Agent role definitions (SKILL.md and supporting references/scripts/assets)
- Generic best practices (SOLID, DDD, INVEST, vertical slicing, etc.)
- Generic examples from multiple domains (B2B SaaS, e-commerce, healthcare, etc.)
- Generic templates (story, feature, persona, ADR, API contract, etc.)
- Generic scripts and tools (validation, linting, formatting)
- Framework documentation (`agents/docs/`)

**What does NOT belong in agents/:**
- Domain-specific terminology
- Competitive analysis for specific markets
- Solution-specific examples (personas, features, stories referencing project entities)
- Solution-specific architecture patterns
- Project-specific business rules or workflows

---

### Rule 2: planning-mds/ is Solution-Specific

**Principle:** Everything in `planning-mds/` is specific to the current project and would be replaced for a new project.

**What belongs in planning-mds/:**
- Project master specification (BLUEPRINT.md)
- Domain knowledge (glossary, competitive analysis, domain-specific patterns)
- Project-specific examples (personas, features, stories, architecture)
- Actual project requirements (features/, stories/, architecture/)
- Project-specific ADRs and design decisions

**What does NOT belong in planning-mds/:**
- Generic best practices (those go in agents/)
- Generic examples from other domains
- Reusable templates (those go in agents/templates/)

---

### Rule 3: Agents Must Not Invent Requirements

**Principle:** Agent roles consume requirements from `planning-mds/`; they do not create or embed solution requirements.

**Implementation:**
- Agents read from `planning-mds/BLUEPRINT.md` and `planning-mds/domain/` to understand project context
- Agents reference `planning-mds/examples/` to see how generic patterns apply to this project
- Agents generate deliverables based on templates in `agents/templates/` and requirements in `planning-mds/`
- Agents never hard-code project-specific business logic in their role definitions

---

### Rule 4: Starting a New Project

**Principle:** Reusing agents for a new project should be as simple as copying `agents/` and running the `init` action.

**Process:**
1. Copy entire `agents/` directory to new project
2. Run the `init` action to scaffold root-level framework files and `planning-mds/` structure
3. Write new domain knowledge in `planning-mds/domain/`
4. Create new project-specific examples in `planning-mds/examples/`
5. Write new BLUEPRINT.md for the new project
6. Agents are immediately ready to use with new project context

---

### Rule 5: Script Placement Boundary

**Principle:** Framework-generic automation must live under `agents/`; repository-root `scripts/` is solution/runtime scoped.

**Placement rules:**
- Put reusable governance and validation scripts under `agents/**/scripts/` or `agents/scripts/`
- Keep role-specific generic validators with their role (`agents/<role>/scripts/`)
- Do not keep framework-generic scripts in root `scripts/`
- Use root `scripts/` only for solution deployment/runtime/project-specific automation

---

## Standard Example Entities

All reference files in `agents/` that include code or API examples must use the same set of example entities. This prevents inconsistency across files and accidental re-introduction of solution-specific entities.

### The Standard Set

| Entity | Role in examples |
|--------|-----------------|
| `customers` | Primary entity - CRUD, search, filtering |
| `orders` | Child entity - nested resources, status workflows |

### Field Mapping

| Field type | Use | Example |
|------------|-----|---------|
| Name / primary text | `name` | `"Acme Inc"` |
| Unique identifier (business key) | `orderNumber` | `"ORD-12345"` |
| Status (enumerated) | `status` | `Active`, `Pending`, `Shipped` |
| Email | `email` | `"contact@acme.example.com"` |
| Numeric (for range filters) | `amount` | `500.00` |
| Date (for range filters) | `orderDate` | `"2026-06-15"` |

### Header Note

Every reference file that uses examples must include this note at the top, immediately after the `# Title`:

```markdown
> **Examples in this guide use `customers` and `orders` as illustrative entities.
> These are not prescriptive - substitute your own domain entities when applying
> these patterns. See `BOUNDARY-POLICY.md` for the full convention and field mapping.
```

---

## Enforcement

### Code Review Checklist

When reviewing PRs that modify `agents/`:
- [ ] No project-specific terminology in agent files
- [ ] Examples use the standard entity set (`customers` / `orders`)
- [ ] No hard-coded business rules or domain logic
- [ ] All project-specific content belongs in `planning-mds/`

When reviewing PRs that modify `planning-mds/`:
- [ ] Content is specific to current project
- [ ] No generic best practices (those belong in `agents/`)
- [ ] References to `agents/` resources are correct
