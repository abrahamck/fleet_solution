# States & Accessibility Reference

> Reference loaded on demand by the `ux-engineer` skill.  
> All contrast ratios must be verified with `contrast.py` — never estimated.

---

## 1. Interactive State Matrix

All interactive components must handle every applicable state. Mark N/A with justification.

| State | CSS pseudo / class | Visual signal required | ARIA |
|---|---|---|---|
| default | — | Normal appearance | — |
| hover | `:hover` | Color shift, cursor change | — |
| focus-visible | `:focus-visible` | Focus ring (see §2) | — |
| active | `:active` | Press depression (transform or shadow) | — |
| disabled | `[disabled]`, `.disabled` | Reduced opacity or muted color; `cursor: not-allowed` | `aria-disabled="true"` or `disabled` attr |
| error | `.invalid`, `aria-invalid` | Red border/outline + error text below | `aria-invalid="true"`, `aria-describedby` pointing to error element |
| loading | `.loading`, spinner | Spinner + `disabled` interaction | `aria-busy="true"`, `aria-live` on status |
| checked/selected | `:checked`, `[aria-selected]` | Filled indicator or highlight | `aria-checked`, `aria-selected` |
| expanded | `[aria-expanded]` | Icon rotation or panel open | `aria-expanded="true/false"` |
| current | `.active`, `[aria-current]` | Bold / underline / indicator | `aria-current="page"` for nav |

### FleetNexus-specific state patterns

| Component | Extra states | Notes |
|---|---|---|
| AuthInput | `has-error`, `focused-with-icon` | Icon color shifts on focus (see CSS). Ensure icon color change is not the only error cue [WCAG SC 1.4.1]. |
| ContactDialog | `step-active`, `step-completed`, `step-future` | Step circles use color + number/checkmark — color not sole differentiator [WCAG SC 1.4.1]. |
| VehicleDialog | `submitting` | All inputs disabled, close button disabled, spinner shown |
| NavMenu | `collapsed`, `expanded` (mobile), `current-page` | `aria-current="page"` on active NavLink |

---

## 2. Focus Appearance (SC 2.4.11 — AA)

SC 2.4.11 requires the focus indicator to have:
- Area ≥ perimeter of the unfocused component × 1 CSS px
- Contrast ≥ 3:1 between focused and unfocused states

FleetNexus default focus ring:

```css
:focus-visible {
  outline: 2px solid var(--color-border-focus);       /* cobalt-500 */
  outline-offset: 2px;
  box-shadow: 0 0 0 4px var(--color-focus-ring);      /* cobalt-500 @ 35% */
}
```

Verify: `contrast.py "#1A73E8" "#F4F6F8"` (cobalt on app bg).  
If result < 3:1, escalate to cobalt-600 for the outline.

**Buttons with custom box-shadow focus only** (`.btn:focus` in app.css):  
The current `box-shadow: 0 0 0 0.1rem white, 0 0 0 0.25rem #258cfb` pattern is valid under SC 2.4.11
if the outer ring contrasts ≥ 3:1 against its adjacent background. Verify per instance.

---

## 3. Target Size (SC 2.5.8 — AA)

Minimum: **24×24 CSS px** for the target size OR 24 px spacing from adjacent targets.

Check exceptions **in this order** before flagging:
1. **Spacing exception** — target < 24 px but has ≥ 24 px offset from all adjacent targets.
2. **Equivalent control** — an accessible alternative ≥ 24 px performs the same function.
3. **Inline** — target is in a sentence or list.
4. **User-agent-controlled** — size is set by the browser and not modified.
5. **Essential** — the target's size is essential to the information conveyed.

44×44 px is SC 2.5.5 (AAA) — a recommendation only. Do not reject conformant interfaces.

---

## 4. Motion (prefers-reduced-motion)

All animations must be wrapped:

```css
@media (prefers-reduced-motion: no-preference) {
  .animate-fade-in { animation: fadeIn 0.2s ease; }
  .wizard-track-progress { transition: width 0.3s ease; }
}
```

Animations currently in app.css that need this wrapper:
- `@keyframes slideDown` (`.auth-validation-msg`)
- `@keyframes spin` (`.btn-spinner`)
- `transform` transitions on `.auth-card:hover`, `.auth-btn:hover`
- `transform` on `.step-indicator.active .step-dot`

---

## 5. Forced Colors

```css
@media (forced-colors: active) {
  /* Focus ring must use a system keyword, not a custom color */
  :focus-visible {
    outline: 3px solid ButtonText;
    outline-offset: 2px;
    box-shadow: none;
  }

  /* Step dots: rely on border rather than fill color */
  .step-dot {
    border: 2px solid ButtonText;
    background-color: Canvas;
  }
  .step-indicator.active .step-dot {
    background-color: Highlight;
    border-color: Highlight;
  }
  .step-indicator.completed .step-dot {
    background-color: ButtonText;
  }

  /* Glass card: remove backdrop blur, use solid surface */
  .auth-card {
    background: Canvas;
    border: 1px solid ButtonText;
    backdrop-filter: none;
  }
}
```

---

## 6. Contrast Quick-Reference (run contrast.py to confirm)

| Pair | Approx ratio | SC 1.4.3 normal | SC 1.4.11 |
|---|---|---|---|
| Navy (#2C4B64) on White | ~9.4:1 | PASS | PASS |
| Cobalt (#1A73E8) on White | ~4.6:1 | PASS | PASS |
| Cobalt (#1A73E8) on BgGray (#F4F6F8) | ~4.3:1 | FAIL (borderline) | PASS |
| Green (#28D07C) on White | ~2.5:1 | FAIL | FAIL |
| White on Cobalt (#1A73E8) | ~4.6:1 | PASS | PASS |
| White on Navy (#2C4B64) | ~9.4:1 | PASS | PASS |
| Error red (#ef4444) on White | ~3.9:1 | FAIL normal / PASS large | PASS |

> **Note on Green:** brand green (#28D07C) fails SC 1.4.3 for normal text on white.  
> Use white text on green backgrounds for AA conformance. Never use green as a text color on light backgrounds.  
> **Note on Cobalt on BgGray:** borderline. Verify with `contrast.py "#1A73E8" "#F4F6F8"` — result depends on exact OKLCH gamut mapping. If < 4.5:1, use cobalt-600 for body links on the bg-gray surface.

---

## 7. Keyboard Contract by Widget Type

| Widget | Keys required | APG pattern |
|---|---|---|
| Button | `Enter`, `Space` | Button |
| Link | `Enter` | Link |
| Tab panel (Inventory tabs) | `Arrow Left/Right` to switch tabs, `Tab` into panel | Tabs |
| Dialog (ContactDialog, VehicleDialog) | `Esc` closes, focus trapped inside, initial focus on first interactive | Dialog (Modal) |
| Step wizard node (ContactDialog) | `Enter`/`Space` to navigate to a step; treated as a tab list | Tabs |
| Disclosure / nav toggle | `Enter`/`Space` to expand, `Esc` to collapse | Disclosure |
| Form input | Standard; `Tab`/`Shift-Tab` to move between fields | — |

> Verify with manual keyboard testing before marking P0/P1 issues closed.
