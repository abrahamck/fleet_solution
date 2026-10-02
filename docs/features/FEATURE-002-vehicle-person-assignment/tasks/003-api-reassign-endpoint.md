# TASK-003: API Reassignment Endpoint & Invariant Enforcement

> **Feature**: `FEATURE-002` ([requirements.md](../requirements.md))  
> **Status**: `VERIFIED`  
> **Order**: 003  
> **Dependencies**: TASK-001, TASK-002  

---

## 1. Objective & Boundaries
Implement the `ReassignVehicleRequest` DTO in `VehicleDtos.cs` and the dedicated `POST /api/vehicles/{id}/reassign` action in `VehiclesController.cs` supporting `ReplacePrimary` and `AddSecondary` operational modes with strict invariant checks.

*Boundaries*: Do NOT touch UI components or create separate controllers.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-005`, `REQ-006`, `REQ-007`, `REQ-010`, `INV-001`, `INV-002`, `INV-004`, `INV-007`, `INV-008`
- **Target Acceptance Criteria**: `AC-006`, `AC-007`, `AC-010`, `AC-012`, `TEST-006`, `TEST-007`, `TEST-010`, `TEST-012`
- **Design References**: [`design.md §3.1.2 & §3.2 Flow 1`](../design.md#312-api-layer-appfleet-nexus-api), [`ADR-010`](../../../docs/ADR/ADR-010-role-based-access-control.md), [`ADR-014`](../../../docs/ADR/ADR-014-soft-delete-and-audit-strategy.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-api/Models/VehicleDtos.cs` | Modify | Add `ReassignVehicleRequest` class with `NewContactId`, `AssociationRole` (default `"Driver"`), and `ReassignMode` (`"ReplacePrimary"` or `"AddSecondary"`). |
| `app-fleet-nexus-net/api/appfleet-nexus-api/Controllers/VehiclesController.cs` | Modify | Implement `POST /api/vehicles/{id}/reassign`: validate tenant isolation, handle `ReplacePrimary` (soft-delete old primary, promote/add new), handle `AddSecondary` (add or update), enforce `INV-001` (≥1 contact), invalidate dashboard KPI cache, return updated assignment list. |

---

## 4. Required Tests & Evidence Proofs
- `TEST-006`: `ReplacePrimary` soft-deletes prior primary and assigns new primary.
- `TEST-007`: `AddSecondary` keeps existing primary and creates secondary contact.
- `TEST-010`: Reassigning an already assigned secondary contact to primary promotes them without duplicate rows.
- `TEST-012`: Reassigning to a cross-tenant contact is rejected (returns 404/400).
- `dotnet build appfleet-nexus-api.csproj` compiles cleanly.

---

## 5. Constraints & Non-Negotiables
- Must be decorated with `[Authorize]`.
- All operations must execute in a single atomic transaction.
- If the new contact is already assigned, endpoint MUST update the existing row rather than attempting duplicate insertion (`INV-004`).

---

## 6. Definition of Done (DoD) Checklist
- [x] `ReassignVehicleRequest` DTO added with validation attributes.
- [x] `POST /api/vehicles/{id}/reassign` implemented in `VehiclesController.cs`.
- [x] Both `ReplacePrimary` and `AddSecondary` branches fully operational.
- [x] Invariant guards (`INV-001`, `INV-002`, `INV-004`) verified.
- [x] Task status updated to `VERIFIED`.
