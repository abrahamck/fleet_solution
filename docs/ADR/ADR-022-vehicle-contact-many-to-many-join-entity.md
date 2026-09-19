# ADR-022: Many-to-Many Vehicle-Contact Association via Join Entity

* **Status**: Accepted
* **Date**: 2026-09-19

---

## Context & Problem Statement

The Contact Management feature (FEATURE-001) requires associating one or more contacts (drivers, fleet managers, etc.) with one or more vehicles in a many-to-many relationship. Each association must also carry metadata: the role of the contact on that vehicle (`Driver` vs. `ResponsibleContact`), whether they are the primary assignee, and the date of assignment. Additionally, business rules mandate that every active vehicle must have at least one assigned contact at all times, while a contact may exist without any vehicle assignment (e.g., new hire or on-bench status).

A simple FK column on either `contacts` or `vehicles` cannot model this M:M relationship with the required role and primary-flag metadata, nor can it cleanly enforce the "vehicle must have ≥ 1 active contact" invariant.

---

## Decision

Introduce a dedicated **join entity** `VehicleContact` (`public.vehicle_contacts` table) that:

1. **Models the association** between `Vehicle` and `Contact` using composite foreign keys `(vehicle_id, contact_id)`.
2. **Carries association metadata**:
   - `association_role`: `TEXT` — `Driver` or `ResponsibleContact`.
   - `is_primary`: `BOOLEAN` — flags the primary assigned contact/driver for a vehicle.
   - `assigned_date`: `TIMESTAMPTZ` — when the association was established.
3. **Inherits `is_deleted` soft-delete** (consistent with ADR-014) rather than performing hard removes.
4. **Enforces the "at least 1 active contact" invariant** at the API service layer (checked before persisting any unassignment), not exclusively at the database layer, to provide a clear, user-facing error message.
5. **Applies tenant isolation** via EF Core Global Query Filters (`TenantId`) inherited through the `Vehicle` and `Contact` FK relationships, with an additional explicit `tenant_id` column on the join table for direct RLS enforcement.

---

## Alternatives Considered

* **FK Column on Vehicles (`assigned_contact_id`)**: Only supports one contact per vehicle (1:1). Cannot model multiple roles or multiple drivers on the same vehicle. Rejected.
* **FK Column on Contacts (`assigned_vehicle_id`)**: Only supports one vehicle per contact. Many real fleet scenarios (e.g., a fleet manager responsible for several vehicles) require a contact to be associated with multiple vehicles. Rejected.
* **JSONB Array of Contact IDs on Vehicles**: Loses referential integrity, cannot enforce FK constraints, not queryable with standard SQL joins, and breaks EF Core's relational model. Rejected.

---

## Consequences & Trade-offs

* **Positive Impacts**:
  * Clean relational model with full referential integrity.
  * Supports future metadata enrichment on the association (e.g., start/end dates, assignment type changes) without schema breaking changes.
  * Soft-delete on `vehicle_contacts` preserves a historical record of past assignments, aligning with ADR-014.
  * Enables dashboard queries for "active drivers" and "license expiration" by joining `vehicle_contacts` + `contacts`.
* **Risks & Trade-offs**:
  * Adds a third entity to manage in CRUD flows (UI must support multi-contact selection on the vehicle form).
  * "At least 1 active contact" validation must be enforced at the API layer; a database CHECK constraint would require a deferred trigger, which adds complexity.
* **Migration Paths**: The join table can be extended with additional columns (e.g., `assignment_notes`, `end_date`) without requiring structural changes to `vehicles` or `contacts`.
