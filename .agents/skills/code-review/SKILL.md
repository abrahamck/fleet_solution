---
name: code-review
description: >-
  Use for the Review Agent after implementation and testing are complete. Performs structured review
  against requirements, ADRs, test proofs, contradiction detection, and deviation classification,
  preparing the feature for Human Final Review and Merge.
---

# Review Agent & Structured Verification Skill

This skill governs the Structured Verification, Contradiction Detection, Deviation Management, and Pre-Merge Review workflow for FleetNexus features.

---

## 1. Operating Rules & Boundaries

1. **Evidence-Driven Review**: Never approve a feature with "Looks good to me". Review must inspect actual test outputs, git diffs, and evidence matrix entries.
2. **Contradiction Detection**: Actively search for discrepancies between `requirements.md`, `design.md`, governing ADRs, and the implemented code.
3. **Strict Deviation Classification**:
   - **Class A (Implementation Detail)**: Auto-reconcile in artifact notes.
   - **Class B (Design Adjustment)**: Reconcile in implementation record, flag for reviewer note.
   - **Class C (Requirement Deviation)**: **BLOCK** — Human decision required.
   - **Class D (Architectural Deviation)**: **BLOCK** — Architecture review / ADR update required.
4. **Artifact Production**: Produce a formal Structured Review Report and update `status.md` to `REVIEW` $\rightarrow$ `APPROVED` (upon human sign-off).

---

## 2. Step-by-Step Review Workflow

```mermaid
flowchart TD
    A[Tasks VERIFIED & Evidence Captured] --> B[Step 1: Check Requirements vs Code Diff]
    B --> C[Step 2: Check Governing ADR Compliance]
    C --> D[Step 3: Verify Test Results in evidence.md]
    D --> E[Step 4: Classify Deviations & Detect Contradictions]
    E --> F{Any Blockers (Class C/D or Contradiction)?}
    F -->|Yes| G[Step 5: Output RECONCILIATION_REQUIRED & Halt]
    F -->|No| H[Step 6: Generate Structured Review Report for Human]
```

### Step 1: Requirements & Scope Verification
Inspect `git diff` against `docs/features/FEATURE-XXX/requirements.md`:
- Are all Acceptance Criteria (`AC-xxx`) accounted for?
- Did any unauthorized scope creep occur?

### Step 2: Architecture & ADR Verification
Verify compliance with active ADRs in `docs/ADR/README.md`:
- Multi-tenancy discriminator (`TenantId`) present and filtered on all new entity types (`ADR-003`, `ADR-004`).
- Soft delete (`ISoftDeletable`) and audit timestamps (`IAuditableEntity`) present (`ADR-014`).
- No rogue dependencies introduced without ADR approval.

### Step 3: Evidence Matrix Verification
Inspect `docs/features/FEATURE-XXX/evidence.md`:
- Are all rows marked `VERIFIED`?
- Are test command runs reproducible and passing without failures or skipped assertions?

### Step 4: Output Structured Review Report
Emit the standardized review report block:

```text
========================================
FEATURE-XXX STRUCTURED REVIEW REPORT
========================================

1. Requirements Compliance
--------------------------
AC-001  VERIFIED (Unit Test: Should_Create_Entity)
AC-002  VERIFIED (Unit Test: Should_Validate_Payload)
AC-003  VERIFIED (Isolation Filter Check)

2. Architecture & ADR Compliance
--------------------------------
ADR-003 (Multi-Tenancy)     COMPLIANT
ADR-004 (Tenant Isolation)  COMPLIANT
ADR-014 (Soft Delete)       COMPLIANT

3. Deviations & Contradictions
------------------------------
DEV-001 Class A: Added private helper method (Auto-reconciled)
Contradictions: NONE DETECTED

4. Recommendation
-----------------
STATUS: READY_FOR_HUMAN_APPROVAL
```

### Step 5: Push Branch & Submit Pull Request (PR)
Once pre-merge validation passes:
1. **Push Branch to Remote**:
   ```bash
   git push -u origin feature/FEATURE-XXX-[description]
   ```
2. **Create Pull Request**:
   - **Target**: `main`
   - **Title**: `feat([scope]): FEATURE-XXX [Feature Title]`
   - **Description**: Include the complete **Structured Review Report** generated in Step 4.
   - Command (if GitHub CLI is installed):
     ```bash
     gh pr create --base main --head feature/FEATURE-XXX-[description] --title "feat(scope): FEATURE-XXX Title" --body-file docs/features/FEATURE-XXX/evidence.md
     ```

### Step 6: Post-Merge Cleanup
Once the human approves and merges the Pull Request into `main`:
1. Mark `status.md` as `MERGED`.
2. Clean up the isolated worktree sandbox:
   ```powershell
   .\scripts\worktree-helper.ps1 remove -FeatureId "FEATURE-XXX"
   ```
