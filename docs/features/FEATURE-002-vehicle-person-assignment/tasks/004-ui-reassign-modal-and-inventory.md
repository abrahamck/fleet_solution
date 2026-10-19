# TASK-004: Blazor UI Quick Reassign Modal & Inventory Wiring

> **Feature**: `FEATURE-002` ([requirements.md](../requirements.md))  
> **Status**: `VERIFIED`  
> **Order**: 004  
> **Dependencies**: TASK-003  

---

## 1. Objective & Boundaries
Create the `ReassignModal.razor` Blazor component providing a dedicated quick reassignment interface and wire up a "Reassign" action button on vehicle cards and table rows in `Inventory.razor`.

*Boundaries*: Do NOT touch inline child dialog nesting in this task (reserved for TASK-005).

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-005`, `REQ-006`, `REQ-007`
- **Target Acceptance Criteria**: `AC-005`, `AC-006`, `AC-007`, `TEST-005`
- **Design References**: [`design.md §3.1.3 & §3.2 Flow 1`](../design.md#313-ui-layer-appfleet-nexus-ui), [`ADR-020`](../../../docs/ADR/ADR-020-unified-inventory-workspace.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Models/VehicleModels.cs` | Modify | Add `ReassignVehicleModel` with `NewContactId`, `AssociationRole`, and `ReassignMode` properties. |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/ReassignModal.razor` | Create | New modal dialog displaying current vehicle assignment, contact selector with license compliance badges, mode selector (`Replace Primary` vs. `Add Co-Driver`), and API submission to `POST /api/vehicles/{id}/reassign`. |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Pages/Inventory.razor` | Modify | Add "Reassign" button to vehicle grid cards and table rows; bind click event to open `ReassignModal`; handle modal `OnSaved` callback to refresh vehicle list. |

---

## 4. Required Tests & Evidence Proofs
- `dotnet build app-fleet-nexus-net/ui/appfleet-nexus-ui/appfleet-nexus-ui.csproj` compiles with 0 errors.
- Visual inspection / DOM check: "Reassign" button appears in Inventory vehicle list and opens modal with current driver details (`TEST-005`).

---

## 5. Constraints & Non-Negotiables
- Modal must clearly display whether the user is replacing the primary driver or adding a co-driver.
- Contact dropdown must show compliance indicators (e.g. ⚠️ Expired CDL) without blocking the dispatch action (`INV-009`).

---

## 6. Definition of Done (DoD) Checklist
- [x] `ReassignVehicleModel` defined.
- [x] `ReassignModal.razor` implemented and styled in accordance with existing modal design tokens.
- [x] "Reassign" button added to vehicle card and table views in `Inventory.razor`.
- [x] Successful reassignment closes modal and refreshes inventory data.
- [x] UI project builds with 0 errors.
- [x] Task status updated to `VERIFIED`.
