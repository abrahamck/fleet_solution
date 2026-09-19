# TASK-002: EF Core Migrations and PostgreSQL RLS Policies

> **Feature**: `FEATURE-001` ([requirements.md](../requirements.md))  
> **Status**: `READY`  
> **Order**: 002  
> **Dependencies**: `TASK-001`  

---

## 1. Objective & Boundaries

Generate the EF Core database migration for the new contact management schema and append PostgreSQL Row-Level Security (RLS) policies:
- Generate migration `AddContactManagementAndTypedContactPoints`.
- Add SQL commands in the migration for enabling RLS and creating tenant isolation policies on `contact_phones`, `contact_emails`, `contact_addresses`, and `vehicle_contacts`.
- Include safe migration of existing legacy columns if present.

*Boundary*: Do NOT implement API endpoints or UI views in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-001`, `REQ-007`, `REQ-010`, `REQ-013`
- **Target Acceptance Criteria**: `AC-013`
- **Design References**: [`design.md`](../design.md), [`ADR-004`](../../../docs/ADR/ADR-004-tenant-isolation-defense-in-depth.md), [`ADR-023`](../../../docs/ADR/ADR-023-polymorphic-contact-point-entity.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Migrations/*_AddContactManagementAndTypedContactPoints.cs` | Create | EF Core generated migration with tables, columns, indexes, and custom RLS SQL commands. |
| `app-fleet-nexus-net/api/appfleet-nexus-data/Migrations/FleetNexusDbContextModelSnapshot.cs` | Modify | Updated EF Core model snapshot. |

---

## 4. Required Tests & Evidence Proofs
- Migration generates cleanly via `dotnet ef migrations add`.
- Project builds cleanly with the new migration.

---

## 5. Constraints & Non-Negotiables
- Must include `ALTER TABLE <table> ENABLE ROW LEVEL SECURITY;` for all 4 new tables.
- Must include RLS policy: `CREATE POLICY <table>_tenant_isolation_policy ON public.<table> USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);`.

---

## 6. Definition of Done (DoD) Checklist
- [ ] Migration compiles and applies to database context.
- [ ] Model snapshot is synchronized.
- [ ] RLS policies and table structures are intact.
- [ ] Task status updated to `VERIFIED`.
