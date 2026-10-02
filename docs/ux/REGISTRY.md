# FleetNexus Component Registry

> **Skill-owned.** Managed by the `ux-engineer` skill.  
> The registry is an index — source files are the source of truth.  
> Before any decision, check drift: `git log -1 --format=%h -- <path>` vs `verified_commit`.  
> Stale entries must be reconciled before use. Do not add entries for Layout/ components (NavMenu, MainLayout) — those are audited but not reuse-managed.

---

## Scope

**In registry:** `Components/` — reusable UI primitives, composites, and orchestrators.  
**Out of registry (audited by Mode 3):** `Layout/` (MainLayout, NavMenu) and `Pages/`.

---

## Entries

---

### AuthInput

```yaml
name: AuthInput
path: app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/AuthInput.razor
kind: primitive
purpose: >-
  Atomic labeled text/password input with icon support, inline validation
  message, aria-invalid/aria-describedby wiring, and EditContext integration.
  Used in authentication and simple form flows.
data_fields:
  - Value: string? — bound input value
props:
  - Id: string? — optional explicit id; auto-generated if omitted (default: null)
  - Title: string? — optional section heading rendered above the label (default: null)
  - Label: string? — visible label text (default: null)
  - Placeholder: string? — placeholder text (default: null)
  - Type: string — HTML input type, e.g. "text", "password", "email" (default: "text")
  - InputMode: string? — HTML inputmode for mobile keyboards (default: null)
  - Autocapitalize: string? — autocapitalize hint (default: null)
  - Autocorrect: string? — autocorrect hint (default: null)
  - Disabled: bool — disables the input (default: false)
  - Error: string? — explicit error message override (default: null)
  - For: Expression<Func<string>>? — model expression for EditContext validation (default: null)
  - ValueChanged: EventCallback<string?> — two-way binding callback
  - AdditionalAttributes: Dictionary<string, object>? — splatted onto the input element
slots: []
variants: []
states:
  - default
  - focus-visible
  - disabled
  - error (aria-invalid + error message)
a11y_notes: >-
  Sets aria-invalid="true" and aria-describedby="{id}-error" when in error state.
  Error message rendered in a <span role="alert">. Icon is decorative (pointer-events: none).
  No explicit autocomplete parameter — open finding A3-003 (P2).
  Focus style relies on browser default — open finding A3-001 (P0, needs explicit :focus-visible rule).
status: active
deprecation_reason: ~
verified_commit: de98169
```

---

### ContactDialog

```yaml
name: ContactDialog
path: app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/ContactDialog.razor
kind: orchestrator
purpose: >-
  5-step wizard dialog for creating and editing a contact (person) record:
  Identity, Phones, Address & Email, Compliance, Review. Manages step state,
  per-step validation, and submission. Domain: contact directory.
data_fields:
  - Contact: ContactModel — the contact entity being created or edited
  - IsEditMode: bool — true = editing existing, false = adding new
  - IsVisible: bool — controls dialog visibility
  - IsSubmitting: bool — submission in progress
  - ErrorMessage: string? — top-level submission error
props:
  - Contact: ContactModel [Parameter] — contact model (default: new ContactModel)
  - IsEditMode: bool [Parameter] — edit vs create mode (default: false)
  - IsVisible: bool [Parameter] — show/hide (default: false)
  - IsSubmitting: bool [Parameter] — disables form while submitting (default: false)
  - ErrorMessage: string? [Parameter] — server-side error display (default: null)
  - OnSave: EventCallback<ContactModel> [Parameter] — fires on valid submit
  - OnCancel: EventCallback [Parameter] — fires on cancel/close
slots: []
variants: []
states:
  - step-1-active through step-5-active
  - step-completed (visual: checkmark in step circle)
  - submitting (all inputs and close button disabled)
  - error (top-level alert + step-level alert)
a11y_notes: >-
  Dialog uses role="dialog" on the wrapper. Modal backdrop present.
  Step nodes are clickable list items — should implement role="tab" + aria-selected
  per APG Tabs pattern for full keyboard compliance (open audit finding).
  Close button has aria-label="Close". Focus trap not explicitly implemented —
  open finding.
  ADR-018 compliance: wizard pattern matches ADR-018 intent but step content is
  inlined (all 5 steps in one file) rather than per-step child components.
  Reported as M4-001 (P2 technical debt).
status: active
deprecation_reason: ~
verified_commit: b0056c6
```

