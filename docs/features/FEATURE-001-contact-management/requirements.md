# FEATURE-001 — Requirements: Contact Management & Vehicle Associations

> **Status**: `READY_FOR_REVIEW` *(Amendment v2 — ContactPoint separation)*  
> **Complexity**: `STANDARD`  
> **Created**: 2026-09-19  
> **Author**: AI Discovery Agent  
> **Gate 1 Approval**: [x] Approved by Human (Date: 2026-09-19)  
> **Amendment Note**: Requirements amended on 2026-09-19 after Gate 1 approval to separate the concept of a Contact from its ContactPoints (phone, email, address). This amendment also introduces ContactPoints on Vehicles (e.g., garage address). Design re-presented for Gate 2.

---

## 1. Executive Overview

### 1.1 Business Problem
Fleet organizations need to maintain a directory of business contacts—including vehicle drivers, fleet managers, dispatchers, vendors, and customers. Vehicles in the fleet must have accountable parties assigned to them (whether an operational driver or a designated responsible contact). Furthermore, fleet administrators need to track driver compliance attributes (such as license numbers and expiration dates) to maintain regulatory compliance and power safety metrics on the fleet dashboard.

A **Contact** is a person (or entity). The **ways to reach** that person (phone, email, mailing address) are distinct data points that can change independently, can have labels (e.g., "Mobile", "Office", "Emergency"), and may have more than one value per type. Similarly, a **Vehicle** itself can carry location/contact metadata (e.g., a garage address, a dispatch contact number), which shares the same structural concept.

This separation of **Contact identity** from **ContactPoints** (the reachability data) cleanly models real-world fleet directory requirements.

### 1.2 User Personas & Actors
- **Fleet Manager / Admin (Primary Actor)**: Creates, views, updates, and archives contacts; manages contact points per person; assigns one or more contacts/drivers to vehicles; monitors driver license expiration.
- **Operations Member (Secondary Actor)**: Views contact details and contact points, checks who is assigned to a vehicle.
- **System / Dashboard Engine (System Actor)**: Queries contact records to calculate driver counts, active driver metrics, and license expiration alerts for KPI tiles.

### 1.3 Out of Scope
- Driver / Contact portal authentication or Supabase login accounts (Contacts are business records, decoupled from system auth users per ADR-013).
- Automated telematics / ELD integration.
- `IsFleetWideContact` automatic assignment rule (excluded from MVP; contact associations are explicit).
- ContactPoints of type `SocialMedia`, `Website`, or other non-standard channels (deferred).

---

## 2. Behavioral Specifications

### 2.1 Domain Model & Attributes

#### A. `Contact` — Identity Entity
Represents a **person or business entity** managed within the tenant directory. Does NOT directly store communication channels or addresses — those are managed as `ContactPoint` records.

| Attribute | Type | Rules |
| :--- | :--- | :--- |
| `Id` | `Guid` | System PK |
| `TenantId` | `Guid` | Multi-tenant discriminator |
| `UniqueId` | `string?` | Human-readable code (e.g., `DRV-101`); max 50 chars; unique per tenant among active records |
| `FirstName` | `string` | Required; max 100 chars |
| `MiddleName` | `string?` | Optional; max 100 chars |
| `LastName` | `string` | Required; max 100 chars |
| `ContactType` | `string` | `Driver`, `FleetManager`, `Dispatcher`, `Customer`, `Vendor`, `Other` |
| `Status` | `string` | `Active`, `Inactive` (default: `Active`) |
| `JobTitle` | `string?` | e.g., "Heavy Truck Driver" |
| `LicenseNumber` | `string?` | Driver's License / CDL number |
| `LicenseState` | `string?` | Issuing state (2-letter) |
| `LicenseExpirationDate` | `DateOnly?` | Enables license expiration tracking |
| `MedicalCertExpirationDate` | `DateOnly?` | DOT Medical Card expiry |
| `EmergencyContactName` | `string?` | Emergency contact person's name |
| `EmergencyContactPhone` | `string?` | Emergency contact phone number |
| `Notes` | `string?` | Free-form notes |
| `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`, `IsDeleted` | Audit | Inherited from `BaseEntity` (ADR-014) |

#### B. `ContactPoint` — Reachability Entity
Represents **a single way of reaching an owner** (Contact or Vehicle). The owner is identified polymorphically via `OwnerType` + `OwnerId`.

