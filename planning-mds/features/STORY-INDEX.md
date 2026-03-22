# User Story Index

Auto-generated index of all user stories across feature folders.

**Total Stories:** 56

---

## F0001 — Dashboard

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0001-S0001](./archive/F0001-dashboard/F0001-S0001-view-key-metrics-cards.md) | View Key Metrics Cards | High | MVP | Distribution User or Relationship Manager |
| [F0001-S0002](./archive/F0001-dashboard/F0001-S0002-view-pipeline-summary.md) | View Pipeline Summary (Sankey Opportunities) | High | MVP | Distribution User or Underwriter |
| [F0001-S0003](./archive/F0001-dashboard/F0001-S0003-view-my-tasks-and-reminders.md) | View My Tasks | High | MVP | Distribution User, Underwriter, or Relationship Manager |
| [F0001-S0004](./archive/F0001-dashboard/F0001-S0004-view-broker-activity-feed.md) | View Broker Activity Feed | High | MVP | Relationship Manager or Distribution User |
| [F0001-S0005](./archive/F0001-dashboard/F0001-S0005-view-nudge-cards.md) | View and Dismiss Nudge Cards | High | MVP | Distribution User, Underwriter, or Relationship Manager |

---

## F0002 — Broker & MGA Relationship Management

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0002-S0001](./archive/F0002-broker-relationship-management/F0002-S0001-create-broker.md) | Create a new broker record | Critical | MVP | Distribution Manager |
| [F0002-S0002](./archive/F0002-broker-relationship-management/F0002-S0002-search-brokers.md) | Search brokers by name or license number | High | MVP | Distribution Manager |
| [F0002-S0003](./archive/F0002-broker-relationship-management/F0002-S0003-read-broker.md) | View broker details in Broker 360 | High | MVP | Distribution Manager |
| [F0002-S0004](./archive/F0002-broker-relationship-management/F0002-S0004-update-broker.md) | Update broker profile information | High | MVP | Distribution Manager |
| [F0002-S0005](./archive/F0002-broker-relationship-management/F0002-S0005-delete-broker.md) | Deactivate (soft delete) a broker | Medium | MVP | Distribution User |
| [F0002-S0006](./archive/F0002-broker-relationship-management/F0002-S0006-manage-broker-contacts.md) | Create, update, and remove broker contacts | High | MVP | Relationship Manager |
| [F0002-S0007](./archive/F0002-broker-relationship-management/F0002-S0007-view-broker-activity-timeline.md) | View broker activity timeline in Broker 360 | High | MVP | Relationship Manager or Distribution Manager |
| [F0002-S0008](./archive/F0002-broker-relationship-management/F0002-S0008-reactivate-broker.md) | Reactivate a deactivated broker | Medium | MVP | Distribution Manager or Admin |
| [F0002-S0009](./archive/F0002-broker-relationship-management/F0002-S0009-adopt-native-casbin-enforcer.md) | Replace custom authorization parser with native Casbin enforcer | Critical | MVP Hardening | Platform Security Engineer |

---

## F0003 — Task Center + Reminders (API-only MVP)

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0003-S0001](./archive/F0003-task-center/F0003-S0001-create-task.md) | Create a task (self-assigned) | High | MVP | Distribution User, Underwriter, Relationship Manager, Program Manager, Distribution Manager, or Admin |
| [F0003-S0002](./archive/F0003-task-center/F0003-S0002-update-task.md) | Update a task (self-assigned) | High | MVP | Distribution User, Underwriter, Relationship Manager, Program Manager, Distribution Manager, or Admin |
| [F0003-S0003](./archive/F0003-task-center/F0003-S0003-delete-task.md) | Soft delete a task (self-assigned) | Medium | MVP | Distribution User, Underwriter, Relationship Manager, Program Manager, Distribution Manager, or Admin |

---

## F0005 — IdP Migration

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0005-S0001](./archive/F0005-idp-migration/F0005-S0001-replace-authentik-infra.md) | F0005-S0001 — Replace authentik Infrastructure (docker-compose + Bootstrap) | Must-complete before any backend story | - | - |
| [F0005-S0002](./archive/F0005-idp-migration/F0005-S0002-claims-normalization-backend.md) | F0005-S0002 — Claims Normalization Layer + Principal Key (Backend) | Must-complete before F0001/F0002 backend implementation | - | - |
| [F0005-S0003](./archive/F0005-idp-migration/F0005-S0003-frontend-oidc-flow.md) | F0005-S0003 — Frontend OIDC Flow Update | Required before real login flow is implemented; dev-auth.ts fix is immediate | - | - |
| [F0005-S0004](./archive/F0005-idp-migration/F0005-S0004-principal-key-data-model.md) | F0005-S0004 — Data Model Principal Key Rename | Must-complete before F0001/F0002 entity implementation | - | - |

---

