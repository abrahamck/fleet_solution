# TASK-002: API Diff-Based Assignment Sync Tech Debt Fix

> **Feature**: `FEATURE-002` ([requirements.md](../requirements.md))  
> **Status**: `VERIFIED`  
> **Order**: 002  
> **Dependencies**: TASK-001  

---

## 1. Objective & Boundaries
Replace the destructive delete-and-recreate assignment sync pattern in both `VehiclesController.UpdateVehicle` (`PUT /api/vehicles/{id}`) and `ContactsController.UpdateContact` (`PUT /api/contacts/{id}`) with a non-destructive **diff-based sync**.

*Boundaries*: Do NOT add the new reassignment endpoint (`POST /api/vehicles/{id}/reassign`) in this task; focus exclusively on the diff-sync tech debt remediation.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `INV-001`, `INV-007`
- **Target Acceptance Criteria**: `AC-008`, `TEST-008`, `TEST-014`, `TEST-015`
- **Design References**: [`design.md §1.2 & §3.1.2`](../design.md#312-api-layer-appfleet-nexus-api), [`ADR-014`](../../../docs/ADR/ADR-014-soft-delete-and-audit-strategy.md), [`ADR-022`](../../../docs/ADR/ADR-022-vehicle-contact-many-to-many-join-entity.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-api/Controllers/VehiclesController.cs` | Modify | In `UpdateVehicle`, replace `Remove(ea)` on all assignments with diff calculation: soft-delete removed, insert new with `UtcNow`, retain existing with preserved `AssignedDate` and updated role/primary flags. Enforce ≥1 contact invariant. |
| `app-fleet-nexus-net/api/appfleet-nexus-api/Controllers/ContactsController.cs` | Modify | In `UpdateContact`, replace `Remove(ea)` on all vehicle assignments with diff calculation: soft-delete removed, insert new, retain existing with preserved `AssignedDate`. |

---

## 4. Required Tests & Evidence Proofs
- `TEST-014`: Vehicle update with retained contact preserves original `AssignedDate`.
- `TEST-015`: Contact update with retained vehicle preserves original `AssignedDate`.
- `TEST-008`: Attempting to clear all contacts on an active vehicle returns `400 Bad Request`.
- Compilation of `appfleet-nexus-api` succeeds with 0 errors.

---

## 5. Constraints & Non-Negotiables
- Must NOT delete/re-insert active records that did not change; `AssignedDate` must remain unchanged.
- Removing an assignment must use EF Core's `Remove()`, which triggers soft-delete (`IsDeleted = true`, `ModifiedDate = UtcNow`) via the DbContext's `SaveChangesAsync` interceptor.

---

## 6. Definition of Done (DoD) Checklist
- [x] `VehiclesController.UpdateVehicle` implements non-destructive diff sync.
- [x] `ContactsController.UpdateContact` implements non-destructive diff sync.
- [x] Code compiles cleanly with 0 errors.
- [x] Invariant I-002 (≥1 contact on active vehicle) remains strictly enforced.
- [x] Task status updated to `VERIFIED`.
