# FEATURE-002 — Implementation Plan: Vehicle and Person Assignment / Reassignment

> **Status**: `APPROVED`  
> **Requirements**: [`requirements.md`](requirements.md) (Status: `APPROVED`)  
> **Design**: [`design.md`](design.md) (Status: `APPROVED`)  
> **Created**: 2026-10-01  
> **Gate 3 Approval**: [x] Approved by Human (Date: 2026-10-02)  
> **Author**: AI Planning Agent  
> **Governing ADRs**: [ADR-003](../../adr/ADR-003-multi-tenancy-shared-schema.md), [ADR-004](../../adr/ADR-004-tenant-isolation-defense-in-depth.md), [ADR-010](../../adr/ADR-010-role-based-access-control.md), [ADR-013](../../adr/ADR-013-user-profile-and-contact-separation.md), [ADR-014](../../adr/ADR-014-soft-delete-and-audit-strategy.md), [ADR-017](../../adr/ADR-017-tenant-scoped-soft-delete-unique-constraints.md), [ADR-018](../../adr/ADR-018-multi-step-input-wizard-ux-pattern.md), [ADR-020](../../adr/ADR-020-unified-inventory-workspace.md), [ADR-022](../../adr/ADR-022-vehicle-contact-many-to-many-join-entity.md)

---

## 1. Definition of Ready (DoR) Checklist

All preconditions for decomposition and execution are satisfied:
- [x] Requirements document exists and is marked `APPROVED` (Gate 1 passed on 2026-10-01).
- [x] Design document exists and is marked `APPROVED` (Gate 2 passed on 2026-10-01).
- [x] Governing ADRs evaluated and compliance verified (no novel ADR required; conforms to ADR-003, ADR-004, ADR-014, ADR-017, ADR-018, ADR-020, ADR-022).
- [x] Scope and acceptance criteria are disambiguated (`AC-001` through `AC-013`, with `AC-011` batch assignment explicitly deferred to `DEFER-001`).
- [x] Pre-existing tech debt identified: destructive delete-and-recreate assignment sync in `PUT /api/vehicles/{id}` and `PUT /api/contacts/{id}` scheduled for diff-based sync remediation.
- [x] Schema decision locked: redundant `UnassignedDate` column rejected in favor of `BaseEntity.ModifiedDate` stamped on soft-delete. Partial unique index on active `(VehicleId, ContactId)` defined.

---

## 2. Technical Scope & Change Inventory

### 2.1 Database & Schema (`appfleet-nexus-data`)

```
app-fleet-nexus-net/api/appfleet-nexus-data/
├── Data/
│   └── FleetNexusDbContext.cs            # [MODIFY] Add partial unique index on (VehicleId, ContactId) WHERE NOT IsDeleted
└── Migrations/
    └── [Timestamp]_AddDuplicateAssignmentGuard.cs # [NEW] EF Core migration for partial unique index
```

#### Detailed Schema Changes:
1. **`FleetNexusDbContext.cs`**:
   - Add partial unique index to `VehicleContact`:
     ```csharp
     modelBuilder.Entity<VehicleContact>()
         .HasIndex(vc => new { vc.VehicleId, vc.ContactId })
         .IsUnique()
         .HasFilter("\"IsDeleted\" = false");
     ```
2. **EF Core Migration (`AddDuplicateAssignmentGuard`)**:
   - Up:
     ```sql
     CREATE UNIQUE INDEX "IX_vehicle_contacts_VehicleId_ContactId"
     ON vehicle_contacts ("VehicleId", "ContactId")
     WHERE "IsDeleted" = false;
     ```
   - Down:
     ```sql
     DROP INDEX IF EXISTS "IX_vehicle_contacts_VehicleId_ContactId";
     ```

---

### 2.2 Security & Multi-Tenancy (`appfleet-nexus-security` & Data Layer)

- Existing global query filters on `VehicleContact` (`e.TenantId == CurrentTenantId && !e.IsDeleted`) enforce strict tenant boundaries.
- Cross-tenant vehicle or contact references in assignment endpoints naturally fail lookup with `404 Not Found`.
- All new controller endpoints decorated with `[Authorize]` (Admin and Member access per ADR-010).

---

### 2.3 API & Business Logic Layer (`appfleet-nexus-api`)

```
app-fleet-nexus-net/api/appfleet-nexus-api/
├── Controllers/
│   ├── VehiclesController.cs             # [MODIFY] Add POST /api/vehicles/{id}/reassign & refactor PUT /api/vehicles/{id} to diff-based sync
│   └── ContactsController.cs             # [MODIFY] Refactor PUT /api/contacts/{id} to diff-based sync for vehicle assignments
└── Models/
    └── VehicleDtos.cs                    # [MODIFY] Add ReassignVehicleRequest DTO & ReassignMode enum/constants
```

