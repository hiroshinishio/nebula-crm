# Repository Architecture (Conceptual)

This diagram shows how the repository is structured and how the reusable framework relates to solution-specific artifacts.

```
┌─────────────────────────────────────────────────────────┐
│                  REPOSITORY STRUCTURE                    │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────────┐     ┌──────────────────────────┐   │
│  │   agents/        │     │    planning-mds/         │   │
│  │  (GENERIC)       │────▶│  (SOLUTION-SPECIFIC)     │   │
│  │                  │     │                          │   │
│  │ • 11 Roles       │     │ • Project Specs          │   │
│  │ • Templates      │     │ • Domain Knowledge       │   │
│  │ • References     │     │ • Features/Stories       │   │
│  │ • Framework Docs │     │                          │   │
│  └──────────────────┘     └──────────────────────────┘   │
│         │                            │                  │
│         │  init action scaffolds     ▼                  │
│         │  root-level files   ┌──────────────────────┐   │
│         └────────────────────▶│ engine/experience/   │   │
│                               │ neuron/              │   │
│                               │   (IMPLEMENTATION)   │   │
│                               └──────────────────────┘   │
│                                                          │
└─────────────────────────────────────────────────────────┘

COPY agents/ ─────┐
   (for new       │    then run init action
    projects)     │    to scaffold the rest
                  ▼
              Your New
              Project
```

## Notes

- `agents/` is the self-contained, reusable framework. Copy it as-is to start a new project.
- The `init` action scaffolds root-level framework files (`lifecycle-stage.yaml`, `BOUNDARY-POLICY.md`, `CONTRIBUTING.md`, CI workflow) and the `planning-mds/` structure.
- `planning-mds/` is replaced for each new project.
- `engine/experience/neuron/` are the implementation layers for the current solution.
- Architect orchestrates app assembly from planning artifacts into implementation layers.