| Attribute | Type | Rules |
| :--- | :--- | :--- |
| `Id` | `Guid` | System PK |
| `TenantId` | `Guid` | Multi-tenant discriminator |
| `OwnerType` | `string` | `Contact` or `Vehicle` |
| `OwnerId` | `Guid` | FK to the owning Contact or Vehicle |
| `ContactPointType` | `string` | `Phone`, `Email`, `Address` |
| `Label` | `string?` | e.g., `"Primary"`, `"Mobile"`, `"Office"`, `"Garage"`, `"Billing"` |
| `IsPrimary` | `bool` | Marks the primary contact point of this type for the owner (default: `false`) |
| — *Phone fields* — | | Active when `ContactPointType == Phone` |
| `PhoneNumber` | `string?` | Validated phone number |
| — *Email fields* — | | Active when `ContactPointType == Email` |
| `EmailAddress` | `string?` | Valid email address |
| — *Address fields* — | | Active when `ContactPointType == Address` |
| `AddressLine1` | `string?` | Street address |
| `AddressLine2` | `string?` | Suite / Apt / Unit |
| `AddressLine3` | `string?` | Optional |
| `AddressLine4` | `string?` | Optional |
| `City` | `string?` | City |
| `State` | `string?` | 2-letter postal abbreviation |
| `PostalCode` | `string?` | Zip / Postal code |
| `Country` | `string?` | Default: `USA` |
| `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`, `IsDeleted` | Audit | Inherited from `BaseEntity` (ADR-014) |

#### C. `VehicleContact` — Association Entity (Many-to-Many)
Represents the **assignment of a Contact to a Vehicle** with a designated role.

| Attribute | Type | Rules |
| :--- | :--- | :--- |
| `VehicleId` | `Guid` | FK to `vehicles` |
| `ContactId` | `Guid` | FK to `contacts` |
| `AssociationRole` | `string` | `Driver` or `ResponsibleContact` |
| `IsPrimary` | `bool` | Flags the primary lead for the vehicle |
| `AssignedDate` | `DateTime` | When the association was established |
| `IsDeleted` | `bool` | Soft-delete (ADR-014) |

---

### 2.2 Business Invariants

| # | Invariant |
| :--- | :--- |
| **I-001** | Every active `Contact` must have **at least 1 `Phone` ContactPoint** and **at least 1 `Address` ContactPoint** at all times. `Email` ContactPoints are optional. |
| **I-002** | Every active `Vehicle` must have **at least 1 active `VehicleContact` assignment**. |
| **I-003** | A `Contact` may have zero `VehicleContact` assignments (unassigned / on-bench state is valid). |
| **I-004** | Vehicle `ContactPoints` are optional; they represent location/logistics metadata (e.g., garage address, dispatch phone). |
| **I-005** | Soft-deleting a `Contact` who is the sole active assignment on a `Vehicle` is blocked until reassignment. |
| **I-006** | Soft-deleting a `ContactPoint` that is the sole `Phone` or `Address` on an active `Contact` is blocked. |
| **I-007** | `UniqueId` on Contact, when provided, must be unique per tenant across active (non-deleted) records. |

---

### 2.3 Happy Path Flows

#### Contact Lifecycle
1. User navigates to **Inventory > Contacts tab**.
2. User clicks **"Add Contact"**, fills out identity fields and driver compliance fields.
3. During contact creation, user adds **at least 1 Phone** and **at least 1 Address** as ContactPoints. Email is optional.
4. System validates, assigns tenant context, and persists Contact + ContactPoints.
5. Contact appears in the directory, available for vehicle assignment.

#### ContactPoint Management
1. User opens an existing Contact's detail view.
2. User clicks **"Add Contact Point"**, selects type (`Phone`, `Email`, `Address`), enters a label and value(s).
3. System validates and persists the ContactPoint.
4. User may set any one ContactPoint of a given type as `IsPrimary`.

#### Vehicle ContactPoint (e.g., Garage Address)
1. User opens an existing Vehicle's detail or edit view.
2. User adds an `Address`-type ContactPoint with label `"Garage"`.
3. System persists the ContactPoint with `OwnerType = Vehicle`, `OwnerId = {vehicleId}`.

#### Vehicle Association Lifecycle
1. When creating or editing a Vehicle, the user selects one or more contacts from the directory.
2. User designates `AssociationRole` (`Driver` or `ResponsibleContact`) and optionally flags one as `IsPrimary`.
3. At least one contact must be confirmed before the vehicle can be saved.
4. On save, `VehicleContact` records are persisted.

---

### 2.4 Boundary Conditions & Limits
- **ContactPoint minimum on Contact**: Creating a contact without providing ≥1 Phone AND ≥1 Address ContactPoint is rejected with HTTP 400.
- **ContactPoint removal guard**: Removing the last Phone or Address ContactPoint from an active Contact is blocked (HTTP 409).
- **Vehicle assignment minimum**: Vehicle cannot be saved with 0 active contact assignments.
- **UniqueId uniqueness**: Enforced via partial DB index (`WHERE NOT is_deleted`) + API pre-check.
- **IsPrimary uniqueness**: At most one ContactPoint per `(owner, contactPointType)` may be `IsPrimary`. Setting a new one as primary automatically clears the previous.

---

### 2.5 Failure Semantics & Error Handling
- **Missing required fields**: HTTP 400 with field-level validation errors.
- **Invariant violation**: HTTP 409 with an actionable user-facing message.
- **Duplicate UniqueId**: HTTP 400/409 with a clear duplicate message.
- **Cross-tenant access**: EF Core Global Query Filter returns HTTP 404 (entity not visible cross-tenant).

---