---

### VehicleDialog

```yaml
name: VehicleDialog
path: app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/VehicleDialog.razor
kind: orchestrator
purpose: >-
  Single-page form dialog for creating and editing a vehicle record:
  specifications (unit number, type, status, make, model, year, VIN, license
  plate, state), driver assignment, and garage location. Domain: vehicle fleet.
data_fields:
  - Vehicle: VehicleModel — the vehicle entity being created or edited
  - IsVisible: bool — controls dialog visibility
  - IsSubmitting: bool — submission in progress
  - ErrorMessage: string? — top-level submission error
props:
  - Vehicle: VehicleModel [Parameter] (default: new VehicleModel)
  - IsVisible: bool [Parameter] (default: false)
  - IsSubmitting: bool [Parameter] (default: false)
  - ErrorMessage: string? [Parameter] (default: null)
  - OnSave: EventCallback<VehicleModel> [Parameter]
  - OnCancel: EventCallback [Parameter]
slots: []
variants: []
states:
  - default (form editable)
  - submitting (all inputs and close button disabled, error alert shown)
  - error (top-level alert)
a11y_notes: >-
  Dialog uses role="dialog". Close button has aria-label="Close" via title attr
  only — aria-label should be made explicit. Focus trap not implemented.
  Uses EditForm + DataAnnotationsValidator for validation; ValidationMessage
  components render error text inline (not role="alert"). Consider adding
  aria-live or role="alert" on the error summary.
status: active
deprecation_reason: ~
verified_commit: b0056c6
```

---

### ReassignModal

```yaml
name: ReassignModal
path: app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/ReassignModal.razor
kind: orchestrator
purpose: >-
  Quick reassignment dialog for a vehicle: displays current active primary and
  secondary contacts, mode selector (Replace Primary vs. Add Co-Driver),
  contact picker with CDL compliance indicator badges (INV-009), and submits to
  the dedicated POST /api/vehicles/{id}/reassign endpoint.
data_fields:
  - Vehicle: VehicleDto? — the vehicle whose assignments are being modified
  - IsVisible: bool — controls dialog visibility
props:
  - IsVisible: bool [Parameter] (default: false)
  - Vehicle: VehicleDto? [Parameter] (default: null)
  - AvailableContacts: List<ContactSummaryDto> [Parameter] (default: empty list)
  - OnSaved: EventCallback [Parameter] — fires after successful reassignment
  - OnCancel: EventCallback [Parameter] — fires on modal cancel/close
slots: []
variants: []
states:
  - default
  - submitting (inputs disabled, spinner displayed)
  - error (top-level alert)
a11y_notes: >-
  Dialog uses role="dialog", aria-modal="true", and aria-labelledby. Close button
  has aria-label="Close". Radio cards for mode selection use native radio buttons
  with descriptive labels.
status: active
deprecation_reason: ~
verified_commit: working-tree
```

---

## Open Findings (tracked from Mode 3 / Mode 4 runs)

These findings were identified during registry bootstrap. Address in planned work.

| ID | Component | Priority | Summary |
|---|---|---|---|
| A3-001 | AuthInput | P0 | No explicit `:focus-visible` rule — relies on browser default |
| A3-002 | AuthInput | P1 | Error state uses color alone on input border (no border-color change) |
| A3-003 | AuthInput | P2 | No `Autocomplete` parameter |
| A3-004 | AuthInput | P1 | Error message color #ef4444 on white: ~3.93:1 — below SC 1.4.3 AA |
| M4-001 | ContactDialog | P2 | Step content inlined vs per-step child components (ADR-018 default) |
| M4-002 | VehicleDialog + ContactDialog | P2 | Near-duplicate modal shell markup — propose BaseDialog composite |
| M4-003 | NavMenu (Layout) | P1 | navbar-toggler missing aria-label |
| M4-004 | MainLayout (Layout) | P2 | `h1:focus { outline: none }` suppresses focus on programmatically focused headings |

---

## Registry Bootstrap Metadata

```
Bootstrapped: 2026-10-02
Bootstrapped by: ux-engineer skill (Mode 4 bootstrap)
Components scanned: Components/ (3 files)
Layout reviewed (not registered): Layout/ (2 files — MainLayout, NavMenu)
Next full drift check: run Mode 4 or any mode that touches UI
```