## F0009 — Authentication + Role-Based Login

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0009-S0001](./archive/F0009-authentication-and-role-based-login/F0009-S0001-login-screen-and-oidc-redirect.md) | Provide login entry screen and IdP sign-in redirect | Critical | Phase 1 | Nebula user |
| [F0009-S0002](./archive/F0009-authentication-and-role-based-login/F0009-S0002-oidc-callback-and-session-bootstrap.md) | Establish session from OIDC callback and bootstrap user context | Critical | Phase 1 | authenticated Nebula user |
| [F0009-S0003](./archive/F0009-authentication-and-role-based-login/F0009-S0003-role-based-entry-and-protected-navigation.md) | Route users to role-appropriate entry points and enforce protected navigation | Critical | Phase 1 | signed-in user |
| [F0009-S0004](./archive/F0009-authentication-and-role-based-login/F0009-S0004-broker-user-access-boundaries.md) | Define and enforce BrokerUser access boundaries | Critical | Phase 1 | broker user |
| [F0009-S0005](./archive/F0009-authentication-and-role-based-login/F0009-S0005-seeded-user-access-validation-matrix.md) | Provide seeded user identities and validate role-specific login outcomes | High | Phase 1 | QA or reviewer |

---

## F0010 — Dashboard Opportunities Refactor (Pipeline Board + Insight Views)

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0010-S0001](./archive/F0010-dashboard-opportunities-refactor/F0010-S0001-replace-sankey-with-pipeline-board-default.md) | Replace Sankey default with Pipeline Board | High | MVP | Distribution User or Underwriter |
| [F0010-S0002](./archive/F0010-dashboard-opportunities-refactor/F0010-S0002-add-opportunity-aging-heatmap-view.md) | Add Opportunities Aging Heatmap view | High | MVP | Distribution User or Underwriter |
| [F0010-S0003](./archive/F0010-dashboard-opportunities-refactor/F0010-S0003-add-opportunity-composition-treemap-view.md) | Add Opportunities Composition Treemap view | Medium | MVP | Relationship Manager or Program Manager |
| [F0010-S0004](./archive/F0010-dashboard-opportunities-refactor/F0010-S0004-add-opportunity-hierarchy-sunburst-view.md) | Add Opportunities Hierarchy Sunburst view | Medium | MVP | Distribution Manager or Program Manager |
| [F0010-S0005](./archive/F0010-dashboard-opportunities-refactor/F0010-S0005-unify-drilldown-responsive-and-accessibility.md) | Unify drilldown, responsive layout, and accessibility across opportunities views | High | MVP | dashboard user on desktop, tablet, or phone |

---

## F0011 — Dashboard Opportunities Flow-First Modernization (Connected Pipeline + Terminal Outcomes)

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0011-S0001](./archive/F0011-dashboard-opportunities-flow-modernization/F0011-S0001-replace-pipeline-board-with-connected-flow-default.md) | Replace Pipeline Board tiles with connected flow-first canvas default | High | MVP | Distribution User or Underwriter |
| [F0011-S0002](./archive/F0011-dashboard-opportunities-flow-modernization/F0011-S0002-add-terminal-outcomes-rail-and-drilldowns.md) | Add terminal outcomes rail and outcome drilldowns | High | MVP | Distribution Manager or Underwriter |
| [F0011-S0003](./archive/F0011-dashboard-opportunities-flow-modernization/F0011-S0003-apply-modern-opportunities-visual-system.md) | Apply modern opportunities visual system (dark depth + stage emphasis) | Medium | MVP | dashboard user |
| [F0011-S0004](./archive/F0011-dashboard-opportunities-flow-modernization/F0011-S0004-rebalance-secondary-insights-as-mini-views.md) | Rebalance secondary insights as mini-views | Medium | MVP | Relationship Manager or Program Manager |
| [F0011-S0005](./archive/F0011-dashboard-opportunities-flow-modernization/F0011-S0005-ensure-responsive-and-accessibility-parity.md) | Ensure responsive and accessibility parity for new opportunities flow | High | MVP | dashboard user on desktop, tablet, or phone |

---

## F0012 — Dashboard Storytelling Infographic Canvas (Flat Canvas + Collapsible Rails)

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0012-S0001](./archive/F0012-dashboard-storytelling-infographic-canvas/F0012-S0001-unify-kpi-and-opportunities-into-single-story-canvas.md) | Unify nudge bar, KPI band, and connected opportunity flow into one flat infographic canvas | High | MVP | Distribution User or Underwriter |
| [F0012-S0002](./archive/F0012-dashboard-storytelling-infographic-canvas/F0012-S0002-build-interactive-opportunities-story-chapters-and-overlays.md) | Add interactive story chapters and in-canvas analytical overlays | High | MVP | Relationship Manager or Program Manager |
| [F0012-S0003](./archive/F0012-dashboard-storytelling-infographic-canvas/F0012-S0003-reflow-dashboard-layout-with-activity-and-tasks-below-canvas.md) | Flow Activity and My Tasks as flat canvas sections below story content | Medium | MVP | dashboard user |
| [F0012-S0004](./archive/F0012-dashboard-storytelling-infographic-canvas/F0012-S0004-support-collapsible-nav-and-neuron-rails-with-adaptive-canvas-width.md) | Preserve collapsible left nav and right Neuron rail with adaptive canvas width | High | MVP | dashboard user |
| [F0012-S0005](./archive/F0012-dashboard-storytelling-infographic-canvas/F0012-S0005-ensure-responsive-accessibility-and-performance-parity-for-story-canvas.md) | Ensure responsive, accessibility, and performance parity for storytelling dashboard | High | MVP | dashboard user on desktop, tablet, or phone |

