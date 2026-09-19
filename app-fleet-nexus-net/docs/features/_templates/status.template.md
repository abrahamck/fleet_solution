# [FEATURE-ID] — Feature Lifecycle Status

> **Feature Name**: [Feature Title]  
> **Complexity Tier**: `LOW` | `STANDARD` | `SIGNIFICANT`  
> **Current State**: `INTAKE`  
> **Last State Transition**: YYYY-MM-DD  
> **Assigned Worktree**: `.worktrees/[FEATURE-ID]`

---

## State Machine Progression

- [ ] `INTAKE` (Problem statement defined, complexity classified)
- [ ] `DISCOVERY` (`requirements.md` drafted)
- [ ] `REQUIREMENTS_REVIEW` (Human Gate 1 - Awaiting user sign-off)
- [ ] `ARCHITECTURE_REVIEW` (`design.md` drafted, ADRs checked / created, Human Gate 2 sign-off)
- [ ] `PLANNING` (`implementation-plan.md` drafted, DoR validated)
- [ ] `TEST_DESIGN` (`test-plan.md` drafted independently)
- [ ] `READY_FOR_IMPLEMENTATION` (Tasks decomposed in `tasks/*.md`, worktree created)
- [ ] `IMPLEMENTATION` (Developer agent executing atomic tasks)
- [ ] `VERIFICATION` (Evidence collected in `evidence.md`, all tests passing)
- [ ] `RECONCILIATION` (Deviations classified, contradictions resolved)
- [ ] `REVIEW` (Review agent assessment complete, report produced)
- [ ] `APPROVED` (Human final sign-off completed)
- [ ] `MERGED` (Changes merged into main repository branch)

---

## State Transition Audit Trail

| Date / Time | From State | To State | Triggered By | Notes |
| :--- | :--- | :--- | :--- | :--- |
| YYYY-MM-DD | `None` | `INTAKE` | User / Agent | Feature initiated |
