# Agent-Driven Builder Framework + Insurance CRM Reference

This repository serves two purposes:

1) A reusable, agent-driven development framework (the generic parts you can copy to any project).
2) A concrete, solution-specific example (an insurance CRM called Nebula) to demonstrate how the framework is used.

The separation is intentional. Generic agents live in `agents/`. Solution-specific planning artifacts live in `planning-mds/`.

## Release Scope (Initial Public Preview)

As of 2026-02-08, this repository is **published as a human-orchestrated framework** preview:

- Actions are executed by a human operator following `agents/actions/*.md`.
- There is no built-in automated orchestrator in this initial release.
- Automated orchestration integration (for example, SDK-based runners) is planned for a later phase after framework validation.

## Quick Orientation

- New project? Start with `blueprint-setup/README.md` and copy `agents/` into your repo.
- Exploring the insurance CRM example? Read `planning-mds/BLUEPRINT.md` and the example artifacts under `planning-mds/examples/`.
- Want the boundary rules? See `BOUNDARY-POLICY.md`.

## Framework Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Nebula Agent-Driven Builder Framework                    │
│                    Plan → Spec → Design → Build → Ship                      │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│  ACTION FLOW (User-Facing Compositions)                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  init       │ Bootstrap project structure                                   │
│  plan       │ Phase A (PM) → Phase B (Architect) [2 approval gates]         │
│  build      │ Backend + Frontend + AI* + QA + DevOps → Review [2 gates]     │
│  feature    │ Single vertical slice (Backend + Frontend + AI* + QA + DevOps) │
│  review     │ Code Reviewer + Security [1 gate]                             │
│  validate   │ Architect + PM validation (read-only)                         │
│  test       │ Quality Engineer testing workflow                             │
│  document   │ Technical Writer documentation                                │
│  blog       │ Blogger dev logs & articles                                   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
* AI Engineer runs when stories include AI/LLM/MCP scope. Architect owns implementation orchestration.
                                        ↓
                              Actions compose Agents
                                        ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│  AGENTS (Role-Based Specialists) - 11 Agents                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  Planning Phase (Phase A-B)                                                 │
│  ├─ product-manager    │ Requirements, stories, acceptance criteria         │
│  └─ architect          │ Design, data model, API contracts, patterns        │
│                                                                              │
│  Implementation Phase (Phase C)                                             │
│  ├─ backend-developer  │ C# APIs, EF Core, domain logic (engine/)           │
│  ├─ frontend-developer │ React, TypeScript, forms (experience/)             │
│  ├─ ai-engineer        │ Python LLMs, agents, MCP, workflows (neuron/) 🧠   │
│  ├─ quality-engineer   │ Unit, integration, E2E tests                       │
│  └─ devops             │ Docker, docker-compose, deployment                 │
│                                                                              │
│  Quality & Documentation                                                    │
│  ├─ code-reviewer      │ Code quality, standards, patterns                  │
│  ├─ security           │ OWASP, auth/authz, vulnerabilities                 │
│  ├─ technical-writer   │ API docs, README, runbooks                         │
│  └─ blogger            │ Dev logs, technical articles                       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
                                        ↓
                        Agents read from & write to
                                        ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│  SOLUTION-SPECIFIC CONTENT (planning-mds/)                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  Single Source of Truth                                                     │
│  └─ BLUEPRINT.md       │ Master specification (Sections 0-6)                │
│                                                                              │
│  Domain Knowledge                                                           │
│  └─ domain/            │ Glossary, competitive analysis                     │
│                                                                              │
│  Architecture                                                               │
│  ├─ architecture/                                                           │
│  │  ├─ SOLUTION-PATTERNS.md  │ Solution-specific patterns ⭐               │
│  │  ├─ decisions/            │ ADRs                                         │
│  │  └─ ...                   │ Data model docs, testing strategy, patterns  │
│                                                                              │
│  API Contracts                                                              │
│  └─ api/               │ OpenAPI specifications (*.yaml)                    │
│                                                                              │
│  Examples & Artifacts                                                       │
│  ├─ examples/          │ Personas, features, stories, screens               │
│  ├─ security/          │ Threat models, security reviews                    │
│  └─ ...                                                                      │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘

─────────────────────────────────────────────────────────────────────────────

  9 Actions · 11 Agents · 1 Source of Truth (BLUEPRINT.md)
  SOLUTION-PATTERNS.md for institutional knowledge
  neuron/ for AI intelligence layer 🧠
