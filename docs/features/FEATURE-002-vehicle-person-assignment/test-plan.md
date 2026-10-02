# FEATURE-002 — Test Plan: Vehicle and Person Assignment / Reassignment

> **Status**: `READY_FOR_EXECUTION`  
> **Requirements Reference**: [`requirements.md`](requirements.md) (Status: `APPROVED`)  
> **Design Reference**: [`design.md`](design.md) (Status: `APPROVED`)  
> **Created**: 2026-10-01  
> **Author**: AI Test Design Agent  

---

## 1. Test Strategy & Independence Principle

This test plan is derived **strictly from business requirements, acceptance criteria, and invariant specifications**, formulated independently of the underlying implementation code.

The test suite spans three distinct validation tiers:
1. **Database & Invariant Guarantees**: Partial unique constraints preventing duplicate active assignments (`INV-004`), soft-delete preservation of audit history (`INV-007`), and multi-tenant isolation (`INV-008`).
2. **API Endpoint & Integration Tests**: Controller integration tests with in-memory database verifying:
   - Dedicated reassign endpoint (`POST /api/vehicles/{id}/reassign`) across `ReplacePrimary` and `AddSecondary` modes.
   - Non-destructive diff-based sync on `PUT /api/vehicles/{id}` and `PUT /api/contacts/{id}`, verifying that `AssignedDate` is preserved across updates.
   - Guard against removing all contacts from active vehicles (`INV-001`, `AC-008`).
   - Cross-tenant security rejection (`AC-012`).
   - Upsert semantics when reassigning already assigned contacts (`AC-010`).
3. **UI Component & Workflow Validation**: Blazor component compilation and structural validation for:
   - Dedicated `ReassignModal.razor` action and state handling.
   - Inline entity wizard launching from `VehicleDialog` and `ContactDialog`.
   - Recursion suppression via `IsInlineMode` (`INV-006`).
   - Draft form preservation upon child dialog cancellation (`AC-013`).

---

## 2. Requirements-to-Test Traceability Matrix