---

## F0013 — Dashboard Framed Storytelling Canvas

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0013-S0000](./archive/F0013-dashboard-framed-storytelling-canvas/F0013-S0000-editorial-palette-refresh-dark-and-light-themes.md) | Editorial palette refresh — dark & light themes | Critical | MVP | dashboard user |
| [F0013-S0001](./archive/F0013-dashboard-framed-storytelling-canvas/F0013-S0001-restore-framed-canvas-identity-with-three-layer-visual-hierarchy.md) | Restore framed canvas identity with three-layer visual hierarchy | Critical | MVP | dashboard user |
| [F0013-S0002](./archive/F0013-dashboard-framed-storytelling-canvas/F0013-S0002-build-timeline-bar-with-connected-stage-nodes-and-terminal-branches.md) | Build vertical timeline with connected stage nodes and terminal outcome branches | High | MVP | dashboard user |
| [F0013-S0003](./archive/F0013-dashboard-framed-storytelling-canvas/F0013-S0003-add-radial-donut-chart-popovers-at-each-timeline-stage-node.md) | Add contextual mini-visualizations at each timeline stage node | High | MVP | dashboard user |
| [F0013-S0004](./archive/F0013-dashboard-framed-storytelling-canvas/F0013-S0004-connect-chapter-controls-to-radial-popover-data-layers.md) | Connect chapter controls as uniform override for timeline visualizations | High | MVP | dashboard user |
| [F0013-S0005](./archive/F0013-dashboard-framed-storytelling-canvas/F0013-S0005-ensure-responsive-accessibility-and-performance-parity.md) | Ensure responsive, accessibility, and performance parity for framed storytelling canvas | Medium | MVP | dashboard user on any device or using assistive technology |

---

## F0004 — Task Center UI + Manager Assignment

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0004-S0001](./F0004-task-center-ui-and-assignment/F0004-S0001-task-list-api-endpoint.md) | Paginated task list API with filters and views | Critical | Phase 1 | Distribution User, Underwriter, Relationship Manager, Program Manager, Distribution Manager, or Admin |
| [F0004-S0002](./F0004-task-center-ui-and-assignment/F0004-S0002-user-search-api-endpoint.md) | User search API for assignee picker | High | Phase 1 | Distribution Manager or Admin |
| [F0004-S0003](./F0004-task-center-ui-and-assignment/F0004-S0003-cross-user-task-authorization.md) | Cross-user task authorization for assign, reassign, and creator-based access | Critical | Phase 1 | Distribution Manager or Admin |
| [F0004-S0004](./F0004-task-center-ui-and-assignment/F0004-S0004-task-center-list-and-filter-ui.md) | Task Center list view with tabs, filters, sort, and pagination | Critical | Phase 1 | Distribution User, Underwriter, Relationship Manager, Program Manager, Distribution Manager, or Admin |
| [F0004-S0005](./F0004-task-center-ui-and-assignment/F0004-S0005-task-create-edit-ui-with-assignment.md) | Task create and edit UI with assignee picker for managers | High | Phase 1 | Distribution Manager or Admin |
| [F0004-S0006](./F0004-task-center-ui-and-assignment/F0004-S0006-task-detail-panel-and-mobile-view.md) | Task detail side panel and mobile detail view | High | Phase 1 | Distribution User, Underwriter, Relationship Manager, Program Manager, Distribution Manager, or Admin |

---

## F0015 — Frontend Quality Gates + Test Infrastructure

| Story ID | Title | Priority | Phase | Persona |
|----------|-------|----------|-------|---------|
| [F0015-S0001](./archive/F0015-frontend-quality-gates-and-test-infrastructure/F0015-S0001-establish-frontend-test-infrastructure-and-commands.md) | Establish frontend test infrastructure and commands | Critical | Infrastructure | frontend engineer |
| [F0015-S0002](./archive/F0015-frontend-quality-gates-and-test-infrastructure/F0015-S0002-activate-nebula-frontend-quality-gates-and-evidence.md) | Activate Nebula frontend quality gates and evidence | Critical | Infrastructure | release approver |
| [F0015-S0003](./archive/F0015-frontend-quality-gates-and-test-infrastructure/F0015-S0003-backfill-critical-frontend-coverage-and-record-full-validation-run.md) | Backfill critical frontend coverage and record one full validation run | High | Infrastructure | quality engineer |

---

## Summary by Phase

| Phase | Count |
|-------|-------|
| Infrastructure | 3 |
| MVP | 37 |
| MVP Hardening | 1 |
| Phase 1 | 11 |
| Unspecified | 4 |

---

## Summary by Priority

| Priority | Count |
|----------|-------|
| Critical | 13 |
| High | 30 |
| Medium | 9 |

---

*Generated by generate-story-index.py*