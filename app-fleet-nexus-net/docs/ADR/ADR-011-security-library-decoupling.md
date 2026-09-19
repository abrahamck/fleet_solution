# ADR-011: Security Library Decoupling (`appfleet-nexus-security`)

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
Security, token extraction, tenant context resolution, and database RLS connection interception are cross-cutting concerns that risk cluttering Web API controllers and business logic if directly intermingled.

---

## Decision
Isolate all security infrastructure, authentication services, tenant middleware, database interceptors, and DTOs into a dedicated class library: **`appfleet-nexus-security`**.

---

## Alternatives Considered
* **Not Available** (Implementing security directly inside the primary `appfleet-nexus-api` project).

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Clear architectural separation of concerns.
  * Security logic is independently unit-testable and reusable by background worker services.
  * API controllers remain focused strictly on application endpoints and business workflows.
* **Risks & Trade-offs**: Requires managing project dependencies and service registration extension methods across assemblies.
* **Migration Paths**: Not Available.
