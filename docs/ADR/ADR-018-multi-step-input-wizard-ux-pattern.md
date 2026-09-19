# ADR-018: Multi-Step Input Wizard UX Pattern

* **Status**: Accepted
* **Date**: 2026-06-22

---

## Context & Problem Statement
Long multi-field forms on mobile devices introduce significant visual clutter, high cognitive load, and poor tap ergonomics, leading to form abandonment during registration and onboarding.

---

## Decision
Establish a standardized **Multi-Step Input Wizard pattern** across all data-collection workflows:
1. **Incremental Disclosure**: Break questionnaires into single-item or cohesive logical steps.
2. **Minimalist Dot Progress Tracking**: Horizontal unlabelled colored dots indicating progress without visual clutter.
3. **Unified Component Wrapper**: Standard title, subtitle label, single auto-focused input field, and inline validation warnings (`AuthInput.razor`).
4. **Progressive Step Validation**: Validate active step properties via DataAnnotations before permitting step advancement.
5. **Dynamic Navigation Controls**: Disabled "Previous" on Step 1, and "Next" transitioning into "Submit" on the final step.

---

## Alternatives Considered
* **Single Long Scrolling Form**: Presenting all inputs simultaneously on one page.
* **Accordion Controls**: Expanding/collapsing panels on a single page.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Optimized mobile ergonomics and reduced cognitive load.
  * Reusable pattern across Signup, Roadblock Waitlist, and future entity creation flows.
* **Risks & Trade-offs**: Increases the number of user click/tap interactions compared to a single dense form.
* **Migration Paths**: Not Available.
