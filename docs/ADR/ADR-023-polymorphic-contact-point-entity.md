# ADR-023: Separate Typed Contact Point Tables (Phones, Emails, Addresses)

* **Status**: Accepted
* **Date**: 2026-09-19

---

## Context & Problem Statement

The Contact Management feature (FEATURE-001) requires storing communication and location data for `Contact` and `Vehicle` records. Both entity types can have multiple phone numbers, email addresses, and physical addresses — each carrying a label (e.g., `"Mobile"`, `"Garage"`, `"Billing"`) and a primary flag.

An initial approach considered a single polymorphic `contact_points` table with a `contact_point_type` discriminator column and all possible fields (phone, email, address columns) in a single flat row. This was rejected because it creates wide, sparse rows — a `Phone` record carries NULL values for all 10 address columns and vice versa. This is a well-known relational anti-pattern that degrades clarity, indexing efficiency, and schema integrity.

Contact point data (phone, email, address with multiple labeled values) is a standard problem domain. The internet standard **vCard / RFC 6350** defines multi-value typed fields (`TEL`, `EMAIL`, `ADR`) as first-class repeatable elements. Enterprise CRM and ERP systems (Salesforce, SAP Business Address Services, Google Workspace People API) uniformly implement these as **separate typed child tables** rather than a single sparse discriminated table.

---

## Decision

Implement **three separate, lean, typed tables** for contact point storage:

| Table | Entity | Columns |
| :--- | :--- | :--- |
| `public.contact_phones` | `ContactPhone` | `owner_type`, `owner_id`, `label`, `is_primary`, `phone_number` + audit |
| `public.contact_emails` | `ContactEmail` | `owner_type`, `owner_id`, `label`, `is_primary`, `email_address` + audit |
| `public.contact_addresses` | `ContactAddress` | `owner_type`, `owner_id`, `label`, `is_primary`, address fields + audit |

Each table:

1. **Is polymorphic via `owner_type` + `owner_id`**: `owner_type` stores `Contact` or `Vehicle`; `owner_id` stores the UUID of the owning record. This allows any future entity type to gain contact point data without a schema change.
2. **Contains only the columns relevant to its type**: Zero NULL columns from type discrimination. Every column in every row is meaningful.
3. **Carries shared metadata**: `label` (e.g., `"Mobile"`, `"Garage"`) and `is_primary` (at most one primary per owner + table combination, enforced at the application layer).
4. **Extends `BaseEntity`**: Inherits `TenantId`, `IsDeleted`, and full audit columns (ADR-014).
5. **Is protected by RLS**: PostgreSQL row-level security policies applied per table (ADR-004).

---

## Alternatives Considered

### A. Single Flat Discriminated Table (Rejected — Original Design)
One `contact_points` table with a `contact_point_type` discriminator column and all possible columns (phone, email, and all address fields) in a single schema.
- **Rejected**: Creates wide, sparse rows. A Phone row has 10 NULL address columns. Address and email columns can never have DB-level NOT NULL constraints. Queries filtering by `phone_number` must also carry the overhead of unused columns. This is the classic anti-pattern of "God Table with discriminator".

### B. Table-Per-Type with Shared Base (TPT / Base + Extension)
One base `contact_points` table for shared metadata (owner, label, is_primary) + thin extension tables per type joined by PK.
- **Rejected**: Every fetch requires a JOIN to retrieve the actual value. EF Core TPT generates a JOIN on every query even when only one type is accessed. Adds complexity without meaningful benefit over Option 1 (separate typed tables).

### C. JSONB `value` Column
One base table with a JSONB `value` blob per row containing the type-specific data.
- **Rejected**: 
  - No column-level type safety or DB constraints (cannot enforce `NOT NULL` or format validation inside JSONB).
  - Queries filtering by internal JSONB fields (e.g., find contacts in Texas) are verbose and unindexed by default.
  - EF Core JSONB owned-type mapping adds framework-specific complexity.
  - Breaks standard reporting tool compatibility.
  - Violates ADR-006 (Relational Domain Identity Storage).
  - Contact point fields are well-defined at design time; JSONB flexibility provides no benefit.

---

## Consequences & Trade-offs

* **Positive Impacts**:
  * Zero sparse rows — every column in every row is always meaningful.
  * Column-level DB constraints can be applied per type (e.g., `phone_number NOT NULL` on contact_phones).
  * Simple, direct LINQ queries with no JOIN overhead per type.
  * Independent indexes per table — e.g., index on `phone_number` for search, index on `(owner_type, owner_id)` for FK-style lookups.
  * Aligns with industry standards (Salesforce, SAP, vCard RFC 6350).
  * Future contact point types (e.g., `Fax`, `Website`) are new lean tables, not schema modifications to existing tables.
* **Risks & Trade-offs**:
  * Three DbSets instead of one. Slightly more code surface area.
  * Polymorphic `owner_type` + `owner_id` still lacks DB-level FK referential integrity (same trade-off as the rejected alternatives). Integrity enforced at the application layer.
  * To load a Contact's "full contact point picture," three separate queries or `Include` paths are needed (Phones, Emails, Addresses).
* **Migration Paths**:
  * If a fourth contact point type is needed (e.g., `contact_social_media`), a new lean table is added with no impact on existing tables.
  * If polymorphic integrity becomes a requirement, the `owner_type` + `owner_id` columns can be replaced with typed FK columns per owning entity on each table independently.
