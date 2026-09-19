# TASK-007: Blazor UI Contacts Tab & ContactDialog Wizard

> **Feature**: `FEATURE-001` ([requirements.md](../requirements.md))  
> **Status**: `READY`  
> **Order**: 007  
> **Dependencies**: `TASK-004`  

---

## 1. Objective & Boundaries

Activate the Contacts management interface within the unified inventory workspace (`appfleet-nexus-ui`):
- Update `Inventory.razor` to replace the "Coming Soon" placeholder in the Contacts tab with:
  - Header with summary KPI tiles (Total Contacts, Active Drivers, Expiring Licenses).
  - Search input and filter pills (All, Drivers, Fleet Managers, Inactive).
  - "Add Contact" button triggering `ContactDialog.razor`.
  - Data table with avatar/initials, full name, unique ID, contact type badge, primary phone, primary email, primary address, assigned vehicles badge, status pill, and action menu (Edit, Delete).
  - Responsive mobile card view for small screens.
- Create `ContactDialog.razor` following the 5-step wizard pattern ([ADR-018](../../../docs/ADR/ADR-018-multi-step-input-wizard-ux-pattern.md)):
  - Step 1: **Identity** (Unique ID, First Name, Middle Name, Last Name, Type, Job Title, Status).
  - Step 2: **Contact Points** (Dynamic lists for Phones with labels & primary toggle [≥1 required], Emails, Addresses with Address 1-4 [≥1 required]).
  - Step 3: **Driver Compliance** (CDL Number, State, Expiration Date, DOT Medical Expiration).
  - Step 4: **Emergency & Notes** (Emergency Contact Name, Emergency Phone, Notes).
  - Step 5: **Vehicle Assignments** (Optional initial vehicle assignment multi-select with role).
- Define client-side models in `Models/ContactModels.cs`.
- Update `Inventory.razor.css` for contact layout styling.

*Boundary*: Do NOT modify `VehicleDialog.razor` in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-001`, `REQ-002`, `REQ-003`, `REQ-004`, `REQ-005`, `REQ-006`, `REQ-007`, `REQ-016`
- **Target Acceptance Criteria**: `AC-016`
- **Design References**: [`design.md`](../design.md) (Section 4 — Frontend UI & User Experience), [`ADR-018`](../../../docs/ADR/ADR-018-multi-step-input-wizard-ux-pattern.md), [`ADR-020`](../../../docs/ADR/ADR-020-unified-inventory-workspace.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Pages/Inventory.razor` | Modify | Activate Contacts tab with KPI cards, search/filter bar, data grid, and mobile cards. |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Pages/Inventory.razor.css` | Modify | CSS classes for contact grid, badges, pills, and dynamic contact point cards. |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/ContactDialog.razor` | Create | 5-step wizard dialog with dynamic phone/email/address repeaters and step-by-step validation. |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Models/ContactModels.cs` | Create | Client-side DTO models, view models, and wizard state container. |

---

## 4. Required Tests & Evidence Proofs
- `dotnet build app-fleet-nexus-net/ui/appfleet-nexus-ui/appfleet-nexus-ui.csproj` compiles with 0 errors.

---

## 5. Constraints & Non-Negotiables
- Wizard must prevent progressing past Step 2 if fewer than 1 phone or fewer than 1 address is added.
- Modern visual styling adhering to dark/light design system with smooth transitions.

---

## 6. Definition of Done (DoD) Checklist
- [ ] Contacts tab fully functional with search, filters, and CRUD triggers.
- [ ] `ContactDialog.razor` provides smooth 5-step wizard flow.
- [ ] UI project builds with 0 errors.
- [ ] Task status updated to `VERIFIED`.
