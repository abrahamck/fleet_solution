# ADR-003: Multi-Tenancy Strategy — Shared Schema with TenantId Column

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
FleetNexus serves multiple distinct fleet organizations. The system requires a scalable multi-tenancy architecture that guarantees tenant data isolation while keeping hosting and database migration costs minimal.

---

## Decision
Adopt a **Shared Database, Shared Schema** architecture using a mandatory `tenant_id` discriminator column on all tenant-scoped database tables.

---

## Alternatives Considered
* **Schema-per-Tenant**: Separate PostgreSQL schema per tenant. Rejected due to high schema migration complexity and maintenance overhead.
* **Database-per-Tenant**: Separate database instance per tenant. Rejected due to significant cost overhead ($25+/month per tenant).

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Zero additional hosting infrastructure cost on Supabase free tier.
  * Direct support in Entity Framework Core via Global Query Filters.
  * Single schema to migrate and maintain.
* **Risks**: Application errors could risk cross-tenant data access if isolation is not enforced at multiple layers.
* **Migration Paths**: Medium complexity. If enterprise requirements mandate schema-per-tenant isolation later, the `ITenantContext` abstraction allows routing EF Core to dynamic schemas.
