# F0001-S0002: View Pipeline Summary (Mini-Kanban)

**Story ID:** F0001-S0002
**Feature:** F0001 — Dashboard
**Title:** View Pipeline Summary (Mini-Kanban)
**Priority:** High
**Phase:** MVP

## User Story

**As a** Distribution User or Underwriter
**I want** to see submission and renewal pipelines as horizontal status columns with counts and expandable card previews
**So that** I can identify bottlenecks at a glance and quickly drill into specific items without leaving the dashboard.

## Context & Background

Pipeline visibility is critical for distribution and underwriting teams. A traditional table of counts shows volume but lacks context. Inspired by Kanban-style boards (e.g., Copper Opportunities), this widget uses a mini-Kanban layout: horizontal status pills with counts that reveal a short card preview on hover or click. This gives users both the high-level funnel picture and quick access to individual items without navigating away.

## Acceptance Criteria

**Happy Path — Collapsed State (default):**
- **Given** the user is authenticated and on the Dashboard
- **When** the dashboard loads
- **Then** the Pipeline Summary widget displays two rows:
  - **Submissions row:** Horizontal sequence of status pills, one per non-terminal status (Received, Triaging, WaitingOnBroker, ReadyForUWReview, InReview, Quoted, BindRequested), each showing the status label and count badge. Scoped to user's authorization.
  - **Renewals row:** Horizontal sequence of status pills, one per non-terminal status (Created, Early, OutreachStarted, InReview, Quoted), each showing the status label and count badge. Scoped to user's authorization.
  - Pills are color-coded by workflow stage using the `ColorGroup` value from the reference status tables (see [data-model.md — Workflow Statuses](../../architecture/data-model.md)):

    | ColorGroup | Tailwind Color | Hex (bg/text) | Meaning | Statuses |
    |------------|---------------|---------------|---------|----------|
    | `intake` | slate | `bg-slate-100 text-slate-700` | New / just arrived | Received, Created, Early |
    | `triage` | blue | `bg-blue-100 text-blue-700` | Being sorted / assessed | Triaging |
    | `waiting` | amber | `bg-amber-100 text-amber-700` | Blocked on external action | WaitingOnBroker, OutreachStarted |
    | `review` | violet | `bg-violet-100 text-violet-700` | Under active review | ReadyForUWReview, InReview |
    | `decision` | emerald | `bg-emerald-100 text-emerald-700` | Near completion | Quoted, BindRequested |
  - A connecting line or arrow between pills conveys left-to-right flow.

**Expanded State (hover/click on a pill):**
- **Given** the user hovers over or clicks a status pill (e.g., "InReview: 7")
- **When** the expansion triggers
- **Then** a dropdown/popover appears below the pill showing up to 5 mini-cards for that status:
  - Each mini-card shows: entity name (Account or Broker name), amount or premium estimate (if Submission), days in current status, and assigned user avatar/initials
  - A "View all N" link at the bottom of the popover navigates to the filtered Submission List or Renewal List

**Click-Through Navigation:**
- **Given** the user clicks a mini-card within the expanded popover
- **When** the click is processed
- **Then** the user is navigated to the Submission or Renewal detail view for that entity

- **Given** the user clicks the "View all N" link
- **When** the click is processed
- **Then** the user is navigated to the Submission List or Renewal List screen, pre-filtered to that status

**Edge Cases:**
- No submissions exist → Submissions row shows all status pills with count "0"; expansion shows "No submissions in this stage"
- No renewals exist → Renewals row shows all status pills with count "0"
- User has restricted scope → Counts and mini-cards reflect only authorized entities
- A status has 0 items → Display the pill with "0" count (do not hide); expansion shows empty state message
- More than 5 items in a status → Show top 5 sorted by days-in-status descending (longest first) plus "View all N" link
- Popover extends beyond viewport edge → Reposition to stay within viewport bounds

**Checklist:**
- [ ] Submissions displayed as horizontal status pills with counts
- [ ] Renewals displayed as horizontal status pills with counts
- [ ] Pills are color-coded by workflow stage
- [ ] Hover/click expands pill to show up to 5 mini-cards
- [ ] Mini-cards show: entity name, amount, days in status, assigned user
- [ ] "View all N" link navigates to filtered list
- [ ] Mini-card click navigates to entity detail
- [ ] Zero-count pills are visible (not hidden)
- [ ] Widget loads within the overall dashboard p95 < 2s target
- [ ] Popover loads within 300ms of interaction
- [ ] Authorization check: counts and mini-cards are filtered by authenticated user permissions (ABAC)
- [ ] Audit/timeline requirement: N/A (read-only view with no mutation)

## Data Requirements

**Required Fields (per status pill):**
- StatusLabel: string (e.g., "InReview")
- Count: non-negative integer
- EntityType: "Submission" | "Renewal"
- ColorGroup: string (one of `intake`, `triage`, `waiting`, `review`, `decision` — maps to Tailwind classes per the color mapping table in Acceptance Criteria)

