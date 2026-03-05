# F0003-S0002: Update Task

**Story ID:** F0003-S0002
**Feature:** F0003 — Task Center + Reminders (API-only MVP)
**Title:** Update a task (self-assigned)  
**Priority:** High  
**Phase:** MVP

## User Story

**As a** Distribution User, Underwriter, Relationship Manager, Program Manager, Distribution Manager, or Admin  
**I want** to update my own tasks  
**So that** I can keep task details and status accurate

## Context & Background

Tasks evolve as work progresses. MVP supports self-updates to status, due date, and notes without enabling cross-user task changes.

## Acceptance Criteria

**Happy Path:**
- **Given** an authenticated internal user
- **When** they submit `PUT /tasks/{taskId}` with a valid payload
- **Then** the task is updated and returned in the response

**Authorization:**
- **Given** the task is not assigned to the authenticated subject
- **When** update is attempted
- **Then** the request is rejected with HTTP 403

**Status Transitions:**
- **Given** `status` changes to `Done`
- **Then** `CompletedAt` is set and a `TaskCompleted` event is appended
- **Given** `status` changes from `Done` to `Open` or `InProgress`
- **Then** `CompletedAt` is cleared and a `TaskReopened` event is appended

**Audit:**
- **Given** a task update that does not change status
- **Then** a `TaskUpdated` ActivityTimelineEvent is appended

## Data Requirements

**Updatable Fields:**
- Title
- Description
- Status
- Priority
- DueDate
- LinkedEntityType, LinkedEntityId
- AssignedTo (must match authenticated subject if provided)

**Validation Rules:**
- Payload must include at least one field
- AssignedTo, if provided, must equal authenticated user subject

## Role-Based Visibility

**Roles that can update tasks:**
- All internal roles — own tasks only

**Data Visibility:**
- Task data is InternalOnly.

## Non-Functional Expectations

- Security: enforce ownership at the API boundary
- Reliability: return ProblemDetails for validation errors

## Dependencies

**Depends On:**
- F0003-S0001 — Create Task

## Out of Scope

- Reassigning tasks to other users
- Bulk task updates

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Permissions enforced (own tasks only)
- [ ] Audit/timeline logged (TaskUpdated/TaskCompleted/TaskReopened)
- [ ] Tests pass
