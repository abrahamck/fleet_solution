# ADR-012: FMCSA Census Dataset Isolation

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
The codebase contains a pre-existing FMCSA census carrier lookup dataset (`fmcsa_census` table, `Carrier` model, and `CarriersController`). A decision was required regarding whether to refactor, delete, or isolate this dataset in the multi-tenant architecture.

---

## Decision
Retain the `fmcsa_census` dataset in the codebase as an open lookup and learning reference, but **isolate it completely from the multi-tenancy security model**. It does not inherit from `BaseEntity`, does not carry a `tenant_id`, and has no RLS policies applied.

---

## Alternatives Considered
* **Not Available** (Deleting the FMCSA census table and controller entirely).

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Preserves experimental carrier lookup functionality without complicating or polluting the multi-tenant domain model.
* **Risks & Trade-offs**: Endpoints must be explicitly managed to prevent unauthenticated misuse or confused domain boundaries.
* **Migration Paths**: Not Available.
