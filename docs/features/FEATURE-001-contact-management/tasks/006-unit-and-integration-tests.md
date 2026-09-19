# TASK-006: Unit and Integration Test Suite

> **Feature**: `FEATURE-001` ([requirements.md](../requirements.md))  
> **Status**: `READY`  
> **Order**: 006  
> **Dependencies**: `TASK-004`, `TASK-005`  

---

## 1. Objective & Boundaries

Implement comprehensive SQLite in-memory integration test fixtures in `appfleet-nexus-api.Tests`:
- Create `ContactsControllerTests.cs` implementing test scenarios `TEST-001` through `TEST-007`, `TEST-009`, and `TEST-012` through `TEST-015`.
- Update `VehiclesControllerTests.cs` implementing test scenarios `TEST-008`, `TEST-010`, and `TEST-011`.
- Execute tests and record logs for `evidence.md`.

*Boundary*: Do NOT modify UI components in this task.

---

## 2. Requirements & Acceptance Criteria
- **Target Requirements**: `REQ-001` through `REQ-015`
- **Target Acceptance Criteria**: `AC-001` through `AC-015`
- **Design References**: [`design.md`](../design.md), [`test-plan.md`](../test-plan.md)

---

## 3. Files In-Scope & Expected Changes

| File Path | Operation | Nature of Change |
| :--- | :--- | :--- |
| `app-fleet-nexus-net/api/appfleet-nexus-api.Tests/Controllers/ContactsControllerTests.cs` | Create | Implement 12+ test methods covering contact lifecycle, validation rules, multi-tenancy isolation, invariants, and delete guard. |
| `app-fleet-nexus-net/api/appfleet-nexus-api.Tests/Controllers/VehiclesControllerTests.cs` | Modify | Update vehicle test suite to test contact assignment requirements and vehicle contact point attachment. |

---

## 4. Required Tests & Evidence Proofs
- Command: `dotnet test app-fleet-nexus-net/api/appfleet-nexus-api.Tests/appfleet-nexus-api.Tests.csproj`
- Expected: 100% tests pass.

---

## 5. Constraints & Non-Negotiables
- Tests must use SQLite in-memory with `TestTenantContextAccessor` to test multi-tenancy isolation.
- Must verify tenant separation by setting up records in Tenant A and querying with Tenant B accessor context.

---

## 6. Definition of Done (DoD) Checklist
- [ ] `ContactsControllerTests.cs` and updated `VehiclesControllerTests.cs` execute without failures.
- [ ] All assertions in `test-plan.md` are covered.
- [ ] Test output captured for `evidence.md`.
- [ ] Task status updated to `VERIFIED`.
