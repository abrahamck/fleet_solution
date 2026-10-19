# TASK-006: Integration Test Suite Execution & Evidence Logging

> **Feature**: `FEATURE-002` ([requirements.md](../requirements.md))  
> **Status**: `VERIFIED`  
> **Order**: 006  
> **Dependencies**: TASK-001 through TASK-005  

---

## 1. Objective & Boundaries
Implement the automated test cases specified in `test-plan.md` across `VehiclesControllerTests.cs` and `ContactsControllerTests.cs`, execute the full test suite, verify the UI compilation, and log all run outcomes in `evidence.md`.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: All requirements and invariants (`REQ-001` through `REQ-013`, `INV-001` through `INV-009`)
- **Target Acceptance Criteria**: `AC-001` through `AC-013`, `TEST-001` through `TEST-015`
- **Design References**: [`test-plan.md`](../test-plan.md), [`evidence.md`](../evidence.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-api.Tests/Controllers/VehiclesControllerTests.cs` | Modify | Add automated test methods for `reassign` endpoint (`ReplacePrimary`, `AddSecondary`, duplicate upsert, cross-tenant isolation) and diff-based sync (`AssignedDate` preservation). |
| `app-fleet-nexus-net/api/appfleet-nexus-api.Tests/Controllers/ContactsControllerTests.cs` | Modify | Add automated test methods for contact diff-based assignment sync (`AssignedDate` preservation). |
| `docs/features/FEATURE-002-vehicle-person-assignment/evidence.md` | Modify | Update all row statuses to `VERIFIED`, capture real test execution logs, and document any non-functional observations. |

---

## 4. Required Tests & Evidence Proofs
- Full test run command:
  ```powershell
  dotnet test c:\Learn\fleet_solution\app-fleet-nexus-net\api\appfleet-nexus-api.Tests\appfleet-nexus-api.Tests.csproj
  ```
- All tests pass (0 failures).
- UI compilation verification:
  ```powershell
  dotnet build c:\Learn\fleet_solution\app-fleet-nexus-net\ui\appfleet-nexus-ui\appfleet-nexus-ui.csproj
  ```
- Build succeeded (0 errors).

---

## 5. Constraints & Non-Negotiables
- Every test must use SQLite in-memory database with tenant context accessor matching repository testing conventions.
- No flaky or timing-dependent assertions.

---

## 6. Definition of Done (DoD) Checklist
- [x] All specified test methods implemented in test fixtures.
- [x] 100% of test suite passing.
- [x] `evidence.md` updated with exact terminal execution output and status `VERIFIED`.
- [x] Task status updated to `VERIFIED`.
