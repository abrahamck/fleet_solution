# FEATURE-001 — Test Plan: Contact Management & Vehicle Associations

> **Status**: `APPROVED`  
> **Requirements Reference**: [`requirements.md`](requirements.md) (Status: `APPROVED`)  
> **Design Reference**: [`design.md`](design.md) (Status: `APPROVED`)  
> **Created**: 2026-09-19  
> **Author**: AI Test Design Agent  

---

## 1. Test Strategy & Independence Principle

This test plan is derived **strictly from business requirements, acceptance criteria, and failure semantics**, formulated independently of implementation code.

The test suite covers the full verification spectrum:
1. **Unit & Data Invariants**: Entity validation, partial unique indexes, soft delete, audit column stamping.
2. **API Controller & Integration**: SQLite in-memory integration tests simulating HTTP endpoints, tenant isolation, cascade safeguards, and error payloads.
3. **UI & E2E Validation**: Blazor component rendering, wizard form validation, dynamic contact point list management, and vehicle assignment selectors.

---

## 2. Requirements-to-Test Traceability Matrix

| Test ID | Target AC | Test Scenario & Description | Test Layer | Expected Result |
| :--- | :--- | :--- | :--- | :--- |
| `TEST-001` | `AC-001` | Create Contact with valid required identity + 1 Phone + 1 Address | API Integration | `201 Created` with generated Contact ID and child contact point IDs |
| `TEST-002` | `AC-002` | Create Contact with all optional attributes (`UniqueId`, `MiddleName`, Email, License/Medical dates) | API Integration | `201 Created`; all fields correctly round-tripped and retrievable via `GET /api/contacts/{id}` |
| `TEST-003` | `AC-003` | Create Contact with empty or missing `Phones` collection | API Integration / Unit | `400 Bad Request` with message indicating at least 1 phone is required |
| `TEST-004` | `AC-004` | Create Contact with empty or missing `Addresses` collection | API Integration / Unit | `400 Bad Request` with message indicating at least 1 address is required |
| `TEST-005` | `AC-005` | Create Contact with missing `FirstName` or `LastName` | API Integration / Unit | `400 Bad Request` with ModelState validation errors |
| `TEST-006` | `AC-006` | Create two Contacts with identical `UniqueId` within the same Tenant | API Integration | Second request returns `400 Bad Request` / `409 Conflict` (Duplicate UniqueId) |
| `TEST-007` | `AC-007` | Add, update, and manage `IsPrimary` flag across multiple phones/emails/addresses | API Integration | Only 1 contact point per `(type, owner)` remains `IsPrimary = true` |
| `TEST-008` | `AC-008` | Add a `ContactAddress` (e.g. Garage Address) with `OwnerType = "Vehicle"` | API Integration | Address persisted under Vehicle ID; retrievable on vehicle details |
| `TEST-009` | `AC-009` | Attempt to update Contact by removing its only Phone or only Address | API Integration | `409 Conflict` / `400 Bad Request` — Contact must maintain ≥1 phone and ≥1 address |
| `TEST-010` | `AC-010` | Assign multiple contacts to a vehicle with roles (`Driver`, `ResponsibleContact`) | API Integration | `vehicle_contacts` records persisted with correct role metadata |
| `TEST-011` | `AC-011` | Attempt to create or update a Vehicle with 0 assigned contacts | API Integration | `400 Bad Request` — Vehicle requires at least 1 assigned contact |
| `TEST-012` | `AC-012` | Create a Contact with zero vehicle assignments (on-bench / unassigned) | API Integration | `201 Created` — Unassigned contact state is valid |
| `TEST-013` | `AC-013` | Tenant A attempts to read or mutate Contact/ContactPoint of Tenant B | API Integration / Security | `404 Not Found` (EF Global Query Filter tenant isolation) |
| `TEST-014` | `AC-014` | Soft-delete a Contact not assigned to any vehicle | API Integration | `200 OK`; `is_deleted = true`; excluded from `GET /api/contacts` |
| `TEST-015` | `AC-015` | Attempt to soft-delete a Contact who is the sole active assignee on an active Vehicle | API Integration | `409 Conflict` — Blocked until vehicle is reassigned |
| `TEST-016` | `AC-016` | Navigate to `/inventory` > Contacts tab, perform search, filter, open `ContactDialog`, and test wizard | UI / Component | Full Blazor UI rendering and multi-step wizard interaction verified |

---

## 3. Concrete Test Method Specifications

### 3.1 Contact Lifecycle & Invariant Tests (`ContactsControllerTests.cs`)
- `Should_Create_Contact_With_Required_Phone_And_Address` (`TEST-001`)
- `Should_Create_And_Retrieve_Contact_With_All_Optional_Fields` (`TEST-002`)
- `Should_Reject_Contact_Creation_When_Phone_Missing` (`TEST-003`)
- `Should_Reject_Contact_Creation_When_Address_Missing` (`TEST-004`)
- `Should_Reject_Contact_Creation_When_Names_Missing` (`TEST-005`)
- `Should_Reject_Duplicate_UniqueId_Within_Same_Tenant` (`TEST-006`)
- `Should_Enforce_Single_Primary_Flag_Per_Contact_Point_Type` (`TEST-007`)
- `Should_Block_Removal_Of_Last_Phone_Or_Address_On_Contact` (`TEST-009`)
- `Should_Allow_Contact_Creation_With_No_Assigned_Vehicles` (`TEST-012`)
- `Should_Isolate_Contacts_Between_Tenants` (`TEST-013`)
- `Should_Soft_Delete_Contact_Successfully` (`TEST-014`)
- `Should_Block_Delete_When_Contact_Is_Sole_Vehicle_Assignee` (`TEST-015`)

### 3.2 Vehicle Association & Vehicle ContactPoint Tests (`VehiclesControllerTests.cs`)
- `Should_Create_Vehicle_With_Assigned_Contacts_And_Roles` (`TEST-010`)
- `Should_Reject_Vehicle_Creation_Without_Any_Assigned_Contact` (`TEST-011`)
- `Should_Attach_And_Retrieve_Vehicle_Garage_Address` (`TEST-008`)

---

## 4. Test Execution Instructions

Execute the entire test suite via .NET CLI:
```powershell
# Run all API tests
dotnet test c:\Learn\fleet_solution\app-fleet-nexus-net\api\appfleet-nexus-api.Tests\appfleet-nexus-api.Tests.csproj

# Run specifically the Contact & Vehicle test fixtures
dotnet test c:\Learn\fleet_solution\app-fleet-nexus-net\api\appfleet-nexus-api.Tests\appfleet-nexus-api.Tests.csproj --filter "FullyQualifiedName~ContactsControllerTests|FullyQualifiedName~VehiclesControllerTests"
```
