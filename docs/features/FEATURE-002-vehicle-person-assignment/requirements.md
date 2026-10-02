# FEATURE-002 — Requirements: Vehicle and Person Assignment / Reassignment

> **Status**: `APPROVED`  
> **Complexity**: `STANDARD`  
> **Created**: 2026-10-01  
> **Author**: AI Discovery Agent  
> **Gate 1 Approval**: [x] Approved by Human (Date: 2026-10-01)

---

## 1. Executive Overview

### 1.1 Business Problem
Fleet operators require flexible, bi-directional workflows to manage assignments between Vehicles and Persons (Drivers / Contacts). This feature enables dispatchers and fleet managers to:
1. **Add Vehicle**: Assign an existing contact/driver OR create a new driver/contact via the full wizard without losing vehicle draft progress.
2. **Add Contact**: Assign existing vehicle(s) OR create a new vehicle without losing contact draft progress.
3. **Quick Reassign Vehicle**: Reassign a vehicle directly from inventory rows/cards with a dedicated Reassign modal offering explicit **Replace Primary** vs. **Add Co-Driver / Secondary** options.
4. **Driver Multi-Vehicle Assignment**: Assign one or multiple vehicles to a driver or fleet manager from both the Contact Directory and Inventory.

### 1.2 User Personas & Actors
- **Primary Actor**: Fleet Dispatcher / Fleet Manager — creates vehicles, onboards drivers, assigns equipment, and updates assignments as schedules shift.
- **Secondary Actor**: Operations Lead / Compliance Officer — reviews fleet readiness, unassigned vehicles, and driver compliance.
- **System Actor**: API Validation & Database Service — enforces tenant boundary isolation and business invariants (e.g., active vehicles having designated contacts).

### 1.3 In-Scope vs. Out of Scope

#### In-Scope
- **Vehicle Creation**: Pick existing contact/driver OR open full Contact Wizard to create a new contact inline and automatically assign upon completion.
- **Contact Creation**: Pick existing vehicle(s) OR open Vehicle Dialog to create a new vehicle inline and automatically assign upon completion.
- **Dedicated Quick-Reassign Modal**: Fast single-click action on vehicle cards/rows to reassign drivers with explicit choice (`Replace Primary` vs. `Add Co-Driver / Secondary`).
- **Contact Multi-Vehicle Assignment**: Batch/multi-select assignment of vehicles to a driver/contact with primary vs. secondary role controls.
- **Assignment Metadata Management**: Assign role (`Driver` vs. `ResponsibleContact`), primary driver indicator (`IsPrimary`), assigned timestamp (`AssignedDate`), and unassigned timestamp (`UnassignedDate`).
- **Invariant & Boundary Guards**: Protect against orphan active vehicles, duplicate active assignments, circular dialog nesting, and unauthorized cross-tenant operations.

#### Out-of-Scope
- GPS automated telematics geofencing-based driver check-in.
- Shift scheduling or payroll clock-in/out integration.
- Driver licensing verification with external DMV APIs (manual license input only).

---

## 2. Behavioral Specifications & Use Cases

### 2.1 Use Cases & Workflows

