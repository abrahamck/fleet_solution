# Deferred Items Registry

> **Purpose**: Formal inventory of capabilities, enhancements, and scope items that have been deliberately deferred from their original feature release to a future iteration. This registry ensures nothing is lost and provides traceability back to the originating feature and decision rationale.

---

## How to Use This Registry

1. When a capability is deferred during any lifecycle phase (Discovery, Architecture, Planning, or Implementation), add an entry here.
2. Each entry must reference the originating feature, the specific requirement/AC being deferred, and the rationale.
3. When a deferred item is picked up for implementation, update the **Status** to `SCHEDULED` with the target feature ID and remove it once delivered.

---

## Deferred Items

| ID | Title | Originating Feature | Deferred Requirement(s) | Rationale | Date Deferred | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `DEFER-001` | Multi-Vehicle Batch Assignment to a Driver/Contact | [FEATURE-002](features/FEATURE-002-vehicle-person-assignment/requirements.md) | UC-6, AC-011 (multi-assign subset) | Reduces initial release scope. Single-vehicle assignment via Reassign Modal and inline creation cover the core operational needs. Batch/multi-select assignment is an efficiency enhancement that can be layered on once the foundational assignment APIs and UI components are stable. | 2026-10-01 | `DEFERRED` |

---

## Status Legend

| Status | Meaning |
| :--- | :--- |
| `DEFERRED` | Acknowledged and parked — not scheduled for any release. |
| `SCHEDULED` | Picked up and assigned to a future feature/sprint. |
| `DELIVERED` | Implemented and merged — can be removed from this registry. |
