---
name: ux-engineer
description: >-
  Governs UX design, accessible frontend engineering, and component-library
  governance for FleetNexus (Blazor WASM / .NET 10, Bootstrap 5.3, Inter font,
  CSS custom properties). Trigger this skill when designing screens, building or
  changing UI components, auditing UI for accessibility or usability, generating
  design tokens, or maintaining the component registry. Layout and navigation
  components (MainLayout, NavMenu) are included in audits (Modes 2–3) but are
  not registry-managed reuse candidates.
references:
  - docs/ux/references/tokens.md
  - docs/ux/references/component-governance.md
  - docs/ux/references/states-and-a11y.md
  - docs/ux/references/examples.md
  - docs/ux/REGISTRY.md
  - docs/ADR/ADR-018-multi-step-input-wizard-ux-pattern.md
---

# UX Engineer Skill — FleetNexus

## Stack Quick Reference

| Layer | Choice |
|---|---|
| Framework | Blazor WebAssembly (.NET 10) |
| Language | Razor + C# |
| Styling | Vanilla CSS (`wwwroot/css/app.css`) + Bootstrap 5.3 (CDN) |
| Icons | Bootstrap Icons 1.11 |
| Font | Inter (Google Fonts, weights 300–700) |
| Components dir | `app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/` |
| Layout dir | `app-fleet-nexus-net/ui/appfleet-nexus-ui/Layout/` |
| Registry | `docs/ux/REGISTRY.md` |
| Token reference | `docs/ux/references/tokens.md` |

Breakpoints: **375 / 768 / 1280 px** (mobile / tablet / desktop).  
WCAG target: **2.2 AA**. Dark mode: token-ready but not yet implemented in CSS (see tokens.md).  
ADR-018 (wizard pattern) is authoritative for multi-step flows; read it before any wizard work.  
ADRs outrank skill defaults. Step 0 checks relevant ADRs alongside the registry.

---

## 1. Mode Selection

| Trigger keywords | Default mode |
|---|---|
| "fix", "patch", "tweak", "quick change" | **Mode 1** |
| "design", "spec", "screen", "layout", "new page" | **Mode 2** |
| "audit", "review", "check", "a11y", "WCAG" | **Mode 3** |
| "registry", "library", "duplicate", "catalog", "bootstrap registry" | **Mode 4** |

**When ambiguous: default to Mode 3** (safest; produces findings without changing code).  
State the selected mode and reason at the top of every response.

---

## 2. Standards & Authority Labels

Label every finding with its authority type. Never conflate types.

| Label | Authority | Conformance? |
|---|---|---|
| `[WCAG SC X.X.X]` | WCAG 2.2 Success Criterion | Yes — AA normative |
| `[APG]` | WAI-ARIA Authoring Practices Guide | No — implementation guidance |
| `[Heuristic: Nielsen #N]` | Nielsen's 10 Usability Heuristics | No — usability guidance |
| `[ADR-NNN]` | Project Architectural Decision Record | Yes — project normative |

**SC numbers to cite without lookup:** 1.4.1 (use of color), 1.4.3 (contrast text),
1.4.11 (contrast UI components), 2.4.7 (focus visible), 2.4.11 (focus appearance AA),
2.5.7 (dragging movements), 2.5.8 (target size AA), 3.3.7 (redundant entry), 3.3.8 (accessible authentication).

**Target size rule:** SC 2.5.8 requires 24x24 CSS px minimum with exceptions (spacing,
equivalent control, inline, user-agent-controlled, essential). Check all five exceptions
before flagging. 44x44 px is SC 2.5.5 AAA — a recommendation, not an AA requirement.
Do not reject conformant interfaces on the basis of 44 px.

**If no SC directly applies:** state "no directly applicable WCAG SC" and cite APG or a
named heuristic separately. Never invent or approximate a criterion.

**Always cover:** `forced-colors` media query, `prefers-contrast`, `prefers-reduced-motion`.

---

## 3. Component Governance (Step 0 — mandatory in every mode that touches UI)

### 3a. Read before deciding

1. Read `docs/ux/REGISTRY.md`.
2. For every candidate component, read its actual source file.
3. Check drift: run `git log -1 --format=%h -- <path>` and compare to `verified_commit`.
   If they differ OR `git status --porcelain -- <path>` shows changes, re-read source
   and update the entry before deciding anything.
4. Flag components in code but not in registry (and vice versa) as P1 findings in Mode 4.
5. Check relevant ADRs (especially ADR-018 for any dialog or wizard work).
6. Show what was checked and the outcome before any decision.

### 3b. Component kind classification

| Kind | Definition | Flag ceiling | Total `[Parameter]` soft limit |
|---|---|---|---|
| `primitive` | Atomic input or display element (e.g., AuthInput) | 5 flag/variant | 8 |
| `composite` | Assembles primitives (e.g., a form section) | 6 flag/variant | 12 |
| `orchestrator` | Wizard, dialog, page-level container (e.g., ContactDialog, VehicleDialog) | 4 flag | 12 |

Exceeding a flag ceiling -> propose enum `Variant` or split; **pause and ask**.  
Exceeding total soft limit -> add "consider splitting" note; no pause.  
Classify on creation; confirm with user when ambiguous. State which rule fired.

### 3c. Decision ladder (load `docs/ux/references/component-governance.md` for full detail)

Check in this order — stop at the first match:
1. **Purpose** differs -> skip to "create new"; field overlap is irrelevant.
2. **Interaction model** differs -> same.
3. **States / responsive behavior** differ -> same.
4. **Field overlap** (only if 1-3 match):
   - 100% needed fields covered AND unused fields <= 40% -> **reuse as-is** (silent).
   - Gap closable with additive optional props/slots -> **propose extension** (ask).
   - Otherwise -> **create new** (ask).

