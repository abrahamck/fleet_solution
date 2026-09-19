# ADR-020: Unified Inventory Workspace with Tabbed Sub-Modules

* **Status**: Accepted
* **Date**: 2026-06-22

---

## Context & Problem Statement
Fleet managers manage multiple interconnected asset directories (vehicles, equipment, drivers, customers, vendors). Fragmenting these directories into disconnected top-level routes creates navigation clutter in the sidebar.

---

## Decision
Create a unified **Inventory Management Workspace** at `/inventory` with integrated tabbed navigation:
* **Vehicles Tab** (Active): High-density data grid and mobile card views for fleet vehicle management.
* **Contacts Tab** (Extensible placeholder): Contact directory for customers, vendors, and driver profiles.

---

## Alternatives Considered
* **Separate Disconnected Routes**: Dedicated top-level navigation items for `/vehicles` and `/contacts`.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Clean, cohesive navigation sidebar.
  * Shared UI paradigms (search bars, filter chips, action headers) across asset types.
  * Extensible design allowing future directory categories to be introduced without menu reorganizations.
* **Risks & Trade-offs**: Requires client-side tab state management in `Inventory.razor`.
* **Migration Paths**: Not Available.
