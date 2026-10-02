# TASK-001: Data Layer Partial Unique Index & EF Migration

> **Feature**: `FEATURE-002` ([requirements.md](../requirements.md))  
> **Status**: `VERIFIED`  
> **Order**: 001  
> **Dependencies**: None  

---

## 1. Objective & Boundaries
Configure the partial unique index on `vehicle_contacts (VehicleId, ContactId) WHERE NOT IsDeleted` in `FleetNexusDbContext.cs` and generate the corresponding EF Core migration `AddDuplicateAssignmentGuard`.

*Boundaries*: Do NOT touch API controllers, DTOs, or UI components in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-010`, `INV-004`
- **Target Acceptance Criteria**: `AC-010`, `TEST-011`
- **Design References**: [`design.md §3.1.1`](../design.md#311-data-layer-appfleet-nexus-data), [`ADR-017`](../../../docs/ADR/ADR-017-tenant-scoped-soft-delete-unique-constraints.md), [`ADR-022`](../../../docs/ADR/ADR-022-vehicle-contact-many-to-many-join-entity.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Data/FleetNexusDbContext.cs` | Modify | Add partial unique index on `(VehicleId, ContactId)` filtered by `!IsDeleted` to prevent duplicate active assignments |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Migrations/[Timestamp]_AddDuplicateAssignmentGuard.cs` | Create | EF Core migration generating PostgreSQL partial unique index |

---

## 4. Required Tests & Evidence Proofs
- `dotnet build app-fleet-nexus-net/api/appfleet-nexus-data/appfleet-nexus-data.csproj` succeeds with 0 errors.
- EF Core migration script compiles and verifies `Up()` and `Down()` operations.

---

## 5. Constraints & Non-Negotiables
- Index must be partial: `WHERE "IsDeleted" = false` so that soft-deleted assignment history is preserved without violating uniqueness (ADR-017).
- No new columns may be added to `VehicleContact` (`UnassignedDate` was rejected; `BaseEntity.ModifiedDate` serves as unassignment timestamp).

---

## 6. Definition of Done (DoD) Checklist
- [x] `FleetNexusDbContext.cs` configured with partial unique index.
- [x] EF migration generated cleanly using `dotnet ef migrations add AddDuplicateAssignmentGuard`.
- [x] `appfleet-nexus-data` compiles without warnings or errors.
- [x] Task status updated to `VERIFIED`.
