# TASK-003: DTOs and Request Validation Models

> **Feature**: `FEATURE-001` ([requirements.md](../requirements.md))  
> **Status**: `READY`  
> **Order**: 003  
> **Dependencies**: `TASK-001`  

---

## 1. Objective & Boundaries

Define all Data Transfer Objects (DTOs), request contracts, and validation attributes in `appfleet-nexus-api`:
- `ContactListDto`, `ContactDetailDto`, `ContactSummaryDto`.
- `ContactUpsertRequest`, `ContactPhoneDto`, `ContactEmailDto`, `ContactAddressDto`.
- `VehicleContactAssignmentDto` and extended `VehicleUpsertRequest` / `VehicleDetailDto`.
- Field-level validation rules (lengths, formats, required properties).

*Boundary*: Do NOT implement controller route handling or UI components in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-001`, `REQ-002`, `REQ-003`, `REQ-004`, `REQ-005`, `REQ-007`, `REQ-010`
- **Target Acceptance Criteria**: `AC-001`, `AC-002`, `AC-003`, `AC-004`, `AC-005`, `AC-007`, `AC-010`
- **Design References**: [`design.md`](../design.md) (Section 3 — API Endpoints & DTOs)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-api/Models/ContactDtos.cs` | Create | Define all Contact DTOs (`ContactUpsertRequest`, `ContactDetailDto`, `ContactListDto`, `ContactPhoneDto`, `ContactEmailDto`, `ContactAddressDto`, `VehicleContactAssignmentDto`, `ContactSummaryDto`). |
| `app-fleet-nexus-net/api/appfleet-nexus-api/Models/VehicleDtos.cs` | Create / Modify | Define/extend `VehicleUpsertRequest` with `AssignedContacts` and `ContactAddresses` (for garage address). |

---

## 4. Required Tests & Evidence Proofs
- `dotnet build app-fleet-nexus-net/api/appfleet-nexus-api/appfleet-nexus-api.csproj` compiles with 0 errors.

---

## 5. Constraints & Non-Negotiables
- Validation annotations must enforce:
  - `FirstName`, `LastName` required, max 100 chars.
  - `UniqueId` max 50 chars.
  - `PhoneNumber` on `ContactPhoneDto` required, max 30 chars.
  - `EmailAddress` on `ContactEmailDto` required, valid email format, max 256 chars.
  - `AddressLine1`, `City`, `State` (2 letters), `PostalCode` required on `ContactAddressDto`.

---

## 6. Definition of Done (DoD) Checklist
- [ ] All DTO classes created with complete validation attributes.
- [ ] `appfleet-nexus-api` compiles with 0 errors.
- [ ] Task status updated to `VERIFIED`.
