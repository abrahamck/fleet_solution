# ADR-005: Stateless JWT Token Validation (HS256)

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
The backend API requires an authentication verification mechanism that validates incoming requests with low latency and without maintaining sticky server-side sessions or calling an identity provider on every HTTP request.

---

## Decision
Implement **stateless JWT validation** in the .NET Web API using Supabase's symmetric JWT secret key with HMAC-SHA256 (`HS256`).

---

## Alternatives Considered
* **JWKS Asymmetric Validation (RS256/ES256)**: Public/private key validation with JWKS endpoint key discovery.
* **Server-Side Session Validation**: Calling the identity provider API on every incoming request to check session validity.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * High-throughput local token verification without external network latency.
  * Stateless architecture allows horizontal API scaling without shared session stores.
* **Risks & Trade-offs**:
  * The shared secret must remain secure on the server; key rotation requires updating server environment variables.
* **Migration Paths**: Low complexity. Migrating to asymmetric RS256 requires switching ASP.NET `AddJwtBearer` to use `.SetJwksUri()` (~10 lines of code change).
