# FleetNexus — Agent Instructions

> This file is automatically loaded by Gemini/Antigravity at the start of every conversation.
> It defines coding standards, architectural patterns, and best practices for this project.

---

## Project Overview

FleetNexus is a multi-tenant fleet management SaaS application.

- **Frontend**: Blazor WebAssembly (SPA) served via Nginx on Render
- **Backend**: ASP.NET Core 10 Web API on Render (Docker)
- **Database**: PostgreSQL on Supabase (managed)
- **Auth**: Supabase Auth (JWT-based, HS256)

### Solution Structure

```
app-fleet-nexus-net/
├── api/
│   ├── appfleet-nexus-api/          → Web API (controllers, Program.cs)
│   ├── appfleet-nexus-data/         → Data layer (EF Core, models, repositories)
│   └── appfleet-nexus-security/     → Security library (auth, tenancy, middleware)
├── ui/                              → Blazor WASM frontend
└── docs/                            → Architecture docs, implementation plans
```

### Key Architecture Documents

- `docs/SecurityArchitectureDecision.md` — All security architecture decisions with reasoning
- `docs/implementation/SecurityImplementationPlan.md` — Phased implementation plan with DB schema
- `docs/infrastructure_architecture.md` — Production hosting & deployment architecture

---

## C# Coding Standards

### Naming Conventions

- **PascalCase**: Classes, methods, properties, events, enums, public fields
- **camelCase with `_` prefix**: Private fields (e.g., `_repository`, `_logger`)
- **camelCase**: Local variables, method parameters
- **UPPER_CASE**: Never — use PascalCase for constants too (e.g., `MaxRetryCount`)
- **Interfaces**: Prefix with `I` (e.g., `ICarrierRepository`, `ITenantContext`)
- **Async methods**: Suffix with `Async` (e.g., `GetCarrierByDotNumberAsync`)
- **DTOs**: Use C# `record` types (e.g., `record SignupRequest(string Email, string Password)`)

### Code Organization

- **One class per file**. File name must match class name.
- **Namespace = folder path**. Use file-scoped namespaces (`namespace X;` not `namespace X { }`).
- **Order within a class**: Constants → Fields → Constructor → Public methods → Private methods.
- **Use `var`** when the type is obvious from the right side. Use explicit types when it improves clarity.

### Patterns & Practices

- **Repository pattern**: All database access goes through repository interfaces (e.g., `ICarrierRepository`). Controllers never access `DbContext` directly.
- **Dependency injection**: Use constructor injection exclusively. Register services in `Program.cs` or via extension methods.
- **Async all the way**: All I/O-bound operations must be async. Never use `.Result` or `.Wait()`.
- **Guard clauses**: Validate inputs at the top of methods. Return early for invalid inputs.
- **DTOs for API boundaries**: Never expose EF Core entities directly in API responses. Map to DTOs/records.
- **Minimal try-catch**: Only catch exceptions you can handle. Let the global exception handler deal with unexpected errors.
- **No magic strings**: Use constants or `nameof()`. Role names use `Roles.Admin`, `Roles.Member`, etc.

### Multi-Tenancy Rules

- **All new tenant-scoped entities MUST inherit from `BaseEntity`**. This gives them `Id`, `TenantId`, `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`, and `IsDeleted` automatically.
- **Never manually set audit columns** in controllers or services. The `FleetNexusDbContext.SaveChangesAsync()` override handles this.
- **Never hardcode tenant IDs**. Always use `ITenantContext.TenantId` from the DI container.
- **Global query filters** are registered in `FleetNexusDbContext.OnModelCreating()`. Every `BaseEntity`-derived entity must have a filter: `e.TenantId == currentTenantId && !e.IsDeleted`.
- **To bypass filters** (e.g., admin queries), use `.IgnoreQueryFilters()` explicitly and document why.

### EF Core Guidelines

- **No lazy loading**. Use `.Include()` for eager loading or projection with `.Select()`.
- **Use `Guid` primary keys** with `DEFAULT gen_random_uuid()` on the DB side.
- **Use `DateTime` (UTC)** for all timestamp properties. The DB stores `TIMESTAMPTZ`.
- **Migrations**: Name them descriptively (e.g., `AddVehicleTable`, `AddSoftDeleteToContacts`).
- **Never use raw SQL** unless absolutely necessary. If you must, parameterize everything.

### Error Handling

- **API responses**: Return consistent shapes: `{ success: true, data: ... }` or `{ success: false, message: "..." }`.
- **Never leak stack traces** in non-development environments.
- **Log errors with Serilog**: Use structured logging with `_logger.LogError(ex, "Message {Param}", value)`.

