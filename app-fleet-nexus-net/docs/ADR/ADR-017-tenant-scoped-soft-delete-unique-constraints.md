# ADR-017: Tenant-Scoped Soft-Delete Unique Constraints

* **Status**: Accepted
* **Date**: 2026-06-22

---

## Context & Problem Statement
Business entities like vehicles require unique natural identifiers within an organization (e.g. `unit_number` per tenant). Standard database unique constraints conflict with soft-deleted rows, preventing tenants from ever reusing a decommissioned unit number.

---

## Decision
Implement **partial unique indexes** in PostgreSQL filtered by `WHERE NOT is_deleted`:
```sql
CREATE UNIQUE INDEX idx_vehicles_tenant_unit
    ON public.vehicles (tenant_id, unit_number) WHERE NOT is_deleted;
```
Enforce parallel validation at the application API layer before saving changes.

---

## Alternatives Considered
* **Application-Only Checks**: Relying purely on LINQ queries without database-level constraints. Rejected due to concurrency race conditions.
* **Unconditional Unique Index**: Standard `UNIQUE (tenant_id, unit_number)` index. Rejected because it permanently blocks reuse of unit numbers from deleted vehicles.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Guarantees uniqueness for active vehicles while preserving historical audit records for deleted units.
  * Allows companies to reassign or reuse unit numbers after retiring older vehicles.
* **Risks & Trade-offs**: Requires PostgreSQL-specific partial index syntax in EF Core migrations.
* **Migration Paths**: Not Available.
