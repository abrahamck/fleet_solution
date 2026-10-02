# Component Governance — Decision Ladder

> This document is a reference loaded on demand by the `ux-engineer` skill.  
> Do not modify the decision order — it is deterministic and enforced by the skill.

---

## Decision Ladder

The skill checks in this exact order and stops at the first match:

```
1. Purpose differs?           → CREATE NEW   (field overlap irrelevant)
2. Interaction model differs? → CREATE NEW
3. States / responsive differ?→ CREATE NEW
4. Field overlap analysis:
     needed ⊆ existing AND unused ≤ 40%? → REUSE AS-IS
     gap closable with additive props?    → PROPOSE EXTENSION (ask)
     otherwise                           → CREATE NEW (ask)
```

**Silent actions** (state but do not pause): reuse as-is, reuse with configuration, compose.  
**Pause and ask**: extend existing, add variant, create new.

---

## Field-Overlap Thresholds (tunable)

| Rule | Default threshold | Fires when |
|---|---|---|
| Reuse as-is | unused ≤ 40% of existing fields | All needed fields covered |
| Propose extension | gap closable with additive optional props or slots | Needed fields not all covered |
| Create new | purpose or interaction differs | Steps 1–3 |

The skill states which rule fired.

---

## Complexity Ceiling by Kind

| Kind | Flag ceiling | Total `[Parameter]` soft limit | When exceeded |
|---|---|---|---|
| `primitive` | 5 | 8 | Propose enum Variant or split; pause |
| `composite` | 6 | 12 | Propose enum Variant or split; pause |
| `orchestrator` | 4 | 12 | Propose enum Variant or split; pause |

A prop that fundamentally changes layout (e.g., switching from single-column to multi-pane)
always triggers a split proposal regardless of count.

Exceeding **total** soft limit: add a "consider splitting" note only — no pause required.

---

## Wizard / Orchestrator Rules (ADR-018 authoritative)

Read ADR-018 before any wizard or dialog work. Where ADR-018 is silent:
- Each step is its own child component.
- Step state flows through a single model / state object (EditContext where the step contains a form).
- Step count is **not** a parameter.
- Do not refactor ContactDialog. Deviations from ADR-018 are reported as P2 findings in Mode 4.

---

## Pause & Ask Template

When the decision requires a pause, present:

```
## Component Decision Required

**What's needed:** [describe the new requirement]

**Existing candidates checked:**
| Component | Kind | Why it does / doesn't match |
|---|---|---|
| ComponentName | orchestrator | [outcome] |

**Options:**
A. Extend `ComponentName` — [trade-offs] — backward-compatible: [yes/no]
B. Add a variant — [trade-offs]
C. Create `NewComponentName` — [trade-offs]

**Skill recommendation:** Option [X] because [reason].

**Consumers of affected component:** [found by grep / none]

**Your approval is needed before any code is written.**
```

---

## Registry Entry Format

```yaml
- name: ComponentName
  path: app-fleet-nexus-net/ui/appfleet-nexus-ui/Components/ComponentName.razor
  kind: primitive | composite | orchestrator
  purpose: >-
    One-line description of what this component does and its domain context.
  data_fields:
    - FieldName: type — description
  props:
    - ParameterName: type — description (default: value)
  slots: []          # RenderFragment parameters, if any
  variants: []       # enum values or boolean flags that change appearance/behavior
  states:
    - default
    - focus
    - disabled
    - error
    - loading        # if applicable
  a11y_notes: >-
    ARIA roles, attributes, keyboard contract, SC citations.
  status: active | deprecated
  deprecation_reason: ~   # fill if status == deprecated
  verified_commit: abc1234
```

### Worked Example A — Reuse (fewer fields needed)

**Scenario:** Signup form needs a text input for a referral code.

Registry check: `AuthInput` — purpose: atomic text/password input with label, validation, icon support.  
Need: labeled text input with validation. All needed fields covered; AuthInput has 9 parameters, none unused for this use.  
Unused fields ≤ 40%? Yes (Type, InputMode, Autocapitalize, Autocorrect are optional/defaulted).  
**Decision: REUSE AS-IS (silent). No new component.**

```razor
<AuthInput Label="Referral Code (optional)"
           Placeholder="e.g. FLEET2026"
           @bind-Value="model.ReferralCode" />
```

### Worked Example B — Extension needed (more fields needed)

**Scenario:** Vehicle card needs a "hazmat certified" badge icon shown conditionally.

Registry check: no primitive badge/tag component exists. Nearest: AuthInput (text input, wrong purpose at step 1).  
**Decision: CREATE NEW — purpose differs at step 1. Pausing to ask.**

```
## Component Decision Required

**What's needed:** A small badge/chip showing a certification label with an icon,
  conditionally rendered based on a boolean and a label string.

**Existing candidates checked:**
| Component       | Kind        | Outcome                          |
|-----------------|-------------|----------------------------------|
| AuthInput       | primitive   | Purpose differs (input vs display)|
| VehicleDialog   | orchestrator| Purpose differs (editor vs badge) |
| ContactDialog   | orchestrator| Purpose differs                   |

**Options:**
A. Create `CertificationBadge` primitive — additive, no existing component affected.
B. Inline the badge markup in the vehicle card page — not reusable.

**Skill recommendation:** Option A. Purpose is display-only, no existing primitive covers it.
Consumers of existing components: unaffected.

**Your approval is needed before any code is written.**
```
