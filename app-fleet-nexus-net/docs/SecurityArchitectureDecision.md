# FleetNexus — Security Architecture Decision Record

> **Status**: Approved  
> **Date**: 2026-06-21  
> **Scope**: Authentication, Authorization, Multi-Tenancy, Data Isolation  

---

## 1. High-Level Security Strategy

### General Approach

FleetNexus adopts a **zero-cost, defense-in-depth** security model built entirely on existing infrastructure (Supabase + ASP.NET Core + PostgreSQL). The strategy avoids introducing new paid services and instead leverages free, battle-tested primitives already available in the stack.

### Core Principles

- **External identity provider** — authentication is delegated to Supabase Auth; we never store or handle passwords ourselves
- **Stateless API authentication** — every API request carries a JWT; the API validates it locally using Supabase's signing key with no session state
- **API-centric auth orchestration** — the Blazor WASM frontend never talks to Supabase directly; all auth flows route through the .NET API, which is the single point of control
- **Two-layer tenant isolation** — every tenant-scoped query is filtered at both the application layer (EF Core Global Query Filters) and the database layer (PostgreSQL Row-Level Security)
- **Audit-by-default** — all tenant-scoped entities carry `CreatedBy`, `ModifiedBy`, `CreatedDate`, `ModifiedDate` populated automatically via `SaveChanges` override
- **Soft-delete-by-default** — no tenant data is ever physically removed; deletions set `IsDeleted = true`, filtered out globally by EF Core query filters. A future **audit history table** will capture who performed the deletion and when.
- **Least-privilege default** — all API endpoints require authentication by default (global fallback policy); only explicitly marked endpoints (`[AllowAnonymous]`) are public
- **CORS lockdown** — API accepts requests only from known frontend origins (`fleetnexus.io`, `localhost` for dev)
- **Role-based access** — two application roles per tenant (`Admin`, `Member`) enforced via ASP.NET `[Authorize(Roles = "...")]`; a platform-level `SuperAdmin` role exists for the platform owner
- **Separation of concerns** — all security infrastructure lives in a dedicated `appfleet-nexus-security` class library, keeping the API project focused on business logic

### What's Explicitly Out of Scope (for now)

