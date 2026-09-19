# FEATURE-001 — Implementation Plan: Contact Management & Vehicle Associations

> **Status**: `READY_FOR_EXECUTION`  
> **Requirements**: [`requirements.md`](requirements.md) (Status: `APPROVED`, Amendment v2)  
> **Design**: [`design.md`](design.md) (Status: `APPROVED`, v3 Typed Contact Points)  
> **Created**: 2026-09-19  
> **Author**: AI Planning Agent  
> **Governing ADRs**: [ADR-003](../../ADR/ADR-003-multi-tenancy-shared-schema.md), [ADR-004](../../ADR/ADR-004-tenant-isolation-defense-in-depth.md), [ADR-013](../../ADR/ADR-013-user-profile-and-contact-separation.md), [ADR-014](../../ADR/ADR-014-soft-delete-and-audit-strategy.md), [ADR-017](../../ADR/ADR-017-tenant-scoped-soft-delete-unique-constraints.md), [ADR-018](../../ADR/ADR-018-multi-step-input-wizard-ux-pattern.md), [ADR-020](../../ADR/ADR-020-unified-inventory-workspace.md), [ADR-022](../../ADR/ADR-022-vehicle-contact-many-to-many-join-entity.md), [ADR-023](../../ADR/ADR-023-polymorphic-contact-point-entity.md)

---

## 1. Definition of Ready (DoR) Checklist

All preconditions for decomposition and execution are satisfied:
- [x] Requirements document exists and is marked `APPROVED` (Gate 1 passed on 2026-09-19).
- [x] Design document exists and is marked `APPROVED` (Gate 2 passed on 2026-09-19).
- [x] Governing ADRs identified and all marked `Accepted` (ADR-022 and ADR-023 accepted).
- [x] Scope and acceptance criteria are disambiguated with exact schema contracts.
- [x] Invariants codified (≥1 phone + ≥1 address per contact; ≥1 contact per vehicle).

---

## 2. Technical Scope & Change Inventory

### 2.1 Database & Schema (`appfleet-nexus-data`)

```
app-fleet-nexus-net/api/appfleet-nexus-data/
├── Models/
│   ├── Contact.cs                        # [MODIFY] Add UniqueId, MiddleName, ContactType, Status, Driver compliance fields
│   ├── ContactPhone.cs                   # [NEW] Polymorphic phone table (owner_type, owner_id, label, is_primary, phone_number)
│   ├── ContactEmail.cs                   # [NEW] Polymorphic email table (owner_type, owner_id, label, is_primary, email_address)
│   ├── ContactAddress.cs                 # [NEW] Polymorphic address table (owner_type, owner_id, label, is_primary, address fields)
│   └── VehicleContact.cs                 # [NEW] M:M join entity (vehicle_id, contact_id, association_role, is_primary, assigned_date)
├── Data/
│   └── FleetNexusDbContext.cs            # [MODIFY] Register 4 new DbSets, table mappings, composite PKs, filtered indexes, tenant query filters
└── Migrations/
    └── 20260919_AddContactManagementAndTypedContactPoints.cs # [NEW] EF Core migration + PostgreSQL RLS policies
```

#### Detailed Entity Specifications:
1. **`Contact`** (`contacts` table):
   - `UniqueId` (`string?`, max 50): Human-readable identifier (e.g., `DRV-101`). Unique per tenant when active.
   - `FirstName` (`string`, max 100): Required.
   - `MiddleName` (`string?`, max 100): Optional.
   - `LastName` (`string`, max 100): Required.
   - `ContactType` (`string`, max 50): Default `"Driver"`. Options: `Driver`, `FleetManager`, `Dispatcher`, `Customer`, `Vendor`, `Other`.
   - `Status` (`string`, max 20): Default `"Active"`. Options: `Active`, `Inactive`.
   - `JobTitle` (`string?`, max 100).
   - `LicenseNumber` (`string?`, max 50).
   - `LicenseState` (`string?`, max 2).
   - `LicenseExpirationDate` (`DateOnly?`).
   - `MedicalCertExpirationDate` (`DateOnly?`).
   - `EmergencyContactName` (`string?`, max 100).
   - `EmergencyContactPhone` (`string?`, max 30).
   - `Notes` (`string?`, max 1000).
   - Inherits `BaseEntity` (`Id`, `TenantId`, `IsDeleted`, audit timestamps & users).

2. **`ContactPhone`** (`contact_phones` table):
   - `OwnerType` (`string`, max 50): `"Contact"` or `"Vehicle"`.
   - `OwnerId` (`Guid`): FK to owning Contact or Vehicle.
   - `Label` (`string?`, max 50): `"Mobile"`, `"Office"`, `"Home"`, `"Dispatch"`, etc.
   - `IsPrimary` (`bool`): Default `false`.
   - `PhoneNumber` (`string`, max 30): Required.
   - Inherits `BaseEntity`.