#### UC-1: Add Vehicle with Existing Contact Assignment
1. User opens "Add Vehicle" dialog.
2. User enters vehicle specs (Unit #, Type, VIN, Make, Model, Year).
3. In the Assignment section, user selects an existing contact from the dropdown, chooses role (`Driver` or `Responsible Contact`), and marks whether primary.
4. User submits form; vehicle and `VehicleContact` link are persisted atomically.

#### UC-2: Add Vehicle with Graceful Transition to Full Contact Wizard
1. User opens "Add Vehicle" dialog and fills in vehicle data.
2. In the Assignment section, user clicks **"+ Add New Driver / Contact"**.
3. The UI gracefully opens the full 5-step Contact Wizard in inline mode while safely preserving parent vehicle draft state.
4. To prevent infinite recursion, nested child entity creation (e.g. "+ Add Vehicle" within this child contact wizard) is disabled.
5. If user cancels the Contact Wizard, the UI returns to the Vehicle Dialog with all vehicle fields intact.
6. Upon completing the Contact Wizard, the new contact is saved to the database, UI returns to the Vehicle Dialog, and the new contact is automatically selected as assigned.
7. If the user subsequently cancels the Vehicle Dialog, the created contact remains safely stored as an unassigned/on-bench contact (`INV-003`).

#### UC-3: Add Contact with Existing Vehicle Assignment
1. User opens "Add Contact" wizard (Step 5 / Assignment step).
2. User searches and selects one or more existing vehicles.
3. User designates role and primary status.
4. Saving the contact creates the contact record and associated `VehicleContact` rows.

#### UC-4: Add Contact with Graceful Transition to Vehicle Dialog
1. User opens "Add Contact" wizard.
2. In the vehicle assignment step, user clicks **"+ Add New Vehicle"**.
3. UI opens Vehicle Dialog in inline mode while preserving contact draft state (with recursive child creation disabled).
4. Upon saving the new vehicle, UI returns to Contact Wizard with the new vehicle pre-selected in the assignment list.
5. User completes and saves the contact.

#### UC-5: Dedicated Quick-Reassign Vehicle Action
1. User clicks **"Reassign"** directly on a vehicle card or table row in Inventory.
2. The **Quick Reassign Modal** displays:
   - Current assigned primary driver and secondary contacts.
   - Dropdown to select a new contact/driver (with compliance badges e.g. ⚠️ CDL Expired if applicable).
   - Clear choice selector:
     - **Replace Primary Driver**: Unassigns current primary driver (stamping `UnassignedDate` and `IsDeleted = true`) and assigns the selected contact as the new primary driver. Secondary contacts remain unaffected.
     - **Add as Co-Driver / Secondary**: Retains existing primary driver and adds the new contact as a secondary `Driver` or `ResponsibleContact`.
3. User confirms; system immediately updates `VehicleContact` assignments and refreshes the UI.

#### UC-6: Multi-Vehicle Assignment to a Driver / Contact
1. User opens "Assign Vehicles" on a Contact's profile/card.
2. User sees a searchable list of fleet vehicles with current primary assignees shown.
3. User selects multiple vehicles via checkboxes.
4. User selects assignment mode:
   - **Assign as Secondary / Float (Default)**: Leaves existing primary drivers intact.
   - **Assign as Primary Driver**: Replaces existing primary drivers on selected vehicles after user confirmation.
5. Saving updates all selected vehicle assignments.

---

## 3. Business Invariants & Edge Case Rules

| Invariant ID | Rule Description | Enforcement Layer |
| :--- | :--- | :--- |
| **INV-001** | **Active Vehicle Contact Requirement**: Every active vehicle must have at least one active assigned contact (`Driver` or `ResponsibleContact`). Removing the sole contact is blocked with a validation error. | API Controller & UI Validator |
| **INV-002** | **Primary Driver Uniqueness**: A vehicle can have at most one primary driver at any given time. Assigning a new primary driver automatically demotes or unassigns the previous primary driver. | Domain Logic & API |
| **INV-003** | **Multi-Vehicle & Unassigned Contact Support**: A contact may be assigned to zero, one, or multiple vehicles simultaneously (e.g., float drivers, fleet managers, or unassigned on-bench drivers). | Data Schema (ADR-022) |
| **INV-004** | **Duplicate Association Guard**: A contact cannot have multiple active `VehicleContact` rows on the same vehicle. Attempting to assign an already-assigned contact updates their role/primary status rather than creating duplicates. | DB Unique Filter & API |
| **INV-005** | **Sole Contact Deletion Guard**: A contact cannot be deactivated or deleted if they are the sole active contact on one or more active vehicles. User must reassign those vehicles first. | API Controller (`ContactsController`) |
| **INV-006** | **Nested Modal Recursion Prevention**: When a dialog is opened from another modal (e.g. Contact Wizard opened from Vehicle Dialog), recursive child spawning (adding another vehicle inside that contact wizard) is strictly disabled. | UI Component State |
| **INV-007** | **Audit Trail & Timestamps**: Unassigning or replacing a contact soft-deletes the `VehicleContact` record (`IsDeleted = true`) and sets `UnassignedDate = UTC_NOW` to retain full historical assignment records (ADR-014). | EF Core / DB Context |
| **INV-008** | **Tenant Isolation**: Contacts and Vehicles can only be associated if they belong to the exact same `TenantId`. | Global Query Filters & RLS (ADR-004) |
| **INV-009** | **Compliance Soft Warnings**: If a selected driver has an expired license/medical certificate, the UI displays an informational warning badge (`⚠️ License Expired`) without blocking urgent operational dispatch. | UI Validator & Component |

---

## 4. Acceptance Criteria Matrix

| ID | Category | Condition / Trigger | Expected Outcome | Verification Mode |
| :--- | :--- | :--- | :--- | :--- |
| `AC-001` | Vehicle Add | Add vehicle picking existing contact | Vehicle created and `VehicleContact` record created with selected role. | Automated API & UI Test |
| `AC-002` | Vehicle Add | Add vehicle launching full Contact Wizard inline | Preserves vehicle draft, creates contact, returns to vehicle modal with contact assigned; prevents nested child recursion. | UI Integration Test |
| `AC-003` | Contact Add | Add contact picking existing vehicle(s) | Contact created and linked to selected vehicle(s). | Automated API & UI Test |
| `AC-004` | Contact Add | Add contact launching Vehicle Dialog inline | Preserves contact draft, creates vehicle, returns to wizard with vehicle assigned; prevents nested child recursion. | UI Integration Test |
| `AC-005` | Quick Reassign | Click 'Reassign' on vehicle row/card | Dedicated modal opens with current driver details and replacement picker. | UI Integration Test |
| `AC-006` | Reassign Replace | Select 'Replace Primary Driver' option | Previous primary driver is soft-deleted with `UnassignedDate`; new driver assigned as primary. Secondary contacts remain unchanged. | Automated API & UI Test |
| `AC-007` | Reassign Co-Driver | Select 'Add as Co-Driver' in Reassign Modal | Existing primary driver retained; new driver added as secondary contact. | Automated API & UI Test |
| `AC-008` | Invariant Guard | Attempting to remove all contacts from an active vehicle | HTTP 400/409 validation error returned and clear UI error displayed. | Automated API & Unit Test |
| `AC-009` | Sole Contact Delete | Deleting/deactivating contact who is sole assignee on active vehicles | HTTP 409 Conflict with list of blocking vehicle unit numbers. | Automated API Test |
| `AC-010` | Duplicate Guard | Assigning already assigned contact to vehicle | Existing record updated rather than creating duplicate row. | Automated API & Unit Test |
| `AC-011` | Multi-Assign | Assigning multiple vehicles to a single contact | Contact successfully linked to all selected vehicles with chosen primary/secondary role. | Automated API & UI Test |
| `AC-012` | Multi-Tenant | Cross-tenant vehicle or contact selection | Prevented by query filters (returns 404/403). | Security Test |
| `AC-013` | Draft Preservation | Cancelling inline child wizard | Parent dialog draft contents remain completely intact without data loss. | UI Component Test |

---

## 5. Decision Log

### Resolved Decisions
- **D-001**: Data model utilizes the existing `VehicleContact` join entity with `AssociationRole`, `IsPrimary`, and `AssignedDate` (refer to ADR-022 & ADR-014). `UnassignedDate` column was considered and rejected during architecture review — the existing `ModifiedDate` on `BaseEntity` (auto-stamped on soft-delete) serves as the unassignment timestamp.
- **D-002**: Reassignment operations preserve audit history via soft-delete on `VehicleContact` join table records. `ModifiedDate` provides the unassignment timestamp; `AssignedDate` is preserved on unchanged assignments via diff-based sync.
- **D-003**: When adding a new driver from the vehicle dialog (or vehicle from contact wizard), the UI gracefully transitions to the full creation wizard/dialog while preserving parent draft state.
- **D-004**: Recursive modal nesting is disabled during inline dialog mode to avoid navigation traps.
- **D-005**: Inventory UI provides a dedicated single-click "Reassign" button & modal on vehicle cards/rows in addition to the full edit dialog.
- **D-006**: Quick Reassignment modal explicitly prompts the user to choose between **"Replace Primary Driver"** and **"Add as Co-Driver / Secondary"**.
- **D-007**: Multi-vehicle batch assignment deferred to a future release ([DEFER-001](file:///c:/Learn/fleet_solution/docs/DEFERRED.md)). Single-vehicle assignment via Reassign Modal covers core operational needs.
- **D-008**: Driver compliance issues (e.g. expired CDL) display a visible warning badge without hard-blocking operational assignments.