## 3. Non-Functional Requirements (NFRs)
- **Tenant Isolation**: All three entities (`Contact`, `ContactPoint`, `VehicleContact`) carry `TenantId` and are protected by EF Core Global Query Filters + PostgreSQL RLS (ADR-004).
- **Soft Delete & Audit**: All entities extend `BaseEntity` (ADR-014).
- **Performance**: Sub-100ms response for contact directory listing with contact point summaries eager-loaded.
- **Aesthetics & UX**: Rich Blazor UI within the unified inventory workspace (ADR-020), multi-step wizard for contact entry (ADR-018), responsive table + mobile cards.

---

## 4. Acceptance Criteria Matrix

| ID | Category | Condition / Trigger | Expected Outcome | Verification Mode |
| :--- | :--- | :--- | :--- | :--- |
| `AC-001` | Happy Path | Create Contact with required identity fields + ≥1 Phone + ≥1 Address | Contact and ContactPoints persisted; HTTP 201 | Automated API Test |
| `AC-002` | Happy Path | Create Contact with optional attributes (`UniqueId`, `MiddleName`, Email ContactPoint, `LicenseExpirationDate`) | All records persisted and retrievable | Automated API Test |
| `AC-003` | Validation | Create Contact without providing any Phone ContactPoint | HTTP 400 — Phone ContactPoint required | Automated API Test |
| `AC-004` | Validation | Create Contact without providing any Address ContactPoint | HTTP 400 — Address ContactPoint required | Automated API Test |
| `AC-005` | Validation | Create Contact without `FirstName` or `LastName` | HTTP 400 with field validation errors | Automated API Test |
| `AC-006` | Validation | Submit duplicate `UniqueId` within tenant | HTTP 400/409 — UniqueId already in use | Automated API Test |
| `AC-007` | ContactPoint | Add, update, and remove a ContactPoint (phone/email/address) on a Contact | CRUD operations succeed; IsPrimary flag enforced to 1 per type per owner | Automated API Test |
| `AC-008` | ContactPoint | Add an Address ContactPoint to a Vehicle (e.g., garage address) | ContactPoint persisted with `OwnerType=Vehicle`; visible on vehicle detail | Automated API Test |
| `AC-009` | Invariant | Attempt to remove last Phone ContactPoint from active Contact | HTTP 409 — At least 1 phone required | Automated API Test |
| `AC-010` | Association | Assign one or multiple contacts to a vehicle with roles | Many-to-many `VehicleContact` records created | Automated API Test |
| `AC-011` | Invariant | Create/update vehicle with zero contact assignments | HTTP 400 — Vehicle requires ≥1 contact | Automated Test & UI |
| `AC-012` | Invariant | Contact with no vehicle assignments can be created and saved | HTTP 201 — Unassigned state is valid | Automated API Test |
| `AC-013` | Security | User queries contacts/contactpoints | Only tenant-owned records returned | Automated Security Test |
| `AC-014` | Soft Delete | Soft-delete contact | `is_deleted = true`; removed from active directory | Automated API Test |
| `AC-015` | Soft Delete | Soft-delete Contact who is sole active vehicle contact | HTTP 409 — Blocked; reassignment required | Automated API Test |
| `AC-016` | UI | Navigate to `/inventory` > Contacts tab | Full contact directory with search, filters, table, mobile cards | UI Verification |

---

## 5. Documented Decisions & Open Questions

### Documented Business Decisions
- **D-001**: Contacts and Vehicles have a **many-to-many** association via `VehicleContact` with `AssociationRole`.
- **D-002**: Every vehicle must be assigned to at least **1 contact**.
- **D-003**: Contacts can exist without any assigned vehicles (unassigned / on bench).
- **D-004**: `IsFleetWideContact` is excluded from MVP.
- **D-005**: Status values for Contact are `Active` and `Inactive` only.
- **D-006**: Identifier field is named `UniqueId`.
- **D-007**: Contact communication channels and addresses are **separated into a `ContactPoint` entity** (polymorphic, with `OwnerType` + `OwnerId`). Inline phone/email/address fields are removed from the Contact entity.
- **D-008**: Vehicles can optionally carry `ContactPoint` records (e.g., garage address, dispatch phone) using the same `ContactPoint` entity with `OwnerType = Vehicle`.
- **D-009**: A `Contact` **must** have ≥1 Phone and ≥1 Address ContactPoint. Email is optional.
- **D-010**: At most 1 ContactPoint per type per owner may be marked `IsPrimary`.

### Open Questions
- [x] **Q-001**: Middle Name? → Yes, optional.
- [x] **Q-002**: Status include "On Leave"? → No — `Active` / `Inactive` only.
- [x] **Q-003**: Distinguish Driver vs Responsible Contact? → `AssociationRole` on `VehicleContact`.
- [x] **Q-007**: Separate Contact from ContactPoint? → Yes (D-007, D-008, D-009).
- [ ] **Q-004**: For existing seeded vehicles without contacts — seed a default demo contact to satisfy the ≥1 contact invariant?
- [ ] **Q-008**: Should `Vehicle` also require at least 1 ContactPoint (e.g., a garage address), or are Vehicle ContactPoints always optional?
