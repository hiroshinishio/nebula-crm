# F0009 — Broker Visibility Matrix (Phase 1)

Purpose: define required field-level response boundaries for `BrokerUser`.

## Classification Rules

- `BrokerVisible`: may be returned to BrokerUser.
- `InternalOnly`: must not be returned to BrokerUser.
- If classification is unknown for a field, treat as `InternalOnly`.

## Broker Resource

| Field | Classification | Notes |
|-------|----------------|-------|
| `id` | BrokerVisible | Required for routing/detail navigation. |
| `legalName` | BrokerVisible | |
| `licenseNumber` | BrokerVisible | |
| `state` | BrokerVisible | |
| `status` | BrokerVisible | |
| `email` | BrokerVisible | |
| `phone` | BrokerVisible | |
| `rowVersion` | InternalOnly | Concurrency token not exposed to BrokerUser in Phase 1. |
| `isDeactivated` | InternalOnly | Internal lifecycle metadata. |

## Contact Resource

| Field | Classification | Notes |
|-------|----------------|-------|
| `id` | BrokerVisible | |
| `brokerId` | BrokerVisible | |
| `name` | BrokerVisible | |
| `title` | BrokerVisible | |
| `email` | BrokerVisible | |
| `phone` | BrokerVisible | |
| `rowVersion` | InternalOnly | Concurrency token not exposed to BrokerUser in Phase 1. |
| Internal assignment/ownership metadata | InternalOnly | Any non-contract metadata fields are denied. |

## Timeline Event Resource

| Field | Classification | Notes |
|-------|----------------|-------|
| `id` | BrokerVisible | |
| `entityType` | BrokerVisible | |
| `entityId` | BrokerVisible | |
| `eventType` | BrokerVisible | Only approved event types listed below. |
| `eventDescription` | BrokerVisible | Must be sanitized for internal-only content. |
| `occurredAt` | BrokerVisible | |
| `actorDisplayName` | BrokerVisible | |
| `actorUserId` | InternalOnly | |

Approved BrokerUser timeline event types in Phase 1:
- `BrokerCreated`
- `BrokerUpdated`
- `BrokerStatusChanged`
- `ContactAdded`
- `ContactUpdated`

All other timeline event types are InternalOnly for BrokerUser.

## Dashboard/Task Responses

- Only summary fields needed for broker workspace rendering are BrokerVisible.
- Any internal assignment metadata, policy diagnostics, or internal staff identifiers are InternalOnly.
- If an existing response shape cannot be safely filtered, endpoint remains denied for BrokerUser.

## Enforcement

- Field filtering is server-side mandatory for BrokerUser.
- Frontend-only filtering does not satisfy this requirement.
- Verification must include response payload assertions in integration tests.