- **Invitation system** — shareable invite links for adding users to existing tenants. Deferred to post-MVP. See [AD-09](#ad-09-invitation-system--deferred-to-post-mvp).
- Multi-factor authentication (MFA) — Supabase supports it natively; can be toggled on without code changes
- OAuth / social logins — toggle-able in the Supabase dashboard; no code change needed
- Rate limiting — Render and Supabase provide infrastructure-level protection; app-level rate limiting deferred
- Driver authentication — Drivers are data entities, not auth users; can be promoted to auth users later if needed
- Full audit history — a separate audit trail table per entity (or a shared audit log) to record every change. The `is_deleted` flag and standard audit columns provide the MVP foundation; detailed change history is a future enhancement.

---

## 2. Architecture Decisions

### AD-01: Authentication Provider — Supabase Auth

| Aspect | Detail |
|--------|--------|
| **Decision** | Use Supabase Auth as the identity provider |
| **Alternatives considered** | Auth0 (free tier: 25K MAUs), Keycloak (self-hosted), ASP.NET Identity (self-managed) |
| **Reasoning** | Supabase is already in the stack for PostgreSQL hosting. Its Auth service is free (50K MAUs), issues standard JWTs, supports email/password + OAuth + MFA, and requires zero additional infrastructure. |
| **Is this best practice?** | ✅ **Yes** — delegating auth to a dedicated IdP is industry best practice. Supabase Auth is production-grade. |

---

### AD-02: Auth Flow — API-Centric (Blazor → .NET API → Supabase)

| Aspect | Detail |
|--------|--------|
| **Decision** | All auth calls (signup, login, refresh) go through the .NET API. Blazor never calls Supabase directly. |
| **Alternatives considered** | (A) Client-direct: Blazor calls Supabase Auth JS SDK directly. (B) Hybrid: Blazor does auth directly, API handles business logic. |
| **Reasoning** | API-centric gives us a single point of control for auth orchestration. Keeps the Supabase `service_role_key` server-side only. Simplifies the Blazor app (no Supabase SDK dependency). |
| **Is this best practice?** | ⚠️ **Partially** — The industry-standard SaaS pattern is often Hybrid (Option B), where the frontend handles auth directly with the IdP for faster UX and the API only validates JWTs. Our API-centric approach is simpler and more secure (keys never leave the server) but adds a network hop. |
| **Migration complexity** | 🟢 **Low** — To migrate to Hybrid, you would: (1) add `supabase-js` to the Blazor app, (2) call Supabase Auth directly from the browser for login/signup, (3) keep the API's JWT validation unchanged. No schema changes needed. |

---

### AD-03: Multi-Tenancy Strategy — Shared Database, Shared Schema, TenantId Column

| Aspect | Detail |
|--------|--------|
| **Decision** | Single PostgreSQL database, single schema, `tenant_id` discriminator column on all tenant-scoped tables |
| **Alternatives considered** | (A) Schema-per-tenant (separate Postgres schemas). (B) Database-per-tenant (separate Supabase projects). |
| **Reasoning** | Simplest approach, zero additional cost, excellent EF Core support via Global Query Filters. A single Supabase free-tier instance handles everything. Schema-per-tenant adds migration complexity; DB-per-tenant costs $25+/mo per tenant. |
| **Is this best practice?** | ✅ **Yes for early-stage SaaS** — Microsoft's own multi-tenancy guidance recommends this for < 1000 tenants. It's the most common pattern for new SaaS products. |
| **Migration complexity** | 🟡 **Medium** — If tenant isolation requirements grow (compliance, data residency), migrating to schema-per-tenant requires: (1) creating schemas dynamically, (2) routing EF Core to the correct schema, (3) running migrations per schema. No application code changes needed if the `ITenantContext` abstraction is preserved. |

---

### AD-04: Tenant Isolation Enforcement — EF Core Global Query Filters + PostgreSQL RLS

| Aspect | Detail |
|--------|--------|
| **Decision** | Defense-in-depth: application-level filtering via EF Core AND database-level filtering via PostgreSQL Row-Level Security |
| **Alternatives considered** | (A) Application-level only (EF Core filters). (B) Database-level only (RLS). |
| **Reasoning** | EF Core filters are the primary mechanism (easy to work with, unit-testable). Postgres RLS is the safety net — even if there's a code bug that bypasses EF Core filters, the database itself will not return cross-tenant data. |
| **Is this best practice?** | ✅ **Yes** — defense-in-depth is the gold standard for multi-tenant data isolation. |

---

### AD-05: JWT Validation — Stateless with Supabase Symmetric Key

| Aspect | Detail |
|--------|--------|
| **Decision** | API validates JWTs using Supabase's JWT secret (symmetric HS256). No session state. |
| **Alternatives considered** | (A) JWKS (asymmetric RS256) endpoint validation. (B) Server-side session validation (call Supabase on every request). |
| **Reasoning** | Supabase issues HS256 JWTs by default. Symmetric validation is simpler — no JWKS endpoint polling, no key rotation complexity. Stateless means the API can scale horizontally without shared session stores. |
| **Is this best practice?** | ⚠️ **Partially** — Industry best practice is asymmetric (RS256/ES256) with JWKS endpoint for key rotation. HS256 is secure but requires manually rotating the shared secret. |
| **Migration complexity** | 🟢 **Low** — Supabase supports custom JWT signing. To migrate: (1) configure Supabase to use RS256, (2) change `AddJwtBearer` to use `.SetJwksUri()` instead of `SymmetricSecurityKey`. ~10 lines of code change. |

---

### AD-06: User-Tenant-Role Storage — Own PostgreSQL Tables

| Aspect | Detail |
|--------|--------|
| **Decision** | Store tenant membership and roles in custom PostgreSQL tables (`tenants`, `users`, `tenant_users`). Supabase Auth stores only identity (email/password). |
| **Alternatives considered** | (A) Store everything in Supabase `app_metadata`. (B) Hybrid: SQL tables + sync roles into JWT via `app_metadata`. |
| **Reasoning** | Own tables are fully queryable (list all users in a tenant, aggregate role counts), not subject to metadata size limits, and make the domain model portable. If we migrate away from Supabase Auth in the future, the tenant/role data stays intact. |
| **Is this best practice?** | ✅ **Yes** — separating domain data from the auth provider's internal storage is best practice. |

---

### AD-07: Tenant Membership — Single Tenant Per User

| Aspect | Detail |
|--------|--------|
| **Decision** | Each user belongs to exactly one tenant |
| **Alternatives considered** | Multi-tenant membership (user belongs to N tenants with a tenant switcher in the UI) |
| **Reasoning** | Drastically simplifies the auth flow — no tenant selection step, no context switching, no ambiguity about "which tenant am I operating in right now?" The underlying schema (join table `tenant_users`) naturally supports multi-tenant membership if needed later. |
| **Is this best practice?** | ⚠️ **Partially** — Enterprise SaaS typically supports multi-tenant membership. For fleet management, single-tenant is the norm. |
| **Migration complexity** | 🟢 **Low** — The `tenant_users` join table already models a many-to-many relationship. To support multi-tenant: (1) remove the unique constraint on `user_id` in `tenant_users`, (2) add a tenant-selector UI, (3) pass the selected `tenant_id` in a request header or store in the JWT. ~1 day of work. |

---

### AD-08: User Profile Sync — Supabase DB Trigger

| Aspect | Detail |
|--------|--------|
| **Decision** | A PostgreSQL trigger on `auth.users` INSERT automatically creates a `public.contacts` row (for the user's name), a `public.users` row (linked to the contact), a `public.tenants` row, and a `public.tenant_users` row |
| **Alternatives considered** | (A) API webhook from Supabase. (B) Lazy provisioning on first API call. |
| **Reasoning** | DB triggers are atomic (same transaction as the auth signup), have zero latency, and require no external HTTP calls. They execute reliably even if the API is temporarily down. |
| **Is this best practice?** | ✅ **Yes** — Supabase's own documentation recommends this pattern. |

---

### AD-09: Invitation System — Deferred to Post-MVP

| Aspect | Detail |
|--------|--------|
| **Decision** | Invitation links are **deferred** to a future phase. Not part of the MVP. |
| **Original plan** | Admins generate unique invite URLs (3-day expiry). New users sign up via link and auto-join the inviting tenant. |
| **Why deferred** | Adds significant complexity to the signup flow (invite validation, tenant-linking logic in both the API and DB trigger). The MVP focuses on self-service signup where each user gets their own tenant. |
| **Implementation when ready** | (1) Add `invitations` table with `token`, `tenant_id`, `role`, `expires_at`. (2) Add invite CRUD endpoints (`POST /api/invitations`, `GET /api/invitations/{token}`). (3) Update the DB trigger to check for `invited_tenant_id` in user metadata. (4) Update `AuthController.SignUp` to accept an optional invite token. Estimated: ~4-6 hours. |
| **Migration complexity** | 🟢 **Low** — The schema and trigger are designed to accommodate invitations later without breaking changes. |

---

### AD-10: Roles — Admin + Member + SuperAdmin

| Aspect | Detail |
|--------|--------|
| **Decision** | Two tenant-level roles (`Admin`, `Member`) plus one platform-level role (`SuperAdmin`) |
| **Alternatives considered** | (A) Fine-grained permissions (e.g., `can_edit_vehicles`, `can_view_contacts`). (B) More roles (Owner, Admin, Member, Viewer). |
| **Reasoning** | Two roles cover the MVP use cases cleanly. `Admin` manages the tenant and its members. `Member` manages operational data (vehicles, contacts). `SuperAdmin` is the platform owner for cross-tenant support. |
| **Is this best practice?** | ⚠️ **Partially** — Enterprise SaaS typically uses fine-grained permissions (RBAC or ABAC). Simple roles are appropriate for MVP but limit flexibility. |
| **Migration complexity** | 🟢 **Low** — To add fine-grained permissions: (1) create a `permissions` table and a `role_permissions` join table, (2) replace `[Authorize(Roles = "Admin")]` with a custom policy-based authorization handler that checks permissions. The `ITenantContext` abstraction already carries the role; extending it to carry permissions is straightforward. |

---

### AD-11: Security Library — Dedicated `appfleet-nexus-security` Project

| Aspect | Detail |
|--------|--------|
| **Decision** | All security infrastructure (tenant context, middleware, interceptors, auth service, DTOs) lives in a separate class library referenced by the API project |
| **Reasoning** | Clean separation of concerns. The API project focuses on controllers and business logic. Security can be tested independently. If we add a second API (e.g., a background worker), it can reference the same security library. |
| **Is this best practice?** | ✅ **Yes** — separating cross-cutting concerns into dedicated libraries is standard .NET architecture. |

---

### AD-12: FMCSA Census Data — Kept but Isolated

| Aspect | Detail |
|--------|--------|
| **Decision** | The `fmcsa_census` table and its `Carrier` entity / `CarriersController` remain in the codebase but are excluded from the security/multi-tenancy model |
| **Reasoning** | This data was created for learning and experimentation. It has no `tenant_id`, no audit columns, and no RLS policy. It will not be part of any authenticated flow. Keeping it preserves the learning reference without interfering with the security model. |
| **Impact** | The `CarriersController` endpoints will be explicitly marked `[AllowAnonymous]` or moved behind a `SuperAdmin`-only policy as appropriate. No global query filters apply to `Carrier` since it doesn't inherit from `BaseEntity`. |

---

### AD-13: User Profile — Names on Users Table, Contacts Are Separate

| Aspect | Detail |
|--------|--------|
| **Decision** | `first_name` and `last_name` live directly on the `users` table. `contacts` is a separate business entity (customers, vendors, partners) with no FK relationship to `users`. |
| **Alternatives considered** | (A) User-is-a-Contact: `users` holds a `contact_id` FK to `contacts`, name lives only on the contact. (B) Store a single `display_name` on `users`. |
| **Why not user-is-a-contact** | Creates circular dependency in the DB trigger (contact needs `created_by` = user ID, but user needs `contact_id` = contact ID). Forces a JOIN on every auth query. RLS on contacts conflicts with unauthenticated user resolution. Soft-deleting a contact would break the user's profile. Conflates two semantically different domain concepts. |
| **Is this best practice?** | ✅ **Yes** — most SaaS platforms (Stripe, HubSpot, Salesforce) separate user accounts from business contacts. Simple, no circular dependencies, no JOIN overhead. |
| **Migration complexity** | 🟢 **Low** — If needed later, add an optional `contact_id` FK to `users` to link a user to their business contact record. No schema breaking changes. |

---

### AD-14: Soft Delete Strategy — `is_deleted` Flag + Future Audit History

| Aspect | Detail |
|--------|--------|
| **Decision** | Soft delete uses a single `is_deleted` boolean flag. No `deleted_at` or `deleted_by` columns. A full **audit history table** will be added in a future phase to capture all entity changes (including who deleted what and when). |
| **Alternatives considered** | (A) Include `deleted_at` + `deleted_by` columns alongside `is_deleted`. (B) No soft delete (hard delete only). |
| **Reasoning** | `deleted_at` and `deleted_by` are redundant when a proper audit history exists. The audit history table will record every state change (create, update, delete) with timestamps, user IDs, and old/new values — a superset of what `deleted_at`/`deleted_by` provide. Keeping the entity model lean avoids premature fields that will be superseded. |
| **Is this best practice?** | ✅ **Yes** — audit history tables (temporal tables or event-sourced logs) are the industry standard for compliance-grade change tracking. The `is_deleted` flag is the minimum viable soft-delete mechanism. |
| **Future audit history design** | A shared `audit_log` table or per-entity history tables (e.g., `contacts_history`) that capture: `entity_type`, `entity_id`, `action` (Create/Update/Delete), `changed_by`, `changed_at`, `old_values` (JSONB), `new_values` (JSONB). Can be implemented via EF Core interceptors or PostgreSQL triggers. |

---

## 3. Cost Summary

| Component | Cost | Notes |
|-----------|------|-------|
| Supabase Auth | Free | 50K MAUs |
| Supabase PostgreSQL | Free | 500MB, existing |
| Supabase DB Triggers + RLS | Free | Built into PostgreSQL |
| ASP.NET Core JWT Middleware | Free | Built into framework |
| EF Core Global Query Filters | Free | Built into EF Core |
| Render Hosting | Existing | No additional services |
| **Total additional cost** | **$0** | |

---

## 4. Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Supabase free tier limits hit (50K MAUs) | Low (early stage) | Medium | Monitor usage; paid tier is $25/mo if needed |
| HS256 JWT secret leaked | Low | Critical | Store only in Render env vars; rotate in Supabase dashboard if compromised |
| EF Core filter bypassed by raw SQL | Low | Critical | Postgres RLS is the safety net (defense-in-depth) |
| Single-tenant-per-user limits future customers | Low | Low | `tenant_users` join table already supports multi; ~1 day migration |
| No invite system in MVP limits team onboarding | Medium | Low | Each user creates own tenant for now; invite system is ready to implement post-MVP (~4-6 hours) |
