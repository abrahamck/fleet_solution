# ADR-006: Relational Domain Identity & Role Storage

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
The application needs to store and query user profile metadata, organizational memberships, and role assignments without being constrained by identity provider metadata size limits or coupling business queries to external auth schemas.

---

## Decision
Store all domain membership and authorization data in dedicated PostgreSQL relational tables (`public.tenants`, `public.users`, `public.tenant_users`). Supabase Auth is restricted solely to storing credential identity in `auth.users`.

---

## Alternatives Considered
* **Store in Supabase `app_metadata`**: Placing tenant IDs and roles entirely inside the JWT / auth provider metadata payload.
* **Hybrid Storage**: Relational tables with real-time metadata syncing into the token.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Rich SQL queryability (e.g. listing tenant members, counting roles, managing permissions).
  * No metadata payload limits.
  * Domain data portability if the identity provider is changed in the future.
* **Risks & Trade-offs**: Requires synchronizing the primary key `id` between `auth.users` and `public.users`.
* **Migration Paths**: Not Available.
