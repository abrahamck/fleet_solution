---
name: implementation-planning
description: >-
  Use after requirements and architecture are approved to evaluate the Definition of Ready (DoR),
  determine exact file/service modifications, map dependencies, and draft the formal implementation-plan.md.
---

# Implementation Planning Skill

This skill governs the Technical Scoping, Definition of Ready (DoR) verification, and Implementation Planning workflow for FleetNexus features.

---

## 1. Operating Rules & Boundaries

1. **Preconditions (DoR Enforcement)**: Never write an implementation plan until both `requirements.md` (Gate 1) and `design.md` (Gate 2) are formally approved by the human.
2. **Focus on "What Must Change"**: Identify exact database entities, API endpoints, DTO contracts, UI components, and test harnesses. Do not write full method bodies in the plan.
3. **Traceability Anchor**: Ensure every change connects back to an approved requirement ID (`REQ-xxx`) or Acceptance Criteria ID (`AC-xxx`).
4. **Artifact Production**: Write durable artifacts to `docs/features/FEATURE-XXX/implementation-plan.md` and update `status.md` to `PLANNING`.

---

## 2. Step-by-Step Implementation Planning Workflow

```mermaid
flowchart TD
    A[Approved requirements.md + design.md] --> B[Step 1: Validate Definition of Ready (DoR)]
    B --> C[Step 2: Inventory Technical Changes by Layer]
    C --> D[Step 3: Map Database Migrations & Seed Strategy]
    D --> E[Step 4: Draft implementation-plan.md from Template]
    E --> F[Step 5: Hand off to Test Design & Task Decomposition]
```

### Step 1: Verify Definition of Ready (DoR)
Ensure all checkboxes in the DoR checklist pass:
- [x] `requirements.md` is approved with all open questions (`Q-xxx`) answered.
- [x] `design.md` is approved and all governing ADRs are resolved (`Accepted`).
- [x] Scope boundary is unambiguous.

### Step 2: Layer-by-Layer Change Inventory
Map out the concrete files to create or modify across all affected projects:

1. **Data Layer (`app-fleet-nexus-net/api/appfleet-nexus-data/`)**:
   - Entities (`Entities/*.cs`), DbContext configurations (`Configurations/*.cs`), DbSets in `ApplicationDbContext.cs`.
   - Tenant isolation query filters (`e.TenantId == CurrentTenantId`).
   - Audit columns and soft delete interfaces (`IAuditableEntity`, `ISoftDeletable`).
   - Migration script generation (`dotnet ef migrations add ...`).

2. **Security Layer (`app-fleet-nexus-net/api/appfleet-nexus-security/`)**:
   - Authorization policies, custom requirements, claim validation.

3. **API & Business Logic Layer (`app-fleet-nexus-net/api/appfleet-nexus-api/`)**:
   - DTOs & request validation (e.g. `FluentValidation`).
   - Service interfaces and implementations (`Services/*.cs`).
   - Controllers (`Controllers/*.cs`) with OpenAPI annotations and response status codes.

4. **UI Layer (`app-fleet-nexus-net/ui/`)**:
   - Component views, input forms, validation hooks, state management, and routing.

5. **Test Harness (`app-fleet-nexus-net/api/appfleet-nexus-api.Tests/`)**:
   - Unit test fixtures, database mocks, controller tests.

### Step 3: Author `implementation-plan.md`
Copy `docs/features/_templates/implementation-plan.template.md` to `docs/features/FEATURE-XXX/implementation-plan.md`.
Fill out:
- Scope and file inventory.
- High-level task table with dependency ordering.
- Migration and rollback strategy.