3. **`ContactEmail`** (`contact_emails` table):
   - `OwnerType` (`string`, max 50): `"Contact"` or `"Vehicle"`.
   - `OwnerId` (`Guid`): FK to owning Contact or Vehicle.
   - `Label` (`string?`, max 50): `"Primary"`, `"Work"`, `"Personal"`, `"Billing"`, etc.
   - `IsPrimary` (`bool`): Default `false`.
   - `EmailAddress` (`string`, max 256): Required.
   - Inherits `BaseEntity`.

4. **`ContactAddress`** (`contact_addresses` table):
   - `OwnerType` (`string`, max 50): `"Contact"` or `"Vehicle"`.
   - `OwnerId` (`Guid`): FK to owning Contact or Vehicle.
   - `Label` (`string?`, max 50): `"Physical"`, `"Mailing"`, `"Garage"`, `"HQ"`, etc.
   - `IsPrimary` (`bool`): Default `false`.
   - `AddressLine1` (`string`, max 200): Required.
   - `AddressLine2` (`string?`, max 200): Optional.
   - `AddressLine3` (`string?`, max 200): Optional.
   - `AddressLine4` (`string?`, max 200): Optional.
   - `City` (`string`, max 100): Required.
   - `State` (`string`, max 2): Required (2-letter state code).
   - `PostalCode` (`string`, max 20): Required.
   - `Country` (`string`, max 50): Default `"USA"`.
   - Inherits `BaseEntity`.

5. **`VehicleContact`** (`vehicle_contacts` table):
   - `VehicleId` (`Guid`): FK to `vehicles`.
   - `ContactId` (`Guid`): FK to `contacts`.
   - `AssociationRole` (`string`, max 50): `"Driver"` or `"ResponsibleContact"`.
   - `IsPrimary` (`bool`): Default `false`.
   - `AssignedDate` (`DateTime`): UTC timestamp when assigned.
   - Inherits `BaseEntity` (Composite PK `(VehicleId, ContactId)` or single PK with unique index).

---

### 2.2 Security & Multi-Tenancy (`appfleet-nexus-security` & Data Layer)

- **EF Core Global Query Filters**:
  ```csharp
  modelBuilder.Entity<ContactPhone>().HasQueryFilter(e => e.TenantId == CurrentTenantId && !e.IsDeleted);
  modelBuilder.Entity<ContactEmail>().HasQueryFilter(e => e.TenantId == CurrentTenantId && !e.IsDeleted);
  modelBuilder.Entity<ContactAddress>().HasQueryFilter(e => e.TenantId == CurrentTenantId && !e.IsDeleted);
  modelBuilder.Entity<VehicleContact>().HasQueryFilter(e => e.TenantId == CurrentTenantId && !e.IsDeleted);
  ```
- **PostgreSQL RLS Policies**:
  - `contact_phones_tenant_isolation_policy` on `contact_phones`
  - `contact_emails_tenant_isolation_policy` on `contact_emails`
  - `contact_addresses_tenant_isolation_policy` on `contact_addresses`
  - `vehicle_contacts_tenant_isolation_policy` on `vehicle_contacts`

---

### 2.3 API & Business Logic Layer (`appfleet-nexus-api`)

```
app-fleet-nexus-net/api/appfleet-nexus-api/
├── Controllers/
│   ├── ContactsController.cs             # [NEW] Endpoints for Contact CRUD, search, filter, contact points & vehicle assignments
│   └── VehiclesController.cs             # [MODIFY] Add contact assignment & vehicle contact point handling + invariant enforcement
└── Models/
    ├── ContactDtos.cs                    # [NEW] ContactListDto, ContactDetailDto, ContactUpsertRequest, ContactPhoneDto, ContactEmailDto, ContactAddressDto, VehicleContactAssignmentDto
    └── VehicleDtos.cs                    # [MODIFY/NEW] Extend VehicleUpsertRequest & VehicleResponseDto with AssignedContacts & ContactPoints
```

