# [TASK-ID]: [Task Short Title]

> **Feature**: `[FEATURE-ID]` (`[requirements.md](../requirements.md)`)  
> **Status**: `READY` | `IN_PROGRESS` | `VERIFIED` | `BLOCKED`  
> **Order**: [e.g. 001]  
> **Dependencies**: [e.g. None or TASK-001]

---

## 1. Objective & Boundaries
*Precisely state what this atomic task delivers and what it must NOT touch.*

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-xxx`
- **Target Acceptance Criteria**: `AC-xxx`
- **Design References**: `[design.md](../design.md)`, `ADR-xxx`

---

## 3. Files In-Scope & Expected Changes
| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `src/.../MyEntity.cs` | Create | Define entity properties and audit interfaces |
| `src/.../MyDbContext.cs` | Modify | Register DbSet and configuration filter |

---

## 4. Required Tests & Evidence Proofs
- Test cases to implement or run: `TEST-xxx`
- Expected verification evidence: Unit test passing logs in `evidence.md`.

---

## 5. Constraints & Non-Negotiables
- Must comply with tenant query filtering (`TenantId`).
- Must not introduce external dependencies without ADR approval.

---

## 6. Definition of Done (DoD) Checklist
- [ ] Code compiles without warnings or errors.
- [ ] Targeted tests pass.
- [ ] Evidence recorded in `evidence.md`.
- [ ] Task status updated to `VERIFIED`.
