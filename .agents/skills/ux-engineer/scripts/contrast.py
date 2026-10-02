#!/usr/bin/env python3
"""
contrast.py — WCAG 2.2 contrast ratio calculator for FleetNexus UX Engineer skill.

Inputs:  hex (#RRGGBB, #RRGGBBAA), rgb(r g b [/ a]), oklch(L C H)
Outputs: relative luminance for each color, contrast ratio,
         SC 1.4.3 pass/fail (normal text ≥ 4.5:1, large text ≥ 3:1),
         SC 1.4.11 pass/fail (UI components / graphical objects ≥ 3:1).

Alpha compositing: if a foreground has alpha < 1.0, composite it over
the background before computing luminance. A second positional argument
provides the background; default white (#FFFFFF).

Usage:
    python contrast.py <foreground> [background]

Examples:
    python contrast.py "#1A73E8" "#FFFFFF"
    python contrast.py "#2C4B64" "#F4F6F8"
    python contrast.py "oklch(0.48 0.20 264)" "#F4F6F8"
    python contrast.py "rgb(26 115 232 / 0.9)" "#FFFFFF" "#F4F6F8"
"""

import sys
import re
import math


# ---------------------------------------------------------------------------
# Parsing helpers
# ---------------------------------------------------------------------------

def parse_color(s: str) -> tuple[float, float, float, float]:
    """Return (R, G, B, A) in [0,1] range from a color string."""
    s = s.strip()

    # hex
    if s.startswith("#"):
        h = s.lstrip("#")
        if len(h) == 3:
            h = "".join(c * 2 for c in h)
        if len(h) == 6:
            r, g, b = int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16)
            return r / 255, g / 255, b / 255, 1.0
        if len(h) == 8:
            r, g, b, a = int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), int(h[6:8], 16)
            return r / 255, g / 255, b / 255, a / 255
        raise ValueError(f"Unrecognised hex: {s}")

    # rgb / rgba  — CSS4 syntax: rgb(R G B) or rgb(R G B / A)
    m = re.match(r"rgba?\(\s*([^)]+)\)", s, re.I)
    if m:
        parts = re.split(r"[,\s/]+", m.group(1).strip())
        parts = [p for p in parts if p]
        vals = [float(p.rstrip("%")) / (100 if p.endswith("%") else 1) for p in parts]
        if len(vals) == 3:
            vals.append(1.0)
        r, g, b, a = vals
        # un-scale if given in 0-255 range
        if r > 1 or g > 1 or b > 1:
            r, g, b = r / 255, g / 255, b / 255
        return r, g, b, a

    # oklch(L C H)  — L in [0,1] or percentage
    m = re.match(r"oklch\(\s*([^)]+)\)", s, re.I)
    if m:
        parts = re.split(r"[,\s/]+", m.group(1).strip())
        parts = [p for p in parts if p]
        L = float(parts[0].rstrip("%")) / (100 if parts[0].endswith("%") else 1)
        C = float(parts[1])
        H = float(parts[2])
        return oklch_to_linear_srgb(L, C, H) + (1.0,)

    raise ValueError(f"Cannot parse color: {s!r}")


# ---------------------------------------------------------------------------
# Color space math
# ---------------------------------------------------------------------------

def oklch_to_linear_srgb(L: float, C: float, H: float) -> tuple[float, float, float]:
    """OKLCH -> OKLab -> linear sRGB, with gamut clamping."""
    h_rad = math.radians(H)
    a = C * math.cos(h_rad)
    b = C * math.sin(h_rad)

    # OKLab -> LMS (cube root space)
    l_ = L + 0.3963377774 * a + 0.2158037573 * b
    m_ = L - 0.1055613458 * a - 0.0638541728 * b
    s_ = L - 0.0894841775 * a - 1.2914855480 * b

    l = l_ ** 3
    m = m_ ** 3
    s = s_ ** 3

    # LMS -> linear sRGB
    r =  4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s
    g = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s
    b = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s

    # Gamut clamp
    r = max(0.0, min(1.0, r))
    g = max(0.0, min(1.0, g))
    b = max(0.0, min(1.0, b))

    return r, g, b


