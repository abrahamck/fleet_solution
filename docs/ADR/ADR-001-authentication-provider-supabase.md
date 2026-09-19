# ADR-001: Authentication Provider — Supabase Auth

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
FleetNexus requires a reliable, production-grade identity provider to manage user credentials, issue secure authentication tokens, and support password hashing without incurring operational overhead or self-managing an authentication database server.

---

## Decision
Use **Supabase Auth** as the external identity provider for FleetNexus.

---

## Alternatives Considered
* **Auth0**: Free tier capped at 25,000 Monthly Active Users (MAUs).
* **Keycloak**: Self-hosted, requiring dedicated compute infrastructure and continuous operational maintenance.
* **ASP.NET Core Identity**: Self-managed user tables and cryptography requiring custom schema maintenance.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Free tier includes up to 50,000 MAUs.
  * Standardized JWT issuance and built-in support for email/password, OAuth, and MFA.
  * Leverages existing Supabase PostgreSQL infrastructure already utilized for hosting.
* **Risks**: Vendor lock-in to Supabase Auth API format.
* **Migration Paths**: Domain and tenant data remain in standard relational tables (see [ADR-006](ADR-006-relational-domain-identity-storage.md)), making migration to any OIDC provider straightforward if needed.
