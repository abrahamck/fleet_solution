# TASK-004: Contacts API Controller & Invariant Enforcement

> **Feature**: `FEATURE-001` ([requirements.md](../requirements.md))  
> **Status**: `READY`  
> **Order**: 004  
> **Dependencies**: `TASK-001`, `TASK-003`  

---

## 1. Objective & Boundaries

Implement `ContactsController.cs` in `appfleet-nexus-api` to support all Contact directory operations and business invariants:
- `GET /api/contacts`: Directory query with search (`q`), type filter (`contactType`), status filter (`status`), with eager-loaded primary contact points and vehicle assignment counts.
- `GET /api/contacts/{id}`: Detailed contact view including all phone numbers, email addresses, physical addresses, and assigned vehicles.
- `POST /api/contacts`: Create new contact and validate required invariants:
  - Minimum invariant: Must have ≥ 1 Phone ContactPoint and ≥ 1 Address ContactPoint (`AC-003`, `AC-004`).
  - Tenant UniqueId uniqueness check (`AC-006`).
  - Single `IsPrimary` flag per contact point type per owner.
  - Optional initial vehicle assignments.
- `PUT /api/contacts/{id}`: Update contact identity, sync contact point collections, and sync vehicle assignments.
  - Guard: Must not remove the last Phone or last Address (`AC-009`).
- `DELETE /api/contacts/{id}`: Soft-delete contact.
  - Guard: Block deletion (`409 Conflict`) if the contact is the sole active assigned contact on any active vehicle (`AC-015`).
- `GET /api/contacts/summary`: Provide counts for dashboard KPI tiles (total contacts, active drivers, licenses expiring in 30 days).

*Boundary*: Do NOT modify `VehiclesController.cs` or UI files in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-001` through `REQ-007`, `REQ-009`, `REQ-012`, `REQ-013`, `REQ-014`, `REQ-015`
- **Target Acceptance Criteria**: `AC-001`, `AC-002`, `AC-003`, `AC-004`, `AC-005`, `AC-006`, `AC-007`, `AC-009`, `AC-012`, `AC-013`, `AC-014`, `AC-015`
- **Design References**: [`design.md`](../design.md) (Section 3.1 — Contacts Controller)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-api/Controllers/ContactsController.cs` | Create | Implement full REST endpoints with OpenAPI documentation, error handling, tenant isolation, and invariant guards. |

---

## 4. Required Tests & Evidence Proofs
- `dotnet build app-fleet-nexus-net/api/appfleet-nexus-api/appfleet-nexus-api.csproj` compiles with 0 errors.

---

## 5. Constraints & Non-Negotiables
- Controller must be decorated with `[Authorize]`, `[ApiController]`, `[Route("api/contacts")]`.
- Invariant I-001 (≥1 Phone AND ≥1 Address) must be strictly enforced before persisting.
- Invariant I-005 (Sole vehicle assignee delete protection) must be checked before soft deleting.
- Primary flag enforcement: Setting a new primary phone/email/address must clear any existing primary flag of that type for the owner.

---

## 6. Definition of Done (DoD) Checklist
- [ ] `ContactsController.cs` created with all endpoints.
- [ ] All validation guards and invariant checks implemented with proper HTTP status codes.
- [ ] Task status updated to `VERIFIED`.
