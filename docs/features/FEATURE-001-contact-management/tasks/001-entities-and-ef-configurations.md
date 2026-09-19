# TASK-001: Entity Models and EF Core Configurations

> **Feature**: `FEATURE-001` ([requirements.md](../requirements.md))  
> **Status**: `READY`  
> **Order**: 001  
> **Dependencies**: None  

---

## 1. Objective & Boundaries

Implement the five domain entity models and their Entity Framework Core mappings in `appfleet-nexus-data`:
- Update `Contact.cs` with full identity and driver compliance attributes.
- Create `ContactPhone.cs`, `ContactEmail.cs`, `ContactAddress.cs` as lean, typed, polymorphic contact point tables.
- Create `VehicleContact.cs` as the many-to-many join entity.
- Register all `DbSet` collections, snake_case table names, composite keys, tenant global query filters, and filtered indexes in `FleetNexusDbContext.cs`.

*Boundary*: Do NOT write migration scripts or API controllers in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-001`, `REQ-002`, `REQ-007`, `REQ-008`, `REQ-010`, `REQ-013`, `REQ-014`
- **Target Acceptance Criteria**: `AC-001`, `AC-002`, `AC-007`, `AC-008`, `AC-010`, `AC-013`
- **Design References**: [`design.md`](../design.md), [`ADR-014`](../../../docs/ADR/ADR-014-soft-delete-and-audit-strategy.md), [`ADR-022`](../../../docs/ADR/ADR-022-vehicle-contact-many-to-many-join-entity.md), [`ADR-023`](../../../docs/ADR/ADR-023-polymorphic-contact-point-entity.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Models/Contact.cs` | Modify | Add `UniqueId`, `MiddleName`, `ContactType`, `Status`, `JobTitle`, `LicenseNumber`, `LicenseState`, `LicenseExpirationDate`, `MedicalCertExpirationDate`, `EmergencyContactName`, `EmergencyContactPhone`, `Notes`. |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Models/ContactPhone.cs` | Create | Define `ContactPhone` inheriting `BaseEntity` (`OwnerType`, `OwnerId`, `Label`, `IsPrimary`, `PhoneNumber`). |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Models/ContactEmail.cs` | Create | Define `ContactEmail` inheriting `BaseEntity` (`OwnerType`, `OwnerId`, `Label`, `IsPrimary`, `EmailAddress`). |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Models/ContactAddress.cs` | Create | Define `ContactAddress` inheriting `BaseEntity` (`OwnerType`, `OwnerId`, `Label`, `IsPrimary`, `AddressLine1`..`4`, `City`, `State`, `PostalCode`, `Country`). |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Models/VehicleContact.cs` | Create | Define `VehicleContact` inheriting `BaseEntity` (`VehicleId`, `ContactId`, `AssociationRole`, `IsPrimary`, `AssignedDate`). |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Data/FleetNexusDbContext.cs` | Modify | Add `DbSet` properties, configure table names (`contact_phones`, `contact_emails`, `contact_addresses`, `vehicle_contacts`), configure filtered indexes, composite keys, and global query filters. |

---

## 4. Required Tests & Evidence Proofs
- `dotnet build app-fleet-nexus-net/api/appfleet-nexus-data/appfleet-nexus-data.csproj` compiles with 0 errors.
- Schema verification in test database (`Database.EnsureCreated()`).

---

## 5. Constraints & Non-Negotiables
- All entities must extend `BaseEntity` (ADR-014).
- Global query filters must enforce tenant isolation (`TenantId == CurrentTenantId`) and soft delete (`!IsDeleted`).
- Table names must adhere to snake_case convention in PostgreSQL.

---

## 6. Definition of Done (DoD) Checklist
- [ ] `appfleet-nexus-data` compiles cleanly without warnings or errors.
- [ ] All 5 entity models accurately reflect the design document schema.
- [ ] `FleetNexusDbContext` registers all DbSets, relationships, indexes, and query filters.
- [ ] Task status updated to `VERIFIED`.