#### API Endpoints Contract:
| Method | Route | Description | Request Body | Status Codes |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/vehicles/{id}/reassign` | Dedicated single-action driver reassignment | `ReassignVehicleRequest` | `200 OK`, `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| `PUT` | `/api/vehicles/{id}` | Updated vehicle save with **diff-based assignment sync** | `VehicleUpsertRequest` | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `PUT` | `/api/contacts/{id}` | Updated contact save with **diff-based assignment sync** | `ContactUpsertRequest` | `200 OK`, `400 Bad Request`, `404 Not Found` |

#### Detailed Endpoint Logic:
1. **`POST /api/vehicles/{id}/reassign`**:
   - `ReassignVehicleRequest`:
     - `NewContactId` (`Guid`, Required)
     - `AssociationRole` (`string`, Default `"Driver"`)
     - `ReassignMode` (`string`: `"ReplacePrimary"` | `"AddSecondary"`, Required)
   - Behavior:
     - Verify vehicle exists in tenant.
     - Verify `NewContactId` exists in tenant.
     - If `ReplacePrimary`:
       - Soft-delete any existing active primary `VehicleContact` (`IsDeleted = true`).
       - If new contact was already an active secondary contact, upgrade them to `IsPrimary = true` and `AssociationRole = request.AssociationRole`.
       - If new contact was not previously assigned, insert new `VehicleContact` with `IsPrimary = true`, `AssignedDate = DateTime.UtcNow`.
       - Secondary contacts remain untouched.
     - If `AddSecondary`:
       - If new contact is already assigned, update their `AssociationRole` to requested role (keeping `IsPrimary = false` unless already primary).
       - If new contact is not assigned, insert new `VehicleContact` with `IsPrimary = false`, `AssignedDate = DateTime.UtcNow`.
     - Invariant Check (`INV-001`): Ensure at least 1 active contact assignment remains on the vehicle.
     - Invariant Check (`INV-002`): Ensure at most 1 primary driver exists.
     - Save changes in single atomic transaction; evict dashboard KPI cache.
     - Return `200 OK` with updated `List<VehicleContactAssignmentDto>`.

2. **Diff-Based Assignment Sync (`PUT /api/vehicles/{id}` & `PUT /api/contacts/{id}`)**:
   - In `PUT /api/vehicles/{id}`:
     - Load active assignments: `existing = await _dbContext.VehicleContacts.Where(vc => vc.VehicleId == id).ToListAsync();`
     - Compare requested IDs with existing IDs:
       - **Removed**: Existing records not in `request.AssignedContacts` -> `_dbContext.VehicleContacts.Remove(ea)` (soft-deleted via `ISoftDeletable`).
       - **Added**: `request.AssignedContacts` with no matching existing record -> add new `VehicleContact` with `AssignedDate = DateTime.UtcNow`.
       - **Retained**: Exists in both -> update `AssociationRole` and `IsPrimary`. **Crucially, preserve original `AssignedDate`!**
   - In `PUT /api/contacts/{id}`:
     - Perform identical diff-based sync on `_db.VehicleContacts` matching `ContactId == id`.

---

### 2.4 User Interface Layer (`appfleet-nexus-ui`)

```
app-fleet-nexus-net/ui/appfleet-nexus-ui/
├── Components/
│   ├── ReassignModal.razor               # [NEW] Dedicated Quick Reassign Modal
│   ├── VehicleDialog.razor               # [MODIFY] Add inline Contact Wizard launch & IsInlineMode parameter
│   └── ContactDialog.razor               # [MODIFY] Add inline Vehicle Dialog launch & IsInlineMode parameter
├── Pages/
│   ├── Inventory.razor                   # [MODIFY] Add "Reassign" button to vehicle cards/rows and wire ReassignModal
│   └── Inventory.razor.css               # [MODIFY] Styles for Reassign button and compliance warning badges
└── Models/
    ├── VehicleModels.cs                  # [MODIFY] Add ReassignVehicleModel and ReassignMode constants
    └── ContactModels.cs                  # [MODIFY] Add compliance helper properties (e.g. IsLicenseExpired)
```

#### UI Functional Enhancements:
1. **`ReassignModal.razor`**:
   - Header with vehicle Unit Number and VIN.
   - Current assignment overview: shows current Primary Driver and any Secondary assignees.
   - Contact selector: searchable dropdown listing active tenant contacts, highlighting driver license expiration status (⚠️ `License Expired`).
   - Mode selector:
     - `Replace Primary Driver` (radio/segmented control) — informative subtext: "Replaces current primary driver. Previous primary will be unassigned."
     - `Add as Co-Driver / Secondary` (radio/segmented control) — informative subtext: "Retains current primary driver and adds this contact as a co-driver."
   - Submits to `POST /api/vehicles/{id}/reassign`; on success closes modal and invokes `OnSaved` callback to refresh inventory.