**Required Fields (per mini-card, loaded on expand):**
- EntityId: uuid
- EntityName: string (Account name or Broker name)
- Amount: decimal | null (PremiumEstimate for Submissions; null for Renewals)
- DaysInStatus: integer (calendar days since last transition to current status)
- AssignedUserInitials: string (2 chars, from UserProfile)
- AssignedUserDisplayName: string (for tooltip)

**Validation Rules:**
- Counts must be non-negative integers
- Status labels must match the defined workflow statuses exactly
- Terminal statuses (Bound, Declined, Withdrawn, Lost, Lapsed) are excluded
- DaysInStatus must be non-negative; computed as (today - last WorkflowTransition.OccurredAt for this entity)
- Mini-cards sorted by DaysInStatus descending (items stuck longest appear first)

## Role-Based Visibility

**Roles that can view Pipeline Summary:**
- Distribution User — sees submissions/renewals scoped to their department/region
- Underwriter — sees submissions assigned to or accessible by them
- Relationship Manager — sees submissions/renewals linked to their broker relationships
- Program Manager — sees submissions/renewals within their programs
- Admin — sees all (unscoped)

**Data Visibility:**
- All pipeline data is InternalOnly.

## Non-Functional Expectations

- Performance: Status pills with counts must render within the overall dashboard p95 < 2s target. Mini-card popover data must load within 300ms (p95) after user interaction (lazy-loaded, not pre-fetched).
- Security: Backend must enforce Casbin ABAC scope before aggregating counts and returning mini-card data
- Reliability: If count query fails, display "Unable to load pipeline data" and log the error; do not block other widgets. If mini-card query fails, show "Unable to load details" in the popover.

## Dependencies

**Depends On:**
- Submission entity with CurrentStatus field and workflow status values
- Renewal entity with CurrentStatus field and workflow status values
- WorkflowTransition entity (for computing DaysInStatus)
- UserProfile entity (for assigned user initials)
- ~~Submission List / Renewal List screens (for "View all" navigation)~~ — Not in F0001/F0002 scope; links hidden per MVP Navigation Constraints
- ~~Submission / Renewal detail screens (for mini-card click-through)~~ — Not in F0001/F0002 scope; entity names render as plain text per MVP Navigation Constraints

**Related Stories:**
- F0001-S0001 — KPI Cards (complementary high-level metrics)
- F0006 — Submission Intake Workflow (provides submission data)
- F0007 — Renewal Pipeline (provides renewal data)

## Out of Scope

- Full Kanban board with drag-and-drop status transitions (Future)
- Filtering by date range within the widget
- Historical trend comparison (e.g., this week vs last week)
- Terminal status counts (Bound, Declined, etc.)
- Inline editing of submission/renewal from mini-cards

## UI/UX Notes

- Screens involved: Dashboard
- Layout: Two horizontal rows — "Submissions" label + pills on top, "Renewals" label + pills below
- Each pill: rounded rectangle with status label + count badge; color-coded background
- Pills connected by subtle arrows or lines to show flow direction (left to right)
- Expanded popover: appears below the pill, shadowed card with mini-card list
- Mini-card: compact single row — entity name (bold), amount (right-aligned), days-in-status chip (e.g., "12d"), user avatar circle
- Responsive: on narrow viewports, pills wrap to multiple rows; popover becomes full-width overlay
- Color palette: 5 `ColorGroup` categories — `intake` (slate), `triage` (blue), `waiting` (amber), `review` (violet), `decision` (emerald); see mapping table in Acceptance Criteria above

## Questions & Assumptions

**Assumptions:**
- Only non-terminal statuses are shown (terminal statuses are excluded)
- Mini-cards are lazy-loaded on hover/click (not pre-fetched) to keep initial dashboard load fast
- 5 mini-cards per popover is sufficient; users needing more use the "View all" link
- DaysInStatus is computed from the most recent WorkflowTransition for that entity

**MVP Navigation Constraints (confirmed):**
- Submission List, Renewal List, Submission Detail, and Renewal Detail screens are not in F0001/F0002 scope.
- "View all N" links are hidden (not rendered) until target list screens exist.
- Mini-card entity names render as plain text (not clickable) until target detail screens exist.
- See [feature-assembly-plan.md — MVP Navigation Constraints](../../architecture/feature-assembly-plan.md) for full degradation rules. Note: the color-group spec and navigation constraints are also inlined in this story (see the ColorGroup table in Acceptance Criteria and the constraints above) and are the authoritative source if that file does not yet exist.

## Definition of Done

- [x] Acceptance criteria met
- [x] Edge cases handled (zero data, restricted scope, query failure, popover positioning)
- [x] Permissions enforced (Casbin ABAC scope filtering)
- [x] Audit/timeline logged: N/A (read-only)
- [ ] Tests pass (unit test for pill rendering and grouping, integration test for scoped counts and mini-card queries)
- [x] Click-through navigation works for available list and detail screens — mini-card names plain text, "View all" hidden per MVP constraints (F0006/F0007)
- [ ] Accessible: pills are keyboard-navigable (Tab + Enter/Space to expand), popover has role="dialog" with aria-label
