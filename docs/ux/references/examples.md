# UX Engineer Skill — Mode Examples

> One compact worked example per mode.  
> Each example shows input (trigger prompt) + key output excerpts only.

---

## Mode 1 — Quick Patch

**Prompt:** *"The disabled button on the auth form looks too close to the active one — make it more clearly disabled."*

**Step 0 (registry):** AuthInput (primitive, active). No component change needed — this is a CSS-only patch.

**Contrast check:**
```
python contrast.py "#94a3b8" "#FFFFFF"
# Ratio: 2.85:1 — FAILS SC 1.4.3 normal text
# disabled label text; acceptable under SC 1.4.3 note for disabled controls
# (disabled components are exempt from SC 1.4.3 — no WCAG failure)
# SC 1.4.11: UI component — FAIL, but exception applies (disabled control)
```

**Output diff:**
```diff
- .auth-btn:disabled {
-   background: #cbd5e1;
-   color: #94a3b8;
-   cursor: not-allowed;
-   transform: none;
- }
+ .auth-btn:disabled {
+   background: var(--color-neutral-200);   /* oklch(0.92 0.01 240) */
+   color: var(--color-neutral-400);        /* oklch(0.72 0.01 240) */
+   opacity: 0.55;
+   cursor: not-allowed;
+   transform: none;
+   box-shadow: none;
+ }
```

Token mapping: no new primitives. Uses existing neutral scale.

---
Verification
  Contrast script: run — #94a3b8 on #FFFFFF: 2.85:1 (disabled exception applies)
  States covered: disabled
  Authority per finding: SC 1.4.3 (exception: disabled controls)
  Registry checked: yes — 3 entries read
  Decision outcome: reuse as-is (CSS-only patch)
  Consumers searched: NA

---

## Mode 2 — Screen Spec

**Prompt:** *"Design a driver profile summary card for the Inventory page — shows name, license class, expiry date, and assignment status."*

**Step 0 (registry):**
- AuthInput: primitive, text input — purpose differs (display, not input).
- ContactDialog: orchestrator, full contact editor — purpose differs.
- VehicleDialog: orchestrator, vehicle editor — purpose differs.
- **No existing component covers a read-only summary card. Decision: CREATE NEW primitive `DriverSummaryCard`. Pausing to ask.**

*(Example assumes approval was given and continues for illustration.)*

**Component hierarchy:**
```
DriverSummaryCard [primitive]
  ├─ status badge (inline CSS, not a component)
  ├─ <h3> name
  ├─ license class + expiry row
  └─ assignment chip
```

**Responsive behavior:**

| Breakpoint | Layout | Changes |
|---|---|---|
| 375 px | Single column, full width | Stack name above badge |
| 768 px | Two columns in grid | Name + badge inline |
| 1280 px | Three columns in grid | Same as 768; more padding |

**State table:**

| State | Visual | ARIA |
|---|---|---|
| default | Card with shadow | `role="article"` |
| expiry-warning | Amber badge border | `aria-label` includes "expiry warning" |
| expiry-expired | Red badge, bold text | `aria-label` includes "license expired" |

**Key markup excerpt:**
```razor
<article class="driver-card" aria-label="@($"{Driver.FullName}, {Driver.LicenseClass}")">
  <span class="driver-status-badge driver-status-badge--@Driver.AssignmentStatus.ToLower()"
        aria-label="@Driver.AssignmentStatus">
    @Driver.AssignmentStatus
  </span>
  <h3 class="driver-card__name">@Driver.FullName</h3>
  <dl class="driver-card__details">
    <dt>License class</dt><dd>@Driver.LicenseClass</dd>
    <dt>Expires</dt>
    <dd class="@(Driver.IsExpired ? "driver-card__expiry--expired" : "")">
      @Driver.LicenseExpiry.ToString("MMM d, yyyy")
    </dd>
  </dl>
</article>
```

---
Verification
  Contrast script: NA — colors use existing semantic tokens (verify on implementation)
  States covered: default, expiry-warning, expiry-expired
  Authority per finding: NA — no violations found in spec
  Registry checked: yes — 3 entries read
  Decision outcome: new (ask) — DriverSummaryCard primitive
  Consumers searched: NA — new component

---

## Mode 3 — Review / Audit

**Prompt:** *"Audit the AuthInput component."*