def linear_to_srgb(c: float) -> float:
    if c <= 0.0031308:
        return 12.92 * c
    return 1.055 * (c ** (1 / 2.4)) - 0.055


def srgb_to_linear(c: float) -> float:
    if c <= 0.04045:
        return c / 12.92
    return ((c + 0.055) / 1.055) ** 2.4


def relative_luminance(r: float, g: float, b: float) -> float:
    """WCAG relative luminance from sRGB [0,1] values."""
    rl = srgb_to_linear(r)
    gl = srgb_to_linear(g)
    bl = srgb_to_linear(b)
    return 0.2126 * rl + 0.7152 * gl + 0.0722 * bl


def contrast_ratio(L1: float, L2: float) -> float:
    lighter = max(L1, L2)
    darker = min(L1, L2)
    return (lighter + 0.05) / (darker + 0.05)


# ---------------------------------------------------------------------------
# Alpha compositing
# ---------------------------------------------------------------------------

def composite(fg: tuple, bg: tuple) -> tuple[float, float, float]:
    """Composite fg (R,G,B,A) over bg (R,G,B,A), return opaque (R,G,B)."""
    fr, fg_, fb, fa = fg
    br, bg_, bb, ba = bg
    # Porter-Duff over
    out_a = fa + ba * (1 - fa)
    if out_a == 0:
        return 0.0, 0.0, 0.0
    r = (fr * fa + br * ba * (1 - fa)) / out_a
    g = (fg_ * fa + bg_ * ba * (1 - fa)) / out_a
    b = (fb * fa + bb * ba * (1 - fa)) / out_a
    return r, g, b


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    fg_str = sys.argv[1]
    bg_str = sys.argv[2] if len(sys.argv) > 2 else "#FFFFFF"

    try:
        fg = parse_color(fg_str)
        bg = parse_color(bg_str)
    except ValueError as e:
        print(f"ERROR: {e}", file=sys.stderr)
        sys.exit(2)

    # Composite if fg is translucent
    if fg[3] < 1.0:
        r_eff, g_eff, b_eff = composite(fg, bg)
    else:
        r_eff, g_eff, b_eff = fg[0], fg[1], fg[2]

    L_fg = relative_luminance(r_eff, g_eff, b_eff)
    L_bg = relative_luminance(bg[0], bg[1], bg[2])
    ratio = contrast_ratio(L_fg, L_bg)

    # sRGB display values for reporting
    fg_disp = (linear_to_srgb(srgb_to_linear(r_eff)),
               linear_to_srgb(srgb_to_linear(g_eff)),
               linear_to_srgb(srgb_to_linear(b_eff)))

    print("=" * 52)
    print(f"  Foreground : {fg_str}")
    if fg[3] < 1.0:
        print(f"  Background : {bg_str}  (composited)")
    else:
        print(f"  Background : {bg_str}")
    print("-" * 52)
    print(f"  Lum (fg)   : {L_fg:.6f}")
    print(f"  Lum (bg)   : {L_bg:.6f}")
    print(f"  Ratio      : {ratio:.2f}:1")
    print("-" * 52)

    def pf(test: bool) -> str:
        return "PASS" if test else "FAIL"

    print(f"  SC 1.4.3   normal text  (>= 4.5:1) : {pf(ratio >= 4.5)}  [{ratio:.2f}:1]")
    print(f"  SC 1.4.3   large text   (>= 3.0:1) : {pf(ratio >= 3.0)}  [{ratio:.2f}:1]")
    print(f"  SC 1.4.11  UI component (>= 3.0:1) : {pf(ratio >= 3.0)}  [{ratio:.2f}:1]")
    print("=" * 52)

    # Exit code: 0 = all pass for normal text, 1 = any failure
    sys.exit(0 if ratio >= 4.5 else 1)


if __name__ == "__main__":
    main()
