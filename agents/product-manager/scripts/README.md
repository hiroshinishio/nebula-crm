# Product Manager Scripts

Generic scripts for validating stories and generating indexes.

Story convention:
- One story per markdown file.

## validate-stories.py

Validate one or more story files:

```bash
python agents/product-manager/scripts/validate-stories.py planning-mds/features/F0001-dashboard/F0001-S0001-view-kpis.md
python agents/product-manager/scripts/validate-stories.py planning-mds/features/F0001-dashboard/F0001-S0001-view-kpis.md planning-mds/features/F0001-dashboard/F0001-S0002-view-pipeline.md
```

Validate a directory of stories:

```bash
python agents/product-manager/scripts/validate-stories.py planning-mds/features/
```

## generate-story-index.py

Generate a story index for a directory:

```bash
python agents/product-manager/scripts/generate-story-index.py planning-mds/features/
```

Outputs `planning-mds/features/STORY-INDEX.md`.

## validate-trackers.py

Validate tracker consistency across `REGISTRY.md`, `ROADMAP.md`, `STORY-INDEX.md`, and `BLUEPRINT.md`:

```bash
python3 agents/product-manager/scripts/validate-trackers.py
python3 agents/product-manager/scripts/validate-trackers.py --features-dir planning-mds/features --blueprint planning-mds/BLUEPRINT.md
```
