# ADR-007: Single Tenant Membership Model for MVP

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
During initial product launch, managing multi-tenant context switching, complex invitation routing, and ambiguous session states introduces significant frontend and backend complexity.

---

## Decision
Enforce a **single tenant membership per user** for the MVP release via a unique database index on `public.tenant_users (user_id)`.

---

## Alternatives Considered
* **Multi-Tenant Membership**: Allowing users to belong to $N$ tenants simultaneously, requiring tenant-selector dialogs and active tenant switching headers.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Drastically simplifies the authentication handshake, routing, and query contexts with zero tenant ambiguity.
  * Appropriate for the primary commercial fleet management target market.
* **Risks & Trade-offs**: Users cannot belong to multiple client companies under a single email address during the MVP phase.
* **Migration Paths**: Low complexity (~1 day). The underlying `tenant_users` join table already models a many-to-many relationship; enabling multi-tenancy simply requires dropping the unique index on `user_id` and adding UI tenant selection.