### XML Documentation

- **Required on**: All public interfaces, public methods, and DTOs.
- **Format**: Use `<summary>` for the main description. Use `<param>` for parameters. Use `<returns>` for return values.

---

## PostgreSQL / SQL Coding Standards

### Naming Conventions

- **snake_case** for everything: tables, columns, functions, triggers, indexes, constraints
- **Tables**: Plural nouns (e.g., `tenants`, `users`, `vehicles`, `contacts`)
- **Columns**: Descriptive, no abbreviations (e.g., `first_name` not `fname`, `created_date` not `cdt`)
- **Primary keys**: Always `id` (UUID)
- **Foreign keys**: `<referenced_table_singular>_id` (e.g., `tenant_id`, `user_id`)
- **Indexes**: `idx_<table>_<columns>` (e.g., `idx_vehicles_tenant`)
- **Constraints**: `fk_<table>_<referenced>` for foreign keys, `chk_<table>_<rule>` for checks
- **Functions**: `<verb>_<noun>` (e.g., `handle_new_user`, `cleanup_expired_invitations`)
- **Triggers**: `on_<table>_<event>` (e.g., `on_auth_user_created`)

### Data Types

- **IDs**: `UUID` with `DEFAULT gen_random_uuid()` (never `SERIAL` or `BIGINT`)
- **Timestamps**: Always `TIMESTAMPTZ` (never bare `TIMESTAMP`). Use `NOW()` for defaults.
- **Strings**: `TEXT` (never `VARCHAR` without a compelling reason). Add `CHECK` constraints if length matters.
- **Booleans**: `BOOLEAN NOT NULL DEFAULT FALSE` — always provide a default.
- **Money**: `NUMERIC(12,2)` — never `FLOAT` or `DOUBLE`.

### Table Structure

Every tenant-scoped table must include:

```sql
-- Required columns for tenant-scoped tables
id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
tenant_id       UUID NOT NULL REFERENCES public.tenants(id) ON DELETE CASCADE,
created_by      UUID NOT NULL,
created_date    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
modified_by     UUID,
modified_date   TIMESTAMPTZ,
is_deleted      BOOLEAN NOT NULL DEFAULT FALSE
```

### Functions & Triggers

- **Always use `SECURITY DEFINER`** on functions that access `auth` schema tables.
- **Always set `SET search_path = public`** to prevent search_path injection attacks.
- **Use `LANGUAGE plpgsql`** for procedural logic. Use `LANGUAGE sql` for simple single-statement functions.
- **Prefix local variables** with `v_` (e.g., `v_tenant_id`, `v_first_name`).
- **Add `COMMENT ON`** for every table, function, and non-obvious column.
- **Make functions idempotent** where possible (use `CREATE OR REPLACE`, `DROP IF EXISTS`).

### Row-Level Security (RLS)

- **Every tenant-scoped table MUST have RLS enabled**: `ALTER TABLE ... ENABLE ROW LEVEL SECURITY;`
- **Also FORCE RLS**: `ALTER TABLE ... FORCE ROW LEVEL SECURITY;` (required when the API connects as the table owner).
- **Policy pattern**: Check `current_setting('app.current_tenant_id', true)` — the second argument (`true`) prevents errors when the setting is unset.
- **SuperAdmin bypass**: Add `OR current_setting('app.is_super_admin', true) = 'true'` to all policies.
- **Separate policies per operation**: Create individual policies for SELECT, INSERT, UPDATE, DELETE (not a single `FOR ALL`).

### Indexing

- **Always index `tenant_id`** on tenant-scoped tables.
- **Use partial indexes** with `WHERE NOT is_deleted` to exclude soft-deleted rows.
- **Use partial unique indexes** to handle uniqueness + soft-delete (e.g., `UNIQUE (tenant_id, unit_number) WHERE NOT is_deleted`).

---

## Blazor / Frontend Standards

- **No direct Supabase calls** from the Blazor app. All auth and data flows go through the .NET API.
- **Store JWTs** in memory (not localStorage) for security. Use refresh tokens for persistence.
- **Use `HttpClient` with `AuthorizationMessageHandler`** for authenticated API calls.

---

## Git & Workflow

- **Never commit secrets**. Use `appsettings.Local.json` (gitignored) for local dev. Use Render environment variables for production.
- **Meaningful commit messages**: Use imperative mood (e.g., "Add tenant middleware", not "Added tenant middleware").
- **Configuration hierarchy**: `appsettings.json` (base) → `appsettings.{Environment}.json` (overrides) → `appsettings.Local.json` (local secrets, gitignored) → Environment variables (production secrets).
