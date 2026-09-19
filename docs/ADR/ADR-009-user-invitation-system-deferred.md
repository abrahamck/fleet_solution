# ADR-009: User Invitation System Architecture

* **Status**: Deferred (Post-MVP)
* **Date**: 2026-06-21

---

## Context & Problem Statement
Organizations require the ability to invite secondary team members into an existing tenant organization via unique tokenized invite links.

---

## Decision
**Defer the invitation system to post-MVP**. The initial release focuses strictly on self-service registration where each registrant automatically provisions their own tenant organization.

---

## Alternatives Considered
* **Full MVP Invitation System**: Generating unique invitation URLs with 3-day expiry tokens, token redemption endpoints, and DB trigger branching logic.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Significantly reduces signup complexity and shortens time-to-market.
* **Risks & Trade-offs**: Multiple users cannot collaborate in the same tenant account during the initial MVP.
* **Migration Paths**: Low complexity (~4-6 hours). Implementing post-MVP involves adding an `invitations` table, invite endpoints, and updating `handle_new_user()` to link existing tenants when an invite token is present.
