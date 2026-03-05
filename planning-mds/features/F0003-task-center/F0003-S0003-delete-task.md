# F0003-S0003: Delete Task

**Story ID:** F0003-S0003
**Feature:** F0003 — Task Center + Reminders (API-only MVP)
**Title:** Soft delete a task (self-assigned)  
**Priority:** Medium  
**Phase:** MVP

## User Story

**As a** Distribution User, Underwriter, Relationship Manager, Program Manager, Distribution Manager, or Admin  
**I want** to delete my own tasks  
**So that** I can remove tasks that are no longer relevant

## Context & Background

Tasks are soft-deleted to preserve audit history while removing them from active views.

## Acceptance Criteria

**Happy Path:**
- **Given** an authenticated internal user
- **When** they submit `DELETE /tasks/{taskId}`
- **Then** the task is soft-deleted and the API returns 204

**Authorization:**
- **Given** the task is not assigned to the authenticated subject
- **When** delete is attempted
- **Then** the request is rejected with HTTP 403

**Audit:**
- **Given** a task is deleted
- **Then** a `TaskDeleted` ActivityTimelineEvent is appended

**Visibility:**
- **Given** a task is deleted
- **Then** it no longer appears in `/my/tasks` or nudge calculations

## Data Requirements

**Required Fields:**
- TaskId

## Role-Based Visibility

**Roles that can delete tasks:**
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

- Hard delete
- Deleting tasks assigned to other users

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Permissions enforced (own tasks only)
- [ ] Audit/timeline logged (TaskDeleted)
- [ ] Tests pass
