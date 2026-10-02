# FleetNexus Design Token System

> **Scope:** OKLCH primitive-to-semantic-to-component token tiers.  
> Brand hex values have been converted to OKLCH equivalents.  
> Dark mode tokens are **defined here** but not yet applied in CSS — dark mode is a future feature.  
> All CSS output targets `wwwroot/css/app.css`. Bootstrap 5.3 custom properties are mapped at the semantic tier.

---

## Hex → OKLCH Brand Conversion

| Brand name | Original hex | OKLCH equivalent | Notes |
|---|---|---|---|
| Navy | `#2C4B64` | `oklch(0.37 0.07 237)` | Dark blue-slate |
| Cobalt | `#1A73E8` | `oklch(0.52 0.20 264)` | Primary action blue |
| Green | `#28D07C` | `oklch(0.75 0.19 155)` | Success / CTA green |
| White | `#FFFFFF` | `oklch(1.00 0.00 0)` | Surface |
| BgGray | `#F4F6F8` | `oklch(0.97 0.00 240)` | App background |
| BorderBlue | `#8AB4F8` | `oklch(0.75 0.12 264)` | Input border |

> Conversion method: sRGB -> linear sRGB -> OKLab -> OKLCH.  
> Chroma is rounded to 2 decimal places; hue to nearest degree.  
> Gamut mapping: clamp L to [0,1] and reduce C until in sRGB gamut.

---

## Tier 1 — Primitive Scale

Each hue family has a lightness scale from 50 (lightest) to 950 (darkest).  
Chroma is clamped so all values remain in sRGB gamut.

### Navy scale (H ≈ 237°, chroma ≈ 0.07)

```css
/* Primitive — Navy */
--color-navy-50:  oklch(0.97 0.01 237);
--color-navy-100: oklch(0.93 0.02 237);
--color-navy-200: oklch(0.84 0.04 237);
--color-navy-300: oklch(0.72 0.05 237);
--color-navy-400: oklch(0.58 0.06 237);
--color-navy-500: oklch(0.44 0.07 237);   /* ≈ brand-navy */
--color-navy-600: oklch(0.37 0.07 237);   /* original brand-navy */
--color-navy-700: oklch(0.29 0.06 237);
--color-navy-800: oklch(0.21 0.05 237);
--color-navy-900: oklch(0.14 0.04 237);
--color-navy-950: oklch(0.09 0.03 237);
```

### Cobalt scale (H ≈ 264°, chroma ≈ 0.20)

```css
/* Primitive — Cobalt */
--color-cobalt-50:  oklch(0.97 0.02 264);
--color-cobalt-100: oklch(0.92 0.05 264);
--color-cobalt-200: oklch(0.84 0.09 264);
--color-cobalt-300: oklch(0.74 0.13 264);
--color-cobalt-400: oklch(0.63 0.17 264);
--color-cobalt-500: oklch(0.52 0.20 264);   /* original brand-cobalt */
--color-cobalt-600: oklch(0.43 0.19 264);
--color-cobalt-700: oklch(0.34 0.16 264);
--color-cobalt-800: oklch(0.25 0.12 264);
--color-cobalt-900: oklch(0.17 0.08 264);
--color-cobalt-950: oklch(0.11 0.05 264);
```

### Green scale (H ≈ 155°, chroma ≈ 0.19)

```css
/* Primitive — Green */
--color-green-50:  oklch(0.97 0.03 155);
--color-green-100: oklch(0.93 0.06 155);
--color-green-200: oklch(0.87 0.10 155);
--color-green-300: oklch(0.82 0.14 155);
--color-green-400: oklch(0.75 0.19 155);   /* original brand-green */
--color-green-500: oklch(0.66 0.19 155);
--color-green-600: oklch(0.56 0.17 155);
--color-green-700: oklch(0.45 0.14 155);
--color-green-800: oklch(0.34 0.10 155);
--color-green-900: oklch(0.23 0.07 155);
--color-green-950: oklch(0.14 0.04 155);
```

### Neutral scale (H ≈ 240°, chroma ≈ 0.01 — near-gray)

```css
/* Primitive — Neutral */
--color-neutral-50:  oklch(0.98 0.00 240);
--color-neutral-100: oklch(0.96 0.00 240);
--color-neutral-200: oklch(0.92 0.01 240);
--color-neutral-300: oklch(0.84 0.01 240);
--color-neutral-400: oklch(0.72 0.01 240);
--color-neutral-500: oklch(0.60 0.01 240);
--color-neutral-600: oklch(0.48 0.01 240);
--color-neutral-700: oklch(0.36 0.01 240);
--color-neutral-800: oklch(0.25 0.01 240);
--color-neutral-900: oklch(0.15 0.01 240);
--color-neutral-950: oklch(0.09 0.00 240);
```

### Danger scale (H ≈ 25°, chroma ≈ 0.20 — red-orange)

```css
/* Primitive — Danger */
--color-danger-50:  oklch(0.97 0.02 25);
--color-danger-100: oklch(0.93 0.05 25);
--color-danger-200: oklch(0.86 0.09 25);
--color-danger-300: oklch(0.77 0.14 25);
--color-danger-400: oklch(0.66 0.19 25);
--color-danger-500: oklch(0.55 0.22 25);
--color-danger-600: oklch(0.44 0.20 25);
--color-danger-700: oklch(0.35 0.17 25);
--color-danger-800: oklch(0.26 0.13 25);
--color-danger-900: oklch(0.17 0.08 25);
--color-danger-950: oklch(0.11 0.05 25);
```

---

## Tier 2 — Semantic Tokens (Light Mode)

Maps primitive slots to roles. Bootstrap 5.3 custom properties are co-assigned here.

