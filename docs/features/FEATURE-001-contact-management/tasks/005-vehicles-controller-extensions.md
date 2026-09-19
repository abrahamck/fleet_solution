# TASK-005: Vehicles API Controller Extensions

> **Feature**: `FEATURE-001` ([requirements.md](../requirements.md))  
> **Status**: `READY`  
> **Order**: 005  
> **Dependencies**: `TASK-001`, `TASK-003`, `TASK-004`  

---

## 1. Objective & Boundaries

Extend `VehiclesController.cs` in `appfleet-nexus-api` to support contact assignments and vehicle-level contact points:
- `GET /api/vehicles`: Eager-load and return assigned contacts (with role and primary flag) and primary garage address.
- `GET /api/vehicles/{id}`: Return vehicle details including all assigned contacts (`VehicleContactAssignmentDto`) and vehicle contact points (`ContactAddressDto` / `ContactPhoneDto`).
- `POST /api/vehicles`: Validate invariant I-002 (Every vehicle must have ≥ 1 active contact assignment, `AC-011`). Persist `VehicleContact` records and optional vehicle contact points (e.g. Garage Address, `AC-008`).
- `PUT /api/vehicles/{id}`: Validate invariant I-002 on update. Sync `VehicleContact` assignments and vehicle contact points.
- `DELETE /api/vehicles/{id}`: Soft delete vehicle and its `VehicleContact` join rows.

*Boundary*: Do NOT modify UI components in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-008`, `REQ-010`, `REQ-011`
- **Target Acceptance Criteria**: `AC-008`, `AC-010`, `AC-011`
- **Design References**: [`design.md`](../design.md) (Section 3.2 — Vehicles Controller Extensions), [`ADR-022`](../../../docs/ADR/ADR-022-vehicle-contact-many-to-many-join-entity.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-api/Controllers/VehiclesController.cs` | Modify | Extend vehicle queries and mutations to include `VehicleContacts` and vehicle `ContactAddresses`/`ContactPhones`. Enforce ≥1 assigned contact invariant. |

---

## 4. Required Tests & Evidence Proofs
- `dotnet build app-fleet-nexus-net/api/appfleet-nexus-api/appfleet-nexus-api.csproj` compiles with 0 errors.

---

## 5. Constraints & Non-Negotiables
- Creating or updating a vehicle with 0 assigned contacts must return `400 Bad Request`.
- Multiple contacts may be assigned, but only one can be flagged `IsPrimary` (default to the first driver).

---

## 6. Definition of Done (DoD) Checklist
- [ ] `VehiclesController.cs` updated to handle contact associations and vehicle contact points.
- [ ] Invariant I-002 enforced on vehicle create and update.
- [ ] Task status updated to `VERIFIED`.