2. **Inline Entity Creation & Recursion Guard (`INV-006`, `AC-002`, `AC-004`, `AC-013`)**:
   - `VehicleDialog.razor`:
     - Add `[Parameter] public bool IsInlineMode { get; set; } = false;`
     - Assignment section: Add "+ Add New Driver / Contact" button (hidden when `IsInlineMode == true`).
     - Clicking button switches view to embedded/inline `ContactDialog`:
       - Parent vehicle form state is stored in memory.
       - Embedded `ContactDialog` rendered with `IsInlineMode="true"`.
       - If cancelled: returns to vehicle form with all entered vehicle draft data intact (`AC-013`).
       - If saved: contact is created via API, added to vehicle's `AssignedContacts` list, pre-selected as primary, and dialog returns to vehicle form.
   - `ContactDialog.razor`:
     - Add `[Parameter] public bool IsInlineMode { get; set; } = false;`
     - Step 5 (Assignments): Add "+ Add New Vehicle" button (hidden when `IsInlineMode == true`).
     - Clicking button switches view to embedded/inline `VehicleDialog`:
       - Parent contact wizard state is preserved in memory.
       - Embedded `VehicleDialog` rendered with `IsInlineMode="true"`.
       - If cancelled: returns to contact wizard Step 5 with contact draft intact (`AC-013`).
       - If saved: vehicle is created via API, added to contact's vehicle assignment list, and dialog returns to Step 5.

3. **`Inventory.razor` Integration**:
   - Vehicle cards (grid view) and vehicle rows (table view) receive a prominent "Reassign" button.
   - Clicking opens `ReassignModal` populated with the selected vehicle's current contact assignments.

---

### 2.5 Test Harness (`appfleet-nexus-api.Tests`)

```
app-fleet-nexus-net/api/appfleet-nexus-api.Tests/
└── Controllers/
    ├── VehiclesControllerTests.cs        # [MODIFY] Add tests for Reassign endpoint, diff-based sync, and duplicate guard
    └── ContactsControllerTests.cs        # [MODIFY] Add tests for Contact diff-based assignment sync
```

---

## 3. Ordered Task Breakdown Summary

| Task ID | Task Title | Primary Components | Dependencies | DoD Proof |
| :--- | :--- | :--- | :--- | :--- |
| `TASK-001` | Data Layer Partial Unique Index & Migration | `appfleet-nexus-data` | None | DB index applied, migration generated, build passes |
| `TASK-002` | API Diff-Based Assignment Sync Tech Debt Fix | `appfleet-nexus-api`: `VehiclesController`, `ContactsController` | `TASK-001` | Existing `AssignedDate` preserved on edit; soft delete on remove |
| `TASK-003` | API Reassignment Endpoint & Invariant Enforcement | `appfleet-nexus-api`: `VehiclesController`, DTOs | `TASK-001`, `TASK-002` | `POST /api/vehicles/{id}/reassign` passes all unit/integration tests |
| `TASK-004` | Blazor UI Quick Reassign Modal & Inventory Wiring | `appfleet-nexus-ui`: `ReassignModal`, `Inventory` | `TASK-003` | Reassign modal functional in inventory; build passes |
| `TASK-005` | Blazor UI Inline Entity Creation & Recursion Guards | `appfleet-nexus-ui`: `VehicleDialog`, `ContactDialog` | `TASK-004` | Inline wizard launch, draft preservation, recursion blocked |
| `TASK-006` | Automated Integration Test Suite & Evidence Logging | `appfleet-nexus-api.Tests`, `evidence.md` | `TASK-001` – `TASK-005` | 100% of test suite passing, all ACs verified in `evidence.md` |

---

## 4. Risks, Migrations & Rollback Strategy

### 4.1 Migration Hazards
- **Existing duplicate records**: If test or development data contains duplicate active assignments `(VehicleId, ContactId)`, creating the unique index would fail.
  - *Mitigation*: The migration script or pre-migration cleanup script soft-deletes duplicate active pairs, keeping the one with the latest `AssignedDate`.
- **Concurrency & Locking**: The partial index is created on `vehicle_contacts`. In production PostgreSQL, `CREATE INDEX CONCURRENTLY` can be used if table size requires non-blocking creation.

### 4.2 Rollback Procedure
- **Database Rollback**:
  ```powershell
  dotnet ef database update [PreviousMigration] --project app-fleet-nexus-net/api/appfleet-nexus-data/appfleet-nexus-data.csproj --startup-project app-fleet-nexus-net/api/appfleet-nexus-api/appfleet-nexus-api.csproj
  ```
- **Code Rollback**: Purely additive endpoint and UI component; reverting git branch cleanly restores previous behavior.
