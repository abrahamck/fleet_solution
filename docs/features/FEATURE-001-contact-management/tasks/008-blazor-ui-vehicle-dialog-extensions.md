# TASK-008: Blazor UI VehicleDialog Extensions

> **Feature**: `FEATURE-001` ([requirements.md](../requirements.md))  
> **Status**: `READY`  
> **Order**: 008  
> **Dependencies**: `TASK-005`, `TASK-007`  

---

## 1. Objective & Boundaries

Extend `VehicleDialog.razor` in `appfleet-nexus-ui` to support contact assignments and garage address input:
- Add a dedicated **Contact Assignment** step / section in the vehicle creation wizard.
- Provide a contact picker from active contacts, allowing assignment of Driver vs. Responsible Contact roles and designating a primary contact.
- Enforce the UI invariant that at least 1 contact must be assigned before saving the vehicle (`AC-011`).
- Add an optional **Garage Address** section under vehicle contact points (`AC-008`).
- Update vehicle list views in `Inventory.razor` to display assigned drivers and contact count badges.

*Boundary*: Do NOT modify database or API controller files in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-008`, `REQ-010`, `REQ-011`
- **Target Acceptance Criteria**: `AC-008`, `AC-010`, `AC-011`
- **Design References**: [`design.md`](../design.md) (Section 4.3 — VehicleDialog Extensions)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/VehicleDialog.razor` | Modify | Add contact assignment step/repeater and optional garage address inputs. |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Models/VehicleModels.cs` | Modify / Create | Add models for assigned contact selection and garage address. |
| `app-fleet-nexus-net/ui/appfleet-nexus-ui/Pages/Inventory.razor` | Modify | Display assigned driver avatars/names in the vehicles table. |

---

## 4. Required Tests & Evidence Proofs
- `dotnet build app-fleet-nexus-net/ui/appfleet-nexus-ui/appfleet-nexus-ui.csproj` compiles with 0 errors.

---

## 5. Constraints & Non-Negotiables
- Vehicle save button must be disabled or show validation error if 0 contacts are assigned.

---

## 6. Definition of Done (DoD) Checklist
- [ ] `VehicleDialog.razor` allows assigning contacts with roles and entering garage address.
- [ ] Invariant I-002 enforced in the UI.
- [ ] Vehicles tab displays assigned contact details.
- [ ] Task status updated to `VERIFIED`.