**Step 0 (registry):** AuthInput (primitive, active, verified_commit: abc1234). Drift check: git log matches. Source read.

**Findings:**

| ID | Priority | Authority | Description | Fix |
|---|---|---|---|---|
| A3-001 | P0 | [WCAG SC 2.4.11] | `:focus-visible` is suppressed (`outline: none` on `h1:focus`). AuthInput itself has no explicit `:focus-visible` style — relies on browser default, which may not meet 3:1 area/contrast threshold in all browsers. | Add explicit `:focus-visible` rule per tokens.md §Focus Appearance. |
| A3-002 | P1 | [WCAG SC 1.4.1] | Error state indicated only by the `auth-validation-msg` text color (red). The input border does not change color on error. Color alone is used as the sole differentiator for the error state on the input border. | Add `border-color: var(--color-danger-500)` when `aria-invalid="true"`. |
| A3-003 | P2 | [APG] | `autocomplete` attribute is not exposed as a parameter. AuthInput supports auth flows (username, password) where `autocomplete` is expected. | Add optional `Autocomplete` parameter defaulting to `null`. |

**Contrast checks:**
```
python contrast.py "#ef4444" "#FFFFFF"
# error text: 3.93:1 — FAILS SC 1.4.3 normal text
```
Finding A3-004 (P1): [WCAG SC 1.4.3] — error message color #ef4444 on white: 3.93:1. Below 4.5:1 AA normal text threshold.  
Fix: use `var(--color-danger-700)` = oklch(0.35 0.17 25); verify with `contrast.py`.

---
Verification
  Contrast script: run — #ef4444 on #FFFFFF: 3.93:1 FAIL SC 1.4.3
  States covered: default, focus, error, disabled
  Authority per finding: SC 2.4.11 (A3-001), SC 1.4.1 (A3-002), APG (A3-003), SC 1.4.3 (A3-004)
  Registry checked: yes — 1 entry read (AuthInput)
  Decision outcome: NA — audit mode
  Consumers searched: NA — audit mode

---

## Mode 4 — Library Maintenance

**Prompt:** *"Run a library maintenance check."*

**Step 0:** Read registry. 3 entries. Drift check all 3:

```
git log -1 --format=%h -- Components/AuthInput.razor      → abc1234 (matches)
git log -1 --format=%h -- Components/ContactDialog.razor  → def5678 (matches)
git log -1 --format=%h -- Components/VehicleDialog.razor  → ghi9012 (matches)
```

No drift detected. Layout components (MainLayout, NavMenu) reviewed for standards; not registry-managed.

**Findings:**

| ID | Priority | Authority | Description | Fix |
|---|---|---|---|---|
| M4-001 | P1 | [ADR-018] | ContactDialog uses a multi-step wizard with inline step content (all steps in one file). ADR-018 says each step should be its own child component (where ADR-018 is silent, the skill defaults to per-step child components). Current implementation departs from this default. | P2 finding only — do not refactor ContactDialog per SKILL.md §3b rule 4. Document as technical debt. |
| M4-002 | P2 | [Heuristic: Nielsen #4] | VehicleDialog and ContactDialog share structural modal markup (backdrop, modal-wrapper, modal-dialog-custom, modal-content-custom, header pattern). Near-duplication of modal shell — no common BaseDialog primitive exists. | Propose `ModalShell` composite in next planning cycle. Pausing to ask before any work. |
| M4-003 | P1 | — | NavMenu: `navbar-toggler` button has `title` but no `aria-label`. Title is not reliably announced by all screen readers. | Add `aria-label="Toggle navigation menu"` alongside `title`. |
| M4-004 | P2 | [WCAG SC 2.4.11] | MainLayout has `h1:focus { outline: none }` in app.css. While h1 is typically not focusable, if JavaScript or FocusOnNavigate sets focus on it, this suppresses the visible focus indicator. | Remove the rule or scope it to `h1:not(:focus-visible)`. |

---
Verification
  Contrast script: NA — no color pairs in scope for this run
  States covered: NA
  Authority per finding: ADR-018 (M4-001), Nielsen #4 (M4-002), SC 2.4.11 (M4-004), ARIA practice (M4-003)
  Registry checked: yes — 3 entries read, 0 drift
  Decision outcome: NA — maintenance audit
  Consumers searched: yes — VehicleDialog and ContactDialog share modal CSS (found by grep)