```css
/* === Semantic — Light Mode === */
:root {
  /* Surfaces */
  --color-surface-app:      var(--color-neutral-50);   /* app background */
  --color-surface-card:     oklch(1.00 0.00 0);        /* white card */
  --color-surface-overlay:  oklch(1.00 0.00 0 / 0.75); /* glass card */
  --color-surface-subtle:   var(--color-neutral-100);

  /* Text */
  --color-text-primary:     var(--color-navy-600);     /* body text */
  --color-text-secondary:   var(--color-neutral-500);
  --color-text-inverse:     oklch(1.00 0.00 0);        /* on dark bg */
  --color-text-danger:      var(--color-danger-600);

  /* Brand actions */
  --color-action-primary:       var(--color-cobalt-500);
  --color-action-primary-hover: var(--color-cobalt-600);
  --color-action-success:       var(--color-green-400);
  --color-action-success-hover: var(--color-green-500);

  /* Borders */
  --color-border-default:   var(--color-cobalt-300);   /* input border */
  --color-border-subtle:    var(--color-neutral-200);
  --color-border-focus:     var(--color-cobalt-500);

  /* Focus ring */
  --color-focus-ring:       oklch(0.52 0.20 264 / 0.35);

  /* Feedback */
  --color-feedback-danger-bg:   var(--color-danger-50);
  --color-feedback-danger-text: var(--color-danger-700);

  /* Bootstrap 5.3 mapping */
  --bs-primary:       var(--color-cobalt-500);
  --bs-success:       var(--color-green-400);
  --bs-danger:        var(--color-danger-500);
  --bs-body-bg:       var(--color-surface-app);
  --bs-body-color:    var(--color-text-primary);
  --bs-border-color:  var(--color-border-subtle);

  /* Legacy aliases — maintained for backward compatibility during migration */
  --brand-navy:         var(--color-navy-600);
  --brand-cobalt:       var(--color-cobalt-500);
  --brand-green:        var(--color-green-400);
  --brand-white:        oklch(1.00 0.00 0);
  --brand-bg-gray:      var(--color-surface-app);
  --brand-border-blue:  var(--color-border-default);
  --brand-green-hover:  var(--color-action-success-hover);
  --brand-cobalt-hover: var(--color-action-primary-hover);
  --brand-navy-dark:    var(--color-navy-700);
}
```

### Dark Mode Semantic Tokens (defined; not yet applied)

```css
/* === Semantic — Dark Mode (future) === */
@media (prefers-color-scheme: dark) {
  :root {
    --color-surface-app:      var(--color-navy-950);
    --color-surface-card:     var(--color-navy-900);
    --color-surface-overlay:  oklch(0.14 0.04 237 / 0.85);
    --color-surface-subtle:   var(--color-navy-800);

    --color-text-primary:     var(--color-neutral-100);
    --color-text-secondary:   var(--color-neutral-400);
    --color-text-inverse:     var(--color-navy-950);
    --color-text-danger:      var(--color-danger-300);

    --color-action-primary:       var(--color-cobalt-300);
    --color-action-primary-hover: var(--color-cobalt-200);
    --color-action-success:       var(--color-green-300);
    --color-action-success-hover: var(--color-green-200);

    --color-border-default:   var(--color-cobalt-700);
    --color-border-subtle:    var(--color-navy-700);
    --color-border-focus:     var(--color-cobalt-300);
    --color-focus-ring:       oklch(0.74 0.13 264 / 0.40);

    --color-feedback-danger-bg:   var(--color-danger-950);
    --color-feedback-danger-text: var(--color-danger-200);

    --bs-primary:      var(--color-cobalt-300);
    --bs-success:      var(--color-green-300);
    --bs-danger:       var(--color-danger-400);
    --bs-body-bg:      var(--color-surface-app);
    --bs-body-color:   var(--color-text-primary);
    --bs-border-color: var(--color-border-subtle);
  }
}
```

---

## Tier 3 — Component-Scoped Tokens

```css
/* Auth form */
--auth-input-height:     48px;
--auth-input-radius:     10px;
--auth-input-border:     1.5px solid var(--color-border-default);
--auth-input-bg:         oklch(1.00 0.00 0 / 0.60);
--auth-btn-radius:       10px;
--auth-card-radius:      16px;
--auth-card-blur:        12px;

/* Modal / Dialog */
--dialog-radius:         16px;
--dialog-header-height:  4rem;
--dialog-body-padding:   1.5rem;

/* Step wizard */
--wizard-dot-size:       14px;
--wizard-dot-active-bg:  var(--color-action-primary);
--wizard-dot-done-bg:    var(--color-action-success);
--wizard-dot-idle-bg:    var(--color-neutral-300);
```

---

## DTCG JSON — Primitive Samples

```json
{
  "color": {
    "cobalt": {
      "500": {
        "$value": "oklch(0.52 0.20 264)",
        "$type": "color",
        "$description": "Primary action blue — brand-cobalt"
      },
      "600": {
        "$value": "oklch(0.43 0.19 264)",
        "$type": "color",
        "$description": "Cobalt hover state"
      }
    },
    "action": {
      "primary": {
        "$value": "{color.cobalt.500}",
        "$type": "color"
      },
      "primary-hover": {
        "$value": "{color.cobalt.600}",
        "$type": "color"
      }
    }
  }
}
```

---

## Forced-Colors & High-Contrast Notes

- Under `forced-colors: active`, CSS custom properties are overridden by the system palette.
  Never rely on custom tokens alone for focus indicators — pair with `outline` or `border` that
  degrades gracefully (use `ButtonText`, `Highlight`, `HighlightText` as fallbacks in
  `@media (forced-colors: active)` blocks).
- Under `prefers-contrast: more`, increase border widths from 1.5 px to 2 px and
  swap surface-overlay for solid surface-card.
