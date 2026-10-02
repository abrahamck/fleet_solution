# TASK-005: Blazor UI Inline Entity Creation & Recursion Guards

> **Feature**: `FEATURE-002` ([requirements.md](../requirements.md))  
> **Status**: `DEFERRED` (Deferred to Phase 2 UI Release)  
> **Order**: 005  
> **Dependencies**: TASK-004  

---

## 1. Objective & Boundaries
Enhance both `VehicleDialog.razor` and `ContactDialog.razor` to support seamless inline entity creation (creating a new contact while drafting a vehicle, or creating a new vehicle while drafting a contact) while enforcing recursion prevention (`IsInlineMode`) and preserving parent draft state.

*Boundaries*: Do NOT alter backend API logic in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-001`, `REQ-002`, `REQ-003`, `REQ-004`, `INV-006`
- **Target Acceptance Criteria**: `AC-002`, `AC-004`, `AC-013`, `TEST-002`, `TEST-004`, `TEST-013`
- **Design References**: [`design.md §3.1.3 & §3.2 Flow 2`](../design.md#313-ui-layer-appfleet-nexus-ui), [`ADR-018`](../../../docs/ADR/ADR-018-multi-step-input-wizard-ux-pattern.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/VehicleDialog.razor` | Modify | 1. Add `[Parameter] public bool IsInlineMode { get; set; } = false;`<br/>2. Add "+ Add New Driver / Contact" button (hidden when `IsInlineMode == true`).<br/>3. Embed `ContactDialog` with inline mode.<br/>4. Retain vehicle form fields when switching to child wizard; on child cancel, restore draft intact (`AC-013`); on child save, append created contact and auto-select as primary. |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/ContactDialog.razor` | Modify | 1. Add `[Parameter] public bool IsInlineMode { get; set; } = false;`<br/>2. On Step 5 (Assignments), add "+ Add New Vehicle" button (hidden when `IsInlineMode == true`).<br/>3. Embed `VehicleDialog` with inline mode.<br/>4. Preserve 5-step wizard state; on child cancel, restore draft intact (`AC-013`); on child save, append created vehicle to assignment list. |

---

## 4. Required Tests & Evidence Proofs
- `dotnet build app-fleet-nexus-net/ui/appfleet-nexus-ui/appfleet-nexus-ui.csproj` succeeds with 0 errors.
- `TEST-002`: VehicleDialog launches ContactDialog inline; nested child creation button is hidden.
- `TEST-004`: ContactDialog launches VehicleDialog inline; nested child creation button is hidden.
- `TEST-013`: Cancelling child dialog preserves parent input draft fields completely.

---

## 5. Constraints & Non-Negotiables
- `INV-006`: Recursive modal nesting is strictly forbidden. When `IsInlineMode == true`, all child entity creation triggers must be suppressed.
- No modal-in-modal z-index stacking: replacing modal body view with child wizard provides cleaner UX without backdrop traps.

---

## 6. Definition of Done (DoD) Checklist
- [ ] `IsInlineMode` parameter added and respected in both dialog components.
- [ ] "+ Add New Driver / Contact" button functional in `VehicleDialog.razor`.
- [ ] "+ Add New Vehicle" button functional in Step 5 of `ContactDialog.razor`.
- [ ] Parent draft state preserved across cancellations and completions.
- [ ] UI project builds cleanly without warnings or errors.
- [ ] Task status updated to `VERIFIED`.
