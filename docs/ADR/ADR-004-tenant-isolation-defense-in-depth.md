# ADR-004: Multi-Tenant Data Isolation — Dual-Layer Defense-in-Depth

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
In a shared database/schema multi-tenant architecture, relying solely on application-level filtering creates a risk where developer error or raw SQL execution could accidentally expose or mutate another organization's fleet data.

---

## Decision
Implement a **dual-layer defense-in-depth isolation model**:
1. **Application Layer**: EF Core Global Query Filters automatically appending `WHERE tenant_id = @current AND is_deleted = false` to all queries on entities inheriting from `BaseEntity`.
2. **Database Layer**: PostgreSQL Row-Level Security (RLS) policies with `FORCE ROW LEVEL SECURITY` enforcing tenant isolation based on transaction-scoped `app.current_tenant_id` session settings.

---

## Alternatives Considered
* **Application-Level Only**: EF Core Global Query Filters without RLS. Rejected due to vulnerability to raw SQL queries or manual filter bypasses.
* **Database-Level Only**: PostgreSQL RLS without EF Core filters. Rejected because application-level filters improve developer experience, linq testing, and query generation.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Industry standard defense-in-depth security.
  * Database guarantees isolation even if application code contains query bugs.
* **Risks & Trade-offs**:
  * Database connections must explicitly execute `SET LOCAL app.current_tenant_id` on every transactional scope via middleware/interceptors.
* **Migration Paths**: Not Available.
