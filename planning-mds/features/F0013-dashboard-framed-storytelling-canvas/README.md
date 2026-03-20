# F0013 — Dashboard Framed Storytelling Canvas

**Status:** Done (Decision A override; active folder retained)
**Priority:** High
**Phase:** MVP
**Corrects:** F0012 (archived), F0011 (abandoned), F0010 (abandoned)

## Overview

Restores the framed canvas identity that F0012 stripped by over-interpreting "infographic" as "flatten everything." Establishes a three-layer visual hierarchy: app chrome as the frame, operational panels (nudges, activity, tasks) with glass-card depth and glow, and the story canvas zone as the flat infographic area. Replaces the flat rectangular opportunity flow cells with a vertical timeline featuring contextual mini-visualizations at each stage node. Chapter controls use three modes (`Flow`, `Friction`, `Outcomes`) for uniform cross-stage emphasis.

## Documents

| Document | Purpose |
|----------|---------|
| [PRD.md](./PRD.md) | Full product requirements (why + what + how) |
| [STATUS.md](./STATUS.md) | Completion checklist and progress tracking |
| [GETTING-STARTED.md](./GETTING-STARTED.md) | Developer/agent setup guide |

## Stories

| ID | Title | Status |
|----|-------|--------|
| [F0013-S0000](./F0013-S0000-editorial-palette-refresh-dark-and-light-themes.md) | Editorial palette refresh — dark & light themes | Done (Decision A override; prior QE/Code findings retained) |
| [F0013-S0001](./F0013-S0001-restore-framed-canvas-identity-with-three-layer-visual-hierarchy.md) | Restore framed canvas identity with three-layer visual hierarchy | Done (Decision A override; prior QE/Code findings retained) |
| [F0013-S0002](./F0013-S0002-build-timeline-bar-with-connected-stage-nodes-and-terminal-branches.md) | Build vertical timeline with connected stage nodes and terminal outcome branches | Done (Decision A override; prior QE/Code findings retained) |
| [F0013-S0003](./F0013-S0003-add-radial-donut-chart-popovers-at-each-timeline-stage-node.md) | Add contextual mini-visualizations at each timeline stage node | Done (Decision A override; prior QE/Code findings retained) |
| [F0013-S0004](./F0013-S0004-connect-chapter-controls-to-radial-popover-data-layers.md) | Connect chapter controls as uniform override for timeline visualizations | Done (Decision A override; prior QE/Code findings retained) |
| [F0013-S0005](./F0013-S0005-ensure-responsive-accessibility-and-performance-parity.md) | Ensure responsive, accessibility, and performance parity | Done (Decision A override; prior QE/Code findings retained) |

**Total Stories:** 6
**Completed:** 6 / 6

## Closeout Note (2026-03-19)

F0013 remains `Done` only as the 2026-03-17 Decision A override (active folder retained). The doc/spec drift called out in the 2026-03-19 PM review has been remediated, and refreshed QE / Code Review / Security / DevOps evidence now exists for the current repo state. That rerun did not clear the feature for archive-ready closeout: QE is still non-pass, Code Review remains `REJECTED`, Security remains `CONDITIONAL PASS`, and DevOps is `FAIL`. PM final closeout re-review is still pending against those refreshed gate results. See `STATUS.md` plus `planning-mds/operations/evidence/f0013/{pm,qe,code-review,security,devops}-2026-03-19.md`.
