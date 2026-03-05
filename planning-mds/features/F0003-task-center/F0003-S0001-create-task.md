# F0003-S0001: Create Task

**Story ID:** F0003-S0001
**Feature:** F0003 — Task Center + Reminders (API-only MVP)
**Title:** Create a task (self-assigned)  
**Priority:** High  
**Phase:** MVP

## User Story

**As a** Distribution User, Underwriter, Relationship Manager, Program Manager, Distribution Manager, or Admin  
**I want** to create a task assigned to myself  
**So that** I can track follow-ups that appear on my dashboard

## Context & Background

The dashboard widgets require real tasks to exist. MVP supports self-assigned tasks only to keep scope minimal while enabling full read/write task flow via API.

## Acceptance Criteria

**Happy Path:**
- **Given** an authenticated internal user
- **When** they submit `POST /tasks` with a valid payload
- **Then** a new task is created with `Status=Open` if status is not provided
- **And** the response returns the created Task record

**Authorization:**
- **Given** the request body contains `assignedTo`
- **When** `assignedTo` does not match the authenticated subject
- **Then** the request is rejected with HTTP 403

**Audit:**
- **Given** a task is created
- **Then** a `TaskCreated` ActivityTimelineEvent is appended

## Data Requirements

**Required Fields:**
- Title: task title
- AssignedTo: must equal authenticated user subject

**Optional Fields:**
- Description
- Priority
- DueDate
- LinkedEntityType, LinkedEntityId

**Validation Rules:**
- Title max length 255
- Description max length 2000

## Role-Based Visibility

**Roles that can create tasks:**
- All internal roles — self-assigned only

**Data Visibility:**
- Task data is InternalOnly.

## Non-Functional Expectations

- Security: enforce self-assignment at the API boundary
- Reliability: return ProblemDetails for validation errors

## Dependencies

**Depends On:**
- Task entity + indexes
- ABAC enforcement and policy coverage

**Related Stories:**
- F0003-S0002 — Update Task
- F0003-S0003 — Delete Task

## Out of Scope

- Assigning tasks to other users
- Task creation from dashboard widget UI

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Permissions enforced (self-assignment only)
- [ ] Audit/timeline logged (TaskCreated)
- [ ] Tests pass