```

## Repository Layout (By Intent)

- `agents/` - Generic, reusable agent roles, templates, and references. Copy as-is.
- `planning-mds/` - Solution-specific requirements, examples, and decisions (Nebula CRM in this repo). Replace for a new project.
- `blueprint-setup/` - Bootstrap guidance for starting a new project.
- `engine/` - Backend application layer (C# .NET APIs) - currently placeholder.
- `experience/` - Frontend application layer (React UI) - currently placeholder.
- `neuron/` - AI intelligence layer (Python LLMs, agents, MCP) 🧠 - directory structure created.
- `docker/agent-builder/` - Container entrypoint/runtime helpers for the builder framework.
- `docs/` - Meta documentation and audits.

## Reuse Workflow (New Project)

1) Copy `agents/` into your new repo unchanged.
2) Create a fresh `planning-mds/` in your new repo.
3) Populate `planning-mds/` with your domain glossary, requirements, and examples.
4) Use the agent roles to produce outputs into `planning-mds/` and then implement code in your project.

This keeps the framework reusable and the solution content replaceable.

## The Example (Nebula Insurance CRM)

Everything under `planning-mds/` in this repo is specific to the Nebula insurance CRM. Treat it as a reference example only.
When you start a new project, replace all `planning-mds/` content with your own domain knowledge and requirements.
The actual application code should be generated from planning artifacts by implementation agents coordinated by Architect.

## Run Framework In Docker

Use the root `Dockerfile` to run the agent-builder framework in a reproducible container:

```bash
docker build -t nebula-agent-builder .
docker run --rm -it -v "$PWD:/workspace" -w /workspace nebula-agent-builder bash
```

Or use compose:

```bash
docker compose -f docker-compose.agent-builder.yml run --rm agent-builder
```

Security note:
- The compose workflow mounts your workspace read-write for local development.
- Use selective or read-only mounts in shared/production-like environments.
- The container runs as a non-root user by default.
- The builder container is orchestration-focused and stack-agnostic; run stack-specific compile/test/security in application runtime containers.

See `docs/CONTAINER-STRATEGY.md` for builder vs application runtime separation.

## Tech Stack Assumptions

The framework is opinionated about delivery practices and provides stack-specific references in some agent guides.
In this repo, the default references assume a modern .NET + React + PostgreSQL stack. If you adopt a different stack,
keep the agent roles but replace the stack-specific reference guides and examples with your own.
Keep the builder base runtime generic; put stack SDKs and execution tooling in the generated application runtime containers.

## Key Documents

- `agents/README.md` - How to use the generic agents.
- `planning-mds/README.md` - What belongs in solution-specific planning.
- `BOUNDARY-POLICY.md` - Rules that separate generic vs solution-specific content.
- `blueprint-setup/README.md` - Bootstrap steps for a new project.
- `docs/FAQ.md` - Common questions about reuse, stacks, and boundaries.
- `docs/CONTAINER-STRATEGY.md` - Two-container model (builder runtime vs application runtime).
- `docs/ORCHESTRATION-CONTRACT.md` - Orchestrator-neutral execution contract.
- `docs/MANUAL-ORCHESTRATION-RUNBOOK.md` - Required manual execution/evidence flow for the preview release.
- `docs/PREVIEW-RELEASE-CHECKLIST.md` - Definition of done for the initial public preview.
- `lifecycle-stage.yaml` - Lifecycle stage declaration + required gate matrix.
- `scripts/run-lifecycle-gates.py` - Stage-aware gate runner used locally and in CI.

## Lifecycle Gates

Gates are activated by lifecycle stage, not by ad-hoc command selection.

```bash
python3 scripts/run-lifecycle-gates.py --list
python3 scripts/run-lifecycle-gates.py
```

CI runs the same command and therefore validates the gates required by `current_stage` only.
A green CI run is not equivalent to `implementation` or `release-readiness` gate completion.

Update `lifecycle-stage.yaml` when moving from bootstrap/planning to implementation/release stages.

## Why This Exists

The goal is to prove out AI-agentic driven development in a reusable way, while also demonstrating the approach with a
real, end-to-end example (insurance CRM). This repo intentionally contains both; the boundary is the key.

## Framework Posture

This repository is a reference framework (specifications, templates, role definitions, and action contracts).
It is orchestrator-agnostic and model-agnostic:

- You can execute it with any agent runtime that follows `docs/ORCHESTRATION-CONTRACT.md`.
- Action files define composition patterns; your orchestrator maps user intents to those actions.
- This repository does not enforce a single vendor-specific orchestration runtime.
- Initial preview mode is human-orchestrated; use `docs/MANUAL-ORCHESTRATION-RUNBOOK.md`.
- Automated orchestrator integrations are future work after framework validation.

## Boundary Policy (Short Version)

- `agents/` is generic and reusable across projects.
- `planning-mds/` is solution-specific and should be replaced for each new project.
- Agents read from `planning-mds/` but must not embed solution-specific requirements.
- Templates and reusable examples live under `agents/templates/` and `agents/**/references/`.
- Domain knowledge, examples, and decisions live under `planning-mds/`.

See `BOUNDARY-POLICY.md` for the full policy.
