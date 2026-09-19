# [FEATURE-ID] — Implementation Plan: [Feature Title]

> **Status**: `DRAFT` | `READY_FOR_EXECUTION` | `IN_PROGRESS` | `COMPLETED`  
> **Requirements**: `[requirements.md](requirements.md)` (Status: `APPROVED`)  
> **Design**: `[design.md](design.md)` (Status: `APPROVED`)  
> **Created**: YYYY-MM-DD  
> **Author**: AI Planning Agent / [Author Name]

---

## 1. Definition of Ready (DoR) Checklist

Before decomposing into tasks or writing code, all items must be checked:
- [ ] Requirements document exists and is marked `APPROVED` (Gate 1 passed).
- [ ] Design document exists and is marked `APPROVED` (Gate 2 passed).
- [ ] Governing ADRs identified and any new ADRs marked `Accepted`.
- [ ] Scope and acceptance criteria are disambiguated.
- [ ] Test plan template drafted.

---

## 2. Technical Scope & Change Inventory

### 2.1 Database & Schema (`appfleet-nexus-data`)
- **New Tables / Entities**:
- **Modified Tables / Columns**:
- **EF Core Migrations**: (e.g. `dotnet ef migrations add AddFeatureX`)

### 2.2 Security & Multi-Tenancy (`appfleet-nexus-security`)
- **Policies / Permissions**:
- **Tenant Isolation Filters**:

### 2.3 API & Services (`appfleet-nexus-api`)
- **DTOs / Contracts**:
- **Services / Business Logic**:
- **Controllers / Endpoints**:

### 2.4 User Interface (`ui`)
- **Components / Views**:
- **State Stores / API Clients**:
- **Routing**:

---

## 3. Ordered Task Breakdown Summary

| Task ID | Task Title | Primary Components | Dependencies | Status |
| :--- | :--- | :--- | :--- | :--- |
| `TASK-001` | Entity models & EF configuration | `appfleet-nexus-data` | None | `READY` |
| `TASK-002` | Service interface & business logic | `appfleet-nexus-api` | `TASK-001` | `BLOCKED` |
| `TASK-003` | API Controller & DTO validation | `appfleet-nexus-api` | `TASK-002` | `BLOCKED` |
| `TASK-004` | UI Components & state integration | `ui` | `TASK-003` | `BLOCKED` |
| `TASK-005` | End-to-end integration & verification | `tests` | `TASK-004` | `BLOCKED` |

---

## 4. Risks, Migrations & Rollback Strategy

- **Migration Hazards**: Data loss risks or locking implications.
- **Rollback Procedure**: Steps to revert migrations or feature flags if verification fails.
