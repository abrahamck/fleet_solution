# [FEATURE-ID] — Verification & Evidence Log: [Feature Title]

> **Feature Reference**: `[requirements.md](requirements.md)`  
> **Last Updated**: YYYY-MM-DD  
> **Overall Verification Status**: `UNVERIFIED` | `PARTIALLY_VERIFIED` | `VERIFIED` | `CONFLICT`

---

## 1. Evidence Matrix

| Criteria ID | Requirement | Implementation Reference | Test Reference | Status | Evidence Details / Run Result |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `AC-001` | `REQ-001` | `src/.../MyService.cs:42` | `tests/.../MyTests.cs:18` | `VERIFIED` | `Should_Create_Entity` passed in 12ms |
| `AC-002` | `REQ-002` | `src/.../MyValidator.cs:15` | `tests/.../MyTests.cs:35` | `VERIFIED` | `Should_Return_400_When_Field_Missing` passed |
| `AC-003` | `REQ-003` | `src/.../MyDbContext.cs:88` | `tests/.../MyTests.cs:52` | `VERIFIED` | Cross-tenant isolation filter verified |

### Evidence Status Definitions:
- `VERIFIED`: Automated test passed with reproducible command and output.
- `SUPPORTED`: Code inspection confirms compliance, but automated test is impractical/deferred.
- `UNVERIFIED`: Claimed complete but missing verification proof.
- `CONFLICT`: Implementation contradicts requirement or ADR.
- `BLOCKED`: Dependency or decision blocks verification.

---

## 2. Test Execution Log Output

```text
=== Automated Test Run Log ===
Timestamp: YYYY-MM-DD HH:MM:SS
Command: dotnet test --filter FullyQualifiedName~FeatureTests
Passed! - Failed: 0, Passed: 6, Skipped: 0, Total: 6, Duration: 1.2s
```

---

## 3. Deviations & Reconciliation Log

| Deviation ID | Class | Description | Impact | Action / Decision |
| :--- | :--- | :--- | :--- | :--- |
| `DEV-001` | `Class A` | Added helper extension method for validation | Low (internal) | Auto-reconciled in implementation |