#### API Endpoints Contract:
| Method | Route | Description | Status Codes |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/contacts` | List contacts with optional search query, type filter, status filter; includes primary phone, email, address summary, and assigned vehicle count | `200 OK` |
| `GET` | `/api/contacts/{id}` | Full contact detail with phones, emails, addresses, and assigned vehicle list | `200 OK`, `404 Not Found` |
| `POST` | `/api/contacts` | Create contact + contact points (requires ≥1 phone, ≥1 address) + optional vehicle assignments | `201 Created`, `400 Bad Request` |
| `PUT` | `/api/contacts/{id}` | Update contact identity + sync contact points + sync vehicle assignments | `200 OK`, `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| `DELETE` | `/api/contacts/{id}` | Soft-delete contact. Blocked (409) if contact is sole active assignee on any active vehicle | `200 OK`, `404 Not Found`, `409 Conflict` |
| `GET` | `/api/contacts/summary` | Summary stats (total contacts, active drivers, expiring licenses in 30 days) for KPI tiles | `200 OK` |
| `GET` | `/api/vehicles/{id}` | Returns vehicle with assigned contacts and vehicle contact points (garage address, etc.) | `200 OK`, `404 Not Found` |
| `POST` | `/api/vehicles` | Extended to validate and persist assigned contacts (≥1 required) and optional vehicle contact points | `201 Created`, `400 Bad Request` |
| `PUT` | `/api/vehicles/{id}` | Extended to validate and sync assigned contacts (≥1 required) and vehicle contact points | `200 OK`, `400 Bad Request`, `404 Not Found` |

---

### 2.4 User Interface Layer (`appfleet-nexus-ui`)

```
app-fleet-nexus-net/ui/appfleet-nexus-ui/
├── Pages/
│   ├── Inventory.razor                   # [MODIFY] Activate Contacts tab (search bar, filter pills, summary metrics, data grid, mobile cards)
│   └── Inventory.razor.css               # [MODIFY] Styles for contact badges, contact points summary, vehicle assignment pills
├── Components/
│   ├── ContactDialog.razor               # [NEW] Multi-step wizard dialog (Identity, Contact Points, Driver Compliance, Emergency & Notes, Assignments)
│   └── VehicleDialog.razor               # [MODIFY] Add contact assignment section (Driver / Responsible Contact) & Garage Address input
└── Models/
    ├── ContactModels.cs                  # [NEW] Client-side models matching API DTOs
    └── VehicleModels.cs                  # [MODIFY] Add AssignedContactModel and ContactAddressModel
```

---

### 2.5 Test Harness (`appfleet-nexus-api.Tests`)

```
app-fleet-nexus-net/api/appfleet-nexus-api.Tests/
└── Controllers/
    ├── ContactsControllerTests.cs        # [NEW] SQLite in-memory tests for AC-001 through AC-015
    └── VehiclesControllerTests.cs        # [MODIFY] Tests for vehicle-contact invariant enforcement (≥1 contact required, sole delete blocked)
```

---

## 3. Ordered Task Breakdown Summary

| Task ID | Task Title | Primary Projects / Files | Dependencies | DoD Proof |
| :--- | :--- | :--- | :--- | :--- |
| `TASK-001` | Entity Models & EF Configurations | `appfleet-nexus-data`: Models, DbContext | None | Entities compile, DbSets wired |
| `TASK-002` | EF Core Migration & Seeding | `appfleet-nexus-data`: Migrations, SQL | `TASK-001` | Migration generated & schema verified |
| `TASK-003` | DTOs & API Request Validators | `appfleet-nexus-api`: Models/DTOs | `TASK-001` | DTO contracts & validations build |
| `TASK-004` | Contacts API Controller & Invariants | `appfleet-nexus-api`: ContactsController | `TASK-002`, `TASK-003` | CRUD endpoints return correct codes |
| `TASK-005` | Vehicles API Controller Extensions | `appfleet-nexus-api`: VehiclesController | `TASK-004` | Vehicle contact invariant enforced |
| `TASK-006` | Automated Unit & Integration Tests | `appfleet-nexus-api.Tests`: ControllerTests | `TASK-004`, `TASK-005` | 100% test suite passing |
| `TASK-007` | Blazor UI Contacts Tab & ContactDialog | `appfleet-nexus-ui`: Inventory, ContactDialog | `TASK-004` | UI renders and interacts seamlessly |
| `TASK-008` | Blazor UI VehicleDialog Extensions | `appfleet-nexus-ui`: VehicleDialog | `TASK-005`, `TASK-007` | Vehicle contact assignment in UI |
| `TASK-009` | End-to-End Verification & Evidence | Full solution | `TASK-001` - `TASK-008` | `evidence.md` completed |

---

## 4. Risks, Migrations & Rollback Strategy

- **Migration Hazards**:
  - `contacts` table is modified in-place: adding new columns with default/nullable attributes is non-destructive.
  - Existing `primary_phone` and `primary_email` data (if any demo rows exist) can be migrated to `contact_phones` and `contact_emails` via EF migration SQL script before dropping the legacy columns.
  - Foreign key constraints on `vehicle_contacts` use `ON DELETE RESTRICT` or soft delete to prevent orphan cascades.
- **Rollback Procedure**:
  - Migration rollback: `dotnet ef database update <PreviousMigrationName>`.
  - Application rollback: Feature can be safely reverted by git branch rollback since new tables are additive.
