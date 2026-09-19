# ADR-010: Role-Based Access Control (Admin, Member, SuperAdmin)

* **Status**: Accepted
* **Date**: 2026-06-21

---

## Context & Problem Statement
The platform requires an authorization model to distinguish between tenant administrative tasks, regular operations, and platform-level support/maintenance operations.

---

## Decision
Adopt a **three-tier role-based access model**:
* **Admin**: Tenant-level manager who administers tenant users and operational data.
* **Member**: Tenant-level operational user who manages day-to-day fleet assets (vehicles, contacts).
* **SuperAdmin**: Platform-level owner role possessing cross-tenant bypass capabilities for administrative support.

---

## Alternatives Considered
* **Fine-Grained Permissions (RBAC/ABAC)**: Matrix of explicit individual capabilities (e.g. `can_edit_vehicles`, `can_view_contacts`).
* **Multi-Role Hierarchy**: Expanding roles to include Owner, Admin, Member, and Viewer.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Clean, understandable model covering all core MVP fleet workflows without premature complexity.
* **Risks & Trade-offs**: Coarse roles lack granular field- or feature-level permission gating.
* **Migration Paths**: Low complexity. The `ITenantContext` abstraction can be extended to carry specific permissions if transitioning to fine-grained RBAC/ABAC in the future.
