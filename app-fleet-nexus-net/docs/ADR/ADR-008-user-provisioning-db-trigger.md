# ADR-008: User & Organization Auto-Provisioning via DB Trigger

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
When a user registers via Supabase Auth, corresponding records must be created across application domain tables (`public.users`, `public.tenants`, `public.tenant_users`) in an atomic, fault-tolerant manner.

---

## Decision
Implement a PostgreSQL trigger (`on_auth_user_created`) that fires the `handle_new_user()` stored procedure on every `INSERT` into `auth.users`. The procedure atomically creates:
1. A mirrored record in `public.users`.
2. A new organization in `public.tenants` (e.g. `"[Name]'s Organization"`).
3. An administrative membership in `public.tenant_users` with role `'Admin'`.

---

## Alternatives Considered
* **API Webhook**: Supabase firing an HTTP webhook back to the .NET API.
* **Lazy Provisioning**: Creating application domain rows upon the user's first authenticated API call.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Atomically executed inside the registration database transaction.
  * Zero HTTP network latency and no external webhook endpoints required.
  * Ensures domain consistency even if the application API is temporarily restarting during signup.
* **Risks & Trade-offs**: Custom schema logic resides in PostgreSQL stored functions.
* **Migration Paths**: Extensible to support invitation acceptance by inspecting metadata properties in future releases.