**Reuse as-is, with configuration, and composition -> proceed silently but state the decision.**  
**Extend, add variant, create new -> PAUSE. Ask with:** what's needed, what exists,
options + trade-offs, recommendation, consumers found by `grep -r "ComponentName"`,
whether the change is backward-compatible (additive optional props, existing defaults preserved).

### 3d. Registry update rule

Update `docs/ux/REGISTRY.md` (including `verified_commit`) **in the same task as the
implementation**, before reporting completion. Never report the registry as updated before
the implementation exists. If the user edits components independently, reconcile the
registry the next time the skill touches that component or runs Mode 4.

---

## 4. Mode Outputs

### Mode 1 — Quick Patch
Output: minimal Razor/CSS diff + token mapping for any new color values.  
Run `contrast.py` for every color pair introduced. Show pass/fail.

### Mode 2 — Screen Spec
Output:
1. **Hierarchy**: component tree with kind labels.
2. **Responsive behavior table**: what stacks / collapses / changes density at each breakpoint.
3. **State table**: all interactive states (see `docs/ux/references/states-and-a11y.md`).
4. **Implementation**: Razor markup + CSS diff using existing tokens.
5. Step 0 registry check must precede implementation.

### Mode 3 — Review / Audit
Scope: all `.razor` files in Components/ and Layout/. Output findings as P0/P1/P2.

| Priority | Meaning |
|---|---|
| P0 | Blocks release: WCAG AA failure, broken interaction, crashes |
| P1 | Should fix before ship: WCAG AA risk, significant usability harm |
| P2 | Recommended improvement: best-practice deviation, minor heuristic issue |

Each finding: **ID * authority label * description * concrete fix**.  
Run `contrast.py` for every color pair examined.

### Mode 4 — Library Maintenance
Output P0/P1/P2 findings covering:
- Registry/code drift (stale `verified_commit`, missing entries, ghost entries).
- Duplicate and near-duplicate components (purpose + interaction check first).
- Deprecation candidates with rationale.
- Consolidation proposals with backward-compatibility analysis.
- ADR-018 compliance check for all orchestrator components.

Bootstrap trigger: if `docs/ux/REGISTRY.md` does not exist, scan Components/ and draft it for review.

---

## 5. Token System

Full OKLCH scale and output formats in `docs/ux/references/tokens.md`.  
Three tiers: **primitive** (oklch scale) -> **semantic** (role-named) -> **component** (scoped).  
Output as CSS custom properties in `app.css` and as DTCG JSON (`$value`, `$type`).  
Bootstrap 5.3 custom properties (`--bs-primary`, `--bs-body-bg`, etc.) are mapped at the semantic tier.  
Dark mode tokens are defined in the reference but not yet applied in CSS.

---

## 6. Contrast Script

**Script:** `.agents/skills/ux-engineer/scripts/contrast.py`  
Run for every color pair — never estimate visually.  
Input: hex / rgb / oklch. Supports alpha compositing over a background.  
Output: WCAG relative luminance, contrast ratio, SC 1.4.3 pass/fail (normal + large text), SC 1.4.11 pass/fail.

Usage (use whichever Python interpreter is on PATH: `py`, `python`, or `python3`):
```
py .agents/skills/ux-engineer/scripts/contrast.py "#1A73E8" "#FFFFFF"
py .agents/skills/ux-engineer/scripts/contrast.py "oklch(0.48 0.20 264)" "#F4F6F8"
```

---

## 7. Verification Line

Append to **every response**:

```
---
Verification
  Contrast script: [run / not run / NA -- reason]
  States covered: [list or NA]
  Authority per finding: [SC X.X.X / APG / Heuristic / ADR-NNN or NA]
  Registry checked: [yes -- N entries read / NA -- mode does not touch UI]
  Decision outcome: [reuse as-is | reuse with config | compose | extend (ask) | new (ask) | NA]
  Consumers searched: [yes -- found: X | none found | NA]
```

---

## 8. Test Prompts

Load `docs/ux/references/examples.md` for one worked example per mode.

Validation prompts (expected behavior in parentheses):

1. **Silent reuse** -- *"Add a text input to the signup form for a referral code."*
   (Step 0 finds AuthInput covers the purpose; skill reuses as-is and states the decision silently.)

2. **Ask: extend** -- *"The vehicle card needs a 'hazmat certified' badge that only shows for certain vehicle types."*
   (Purpose and interaction match an existing component; gap requires an additive optional prop; skill pauses and asks.)

3. **New component** -- *"Design a driver fatigue score gauge -- a radial progress ring with a numeric label and a color band indicating risk level."*
   (Purpose differs from all registered components; skill proposes a new primitive after checking the registry and pauses to ask.)

4. **Field overlap, purpose differs** -- *"Can I use VehicleDialog to collect carrier contact info?"*
   (Purpose differs -- vehicle specifications vs. person contact directory; skill stops the decision at step 1 and proposes ContactDialog or a new component. States: "field overlap check not applicable -- purpose differs at step 1.")

5. **Audit with tempting non-applicable SC** -- *"Audit the step wizard progress bar in ContactDialog."*
   (Skill checks contrast, focus, motion. The decorative progress line is not an interactive control; SC 2.5.8 does not apply to decorative SVG/CSS elements. Skill states "no directly applicable WCAG SC for the decorative track element" and cites APG Tab widget guidance for the clickable step nodes.)
