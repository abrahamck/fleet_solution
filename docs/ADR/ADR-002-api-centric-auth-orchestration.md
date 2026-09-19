# ADR-002: Auth Flow Orchestration — API-Centric

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
The frontend client (Blazor WASM) needs to orchestrate user registration, sign-in, and session refreshing securely without exposing administrative API keys or coupling the client bundle to external identity SDKs.

---

## Decision
Adopt an **API-Centric authentication flow** (`Blazor WASM → .NET Web API → Supabase Auth`). The Blazor client never calls Supabase directly; all authentication requests route through the backend API.

---

## Alternatives Considered
* **Client-Direct**: Blazor WASM client communicates directly with the Supabase Auth JS SDK / REST endpoint.
* **Hybrid**: Blazor client authenticates directly with Supabase, while the .NET API only validates JWT bearer tokens for business endpoints.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Single point of control for registration, validation, and domain bootstrapping.
  * Supabase `service_role_key` remains strictly server-side.
  * Simplifies Blazor WASM dependencies by eliminating external JavaScript Auth SDKs.
* **Risks & Negative Impacts**:
  * Introduces an additional network hop during signup and login requests compared to direct browser-to-IdP communication.
* **Migration Paths**: Low complexity. To migrate to a hybrid model, client-side auth can be adopted while leaving backend JWT bearer validation unchanged.
