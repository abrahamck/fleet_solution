# ADR-013: User Profile vs. Business Contact Separation

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
A model was required for storing the personal profile information of authenticated users (names, emails) alongside business contacts (customers, vendors, external partners) managed within a tenant directory.

---

## Decision
Store `first_name` and `last_name` directly on the `public.users` table. Keep `public.contacts` as an independent business entity with no mandatory foreign key relationship to `public.users`.

---

## Alternatives Considered
* **User-is-a-Contact**: Linking `users` to `contacts` via a `contact_id` foreign key, storing names only in the contacts table.
* **Single Display Name**: Storing a single unseparated `display_name` string on `users`.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Eliminates circular dependencies during automated trigger registration (`handle_new_user`).
  * Avoids unnecessary table JOINs on every authentication and user identity query.
  * Prevents contact soft-deletions from corrupting authenticated user account profiles.
* **Risks & Trade-offs**: None identified.
* **Migration Paths**: Low complexity. An optional `contact_id` nullable FK can be added to `users` in the future if linking internal users to directory entries is needed.