| Test ID | Target AC | Invariant | Test Scenario & Description | Test Layer | Expected Result |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TEST-001` | `AC-001` | `INV-001` | Create vehicle assigning an existing contact | API Integration | `201 Created`; `VehicleContact` record created with designated role and `IsPrimary = true` |
| `TEST-002` | `AC-002` | `INV-006` | Vehicle Dialog launches Contact Wizard inline | UI / Component | `ContactDialog` opens with `IsInlineMode = true`; "+ Add Vehicle" button suppressed |
| `TEST-003` | `AC-003` | `INV-003` | Create contact assigning an existing vehicle | API Integration | `201 Created`; `VehicleContact` created linking contact to vehicle |
| `TEST-004` | `AC-004` | `INV-006` | Contact Wizard Step 5 launches Vehicle Dialog inline | UI / Component | `VehicleDialog` opens with `IsInlineMode = true`; "+ Add Contact" button suppressed |
| `TEST-005` | `AC-005` | — | Open Quick Reassign Modal from vehicle context | UI / Component | Displays vehicle unit number, current primary driver, secondary contacts, and contact dropdown |
| `TEST-006` | `AC-006` | `INV-002`, `INV-007` | Call `reassign` with `ReplacePrimary` mode | API Integration | Current primary driver soft-deleted (`IsDeleted = true`); new driver inserted as `IsPrimary = true`; secondary contacts untouched; `200 OK` |
| `TEST-007` | `AC-007` | `INV-002` | Call `reassign` with `AddSecondary` mode | API Integration | Existing primary driver preserved; new driver added with `IsPrimary = false` and `AssociationRole = "Driver"`; `200 OK` |
| `TEST-008` | `AC-008` | `INV-001` | Attempt to update vehicle removing all assigned contacts | API Integration | `400 Bad Request` ("At least one contact must be assigned to a vehicle") |
| `TEST-009` | `AC-009` | `INV-005` | Attempt to delete contact who is sole assignee on an active vehicle | API Integration | `409 Conflict` with list of blocking vehicle unit numbers (verified existing behavior) |
| `TEST-010` | `AC-010` | `INV-004` | Reassign contact who is already assigned as secondary to primary role | API Integration | Existing record updated in-place to `IsPrimary = true`; no duplicate row created; `200 OK` |
| `TEST-011` | `AC-010` | `INV-004` | Attempt direct insertion of duplicate active `(VehicleId, ContactId)` | DB / Integration | DbUpdateException thrown due to partial unique index |
| `TEST-012` | `AC-012` | `INV-008` | Tenant A attempts to reassign vehicle using Tenant B contact | API Integration / Security | `400 Bad Request` or `404 Not Found` (target contact not accessible) |
| `TEST-013` | `AC-013` | — | Cancel inline Contact Wizard from Vehicle Dialog | UI / Component | Vehicle Dialog draft attributes (`UnitNumber`, `Vin`, `Make`, etc.) remain fully intact |
| `TEST-014` | Tech Debt | `INV-007` | Diff-based sync: Update vehicle with unchanged contact | API Integration | Existing `VehicleContact.AssignedDate` is preserved (not reset to UtcNow); `IsDeleted = false` |
| `TEST-015` | Tech Debt | `INV-007` | Diff-based sync: Update contact with unchanged vehicle | API Integration | Existing `VehicleContact.AssignedDate` is preserved across contact update; `IsDeleted = false` |

---

## 3. Concrete Test Method Specifications

### 3.1 Vehicles Controller Tests (`VehiclesControllerTests.cs`)

1. **`ReassignVehicle_ReplacePrimary_SoftDeletesOldAndAssignsNewPrimary` (`TEST-006`)**:
   - Seed Vehicle with Contact A as Primary.
   - Invoke `POST /api/vehicles/{id}/reassign` with `NewContactId = Contact B`, `ReassignMode = "ReplacePrimary"`.
   - Assert: HTTP 200 OK.
   - Assert: Vehicle has 1 active assignment (Contact B, `IsPrimary = true`).
   - Assert: Soft-deleted assignment for Contact A has `IsDeleted = true`.

2. **`ReassignVehicle_AddSecondary_RetainsPrimaryAndAddsNew` (`TEST-007`)**:
   - Seed Vehicle with Contact A as Primary.
   - Invoke `POST /api/vehicles/{id}/reassign` with `NewContactId = Contact B`, `ReassignMode = "AddSecondary"`.
   - Assert: HTTP 200 OK.
   - Assert: Vehicle now has 2 active assignments: Contact A (`IsPrimary = true`), Contact B (`IsPrimary = false`).

3. **`ReassignVehicle_AlreadyAssignedContact_PromotesWithoutDuplicate` (`TEST-010`)**:
   - Seed Vehicle with Contact A (Primary) and Contact B (Secondary).
   - Invoke `POST /api/vehicles/{id}/reassign` with `NewContactId = Contact B`, `ReassignMode = "ReplacePrimary"`.
   - Assert: HTTP 200 OK.
   - Assert: Total active assignments = 1 (Contact B now Primary). Contact A is soft-deleted. No duplicate rows exist.

4. **`ReassignVehicle_CrossTenantContact_ReturnsNotFoundOrBadRequest` (`TEST-012`)**:
   - Seed Vehicle in Tenant 1, Contact C in Tenant 2.
   - Invoke `POST /api/vehicles/{id}/reassign` with `NewContactId = Contact C`.
   - Assert: HTTP 400 or 404 (contact not accessible in tenant).

5. **`UpdateVehicle_DiffSync_PreservesAssignedDateForExistingAssignments` (`TEST-014`)**:
   - Seed Vehicle with Contact A assigned at `DateTime.UtcNow.AddDays(-10)`.
   - Invoke `PUT /api/vehicles/{id}` retaining Contact A and adding Contact B.
   - Assert: HTTP 200 OK.
   - Assert: Contact A's `AssignedDate` matches the original 10-day-old timestamp, NOT `UtcNow`.
   - Assert: Contact B's `AssignedDate` is set to `UtcNow`.

6. **`UpdateVehicle_RemovingAllContacts_ReturnsBadRequest` (`TEST-008`)**:
   - Seed Vehicle with Contact A.
   - Invoke `PUT /api/vehicles/{id}` with empty `AssignedContacts`.
   - Assert: HTTP 400 Bad Request.

### 3.2 Contacts Controller Tests (`ContactsControllerTests.cs`)

1. **`UpdateContact_DiffSync_PreservesVehicleAssignedDate` (`TEST-015`)**:
   - Seed Contact with Vehicle 1 assigned at `DateTime.UtcNow.AddDays(-10)`.
   - Invoke `PUT /api/contacts/{id}` retaining Vehicle 1.
   - Assert: HTTP 200 OK.
   - Assert: Vehicle 1's `VehicleContact.AssignedDate` remains unchanged.

### 3.3 Database Invariant Tests

1. **`UniqueIndex_PreventsDuplicateActiveVehicleContact` (`TEST-011`)**:
   - Attempt to add two active `VehicleContact` rows with identical `(VehicleId, ContactId)` directly to DbContext.
   - Assert: `DbUpdateException` thrown upon `SaveChangesAsync()`.

---

## 4. Test Execution Instructions

Execute the entire test suite via .NET CLI:
```powershell
# Run all backend unit and integration tests
dotnet test c:\Learn\fleet_solution\app-fleet-nexus-net\api\appfleet-nexus-api.Tests\appfleet-nexus-api.Tests.csproj

# Run specifically Vehicles and Contacts controller tests
dotnet test c:\Learn\fleet_solution\app-fleet-nexus-net\api\appfleet-nexus-api.Tests\appfleet-nexus-api.Tests.csproj --filter "FullyQualifiedName~VehiclesControllerTests|FullyQualifiedName~ContactsControllerTests"

# Build UI project to verify Razor component compilation and references
dotnet build c:\Learn\fleet_solution\app-fleet-nexus-net\ui\appfleet-nexus-ui\appfleet-nexus-ui.csproj
```
