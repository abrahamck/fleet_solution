# ADR-014: Soft-Delete and Audit Column Strategy

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
Fleet operations require strict historical accountability and referential integrity. Permanently purging records breaks relationships with past fuel logs, maintenance history, and trip records.

---

## Decision
Adopt a **soft-delete-by-default** model using a single `is_deleted` boolean flag on `BaseEntity`. Automatically populate audit columns (`CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`) in `FleetNexusDbContext.SaveChangesAsync`. Defer full entity temporal change logs to a future dedicated `audit_log` table.

---

## Alternatives Considered
* **Explicit Deleted Metadata**: Storing `deleted_at` and `deleted_by` columns directly on each entity.
* **Hard Delete Only**: Permanently deleting rows from PostgreSQL.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Prevents accidental data loss and preserves relational integrity across historical transactions.
  * EF Core Global Query Filters automatically exclude soft-deleted entities transparently.
  * Avoids redundant entity columns that will be superseded by future comprehensive temporal audit logs.
* **Risks & Trade-offs**: Requires partial database indexes (filtered by `WHERE NOT is_deleted`) to enforce natural key uniqueness.
* **Migration Paths**: Future addition of an `audit_log` table or PostgreSQL triggers to capture JSON diffs of state modifications.
