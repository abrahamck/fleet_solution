# FEATURE-002 — Structured Code Review: Vehicle Create Validation Hotfix

> **Feature Reference**: [`requirements.md`](requirements.md) · [`evidence.md`](evidence.md)  
> **Review Date**: 2026-10-18  
> **Reviewer**: AI Review Agent (`code-review` skill)  
> **Branch**: `feature/FEATURE-002-vehicle-person-assignment` (base `62b538f`)  
> **Review Outcome**: `CHANGES_REQUESTED` (no Class C/D blockers)

---

## 1. Change Summary

**Trigger**: Creating a vehicle with an assigned contact and garage address failed with HTTP 400. The UI sent `"vehicleUnitNumber": null`. Because NRT (`<Nullable>enable</Nullable>`) is on, MVC treats non-nullable `string` DTO properties as implicitly `[Required]`, so model validation rejected the request before the action ran.

| File | Change |
| :--- | :--- |
| `api/appfleet-nexus-api/Models/ContactDtos.cs` | `VehicleContactAssignmentDto.VehicleUnitNumber` / `ContactFullName` → `string?` (display-only fields) |
| `ui/appfleet-nexus-ui/Components/VehicleDialog.razor` | Backfill assignment unit #, US state normalization, garage address completeness + 2-letter state validation, trim inputs, clear addresses when Line 1 empty |
| `ui/appfleet-nexus-ui/Pages/Inventory.razor` | Show ProblemDetails `errors` / `title` in the vehicle modal |
| `api/appfleet-nexus-api.Tests/Controllers/VehiclesControllerTests.cs` | New test `CreateVehicle_WithAssignedContactAndGarageAddress_NullVehicleUnitNumber_Succeeds` |

## 2. Verification Run

| Command | Result |
| :--- | :--- |
| `dotnet test app-fleet-nexus-net\api\appfleet-nexus-api.Tests\appfleet-nexus-api.Tests.csproj` | Passed 28/28, 0 skipped |
| `dotnet build app-fleet-nexus-net\ui\appfleet-nexus-ui\appfleet-nexus-ui.csproj` | Build succeeded: 0 warnings, 0 errors |

---

## 3. Findings

| ID | Severity | Status | Finding | Recommended Action |
| :--- | :--- | :--- | :--- | :--- |
| `F-1` | High | `OPEN` | The new test calls `controller.CreateVehicle()` directly, which skips model binding and `[ApiController]` implicit-required validation. It would pass even if the DTO fix were reverted, so it doesn't prove the regression is fixed. | Add a pipeline-level test (`WebApplicationFactory<Program>`) that POSTs raw JSON with `"vehicleUnitNumber": null`, or run `ObjectModelValidator` against the DTO. |
| `F-2` | Medium | `OPEN` | Both state inputs have `maxlength="2"`, so the full state-name map (`StateNameToAbbr`) can't be reached from the UI. It only affects old data loaded during an edit. | Remove the map (use `Trim().ToUpperInvariant()` like `ContactDialog`) **or** replace the inputs with a shared US-state `<select>`. |
| `F-3` | Medium | `OPEN` | `Vehicle.LicenseState` is normalized but its length is never checked on the client. A value that isn't 2 letters still goes to the API and comes back as a 400. | Add the same 2-letter guard used for Garage State. |
| `F-4` | Medium | `NEEDS_DECISION` | When Address Line 1 is empty, `ContactAddresses` is now set to an empty list. `UpdateVehicle` deletes and re-creates addresses, so on edit this removes **all** vehicle addresses, including ones that aren't Garage. | Confirm intent. Prefer replacing only the `"Garage"` entry. |
| `F-5` | Low | `OPEN` | `Save()` changes `Vehicle` (assignment unit #, `LicenseState`) before the garage validations, which can `return` early. | Run all validations first, then normalize and apply. |
| `F-6` | Low | `OPEN` | The UI unit-number backfill isn't needed (the server ignores the field and now accepts null). It also only fills empty values, so it goes stale if Unit # is edited after a contact is assigned. | Always overwrite it in `Save()`, or remove the backfill. |
| `F-7` | Low | `OPEN` | The ProblemDetails parsing block is now duplicated in `Inventory.razor` (vehicle and contact save). `VehicleDialog`, `ReassignModal` and `ContactDialog` parse only `message`. | Move it into a shared helper (e.g. `ApiErrorParser.Extract(json, fallback)`) and join messages with a separator so they stay readable. |
| `N-1` | Nit | `OPEN` | The test file has an extra blank line at EOF. `VehicleDialog.razor` uses LF line endings, and git warns they will become CRLF. | Normalize line endings via `.gitattributes`. |
| `N-2` | Info (existing code) | `BACKLOG` | `CreateVehicle` forces the first `Driver` to be primary without clearing `IsPrimary` on other entries. This could break INV-002 (one primary driver) if a `ResponsibleContact` marked primary comes first. Not introduced by this change. | Track as a backlog item. Normalize the primary flag server-side. |

---

## 4. Structured Review Report

```text
========================================
FEATURE-002 STRUCTURED REVIEW REPORT (Vehicle Create Validation Hotfix)
========================================

1. Requirements Compliance
--------------------------
AC-001  SUPPORTED   Fix unblocks Create Vehicle + Contact; the new test does
                    not run model binding (F-1), so this is not a true regression proof
AC-008  UNCHANGED   (garage address is optional; no AC impact)
Scope:  No scope creep. Bug fix + UX hardening only.

2. Architecture & ADR Compliance
--------------------------------
ADR-003/004 (Multi-Tenancy)  COMPLIANT  (no entity / query-filter changes)
ADR-014 (Soft Delete/Audit)  COMPLIANT  (no entity changes)
New dependencies             NONE

3. Deviations & Contradictions
------------------------------
DEV-002 Class A: DTO display-only fields made nullable to avoid implicit
                 [Required] under NRT (auto-reconciled; UI model already string?)
DEV-003 Class B: Clearing Address Line 1 now deletes vehicle addresses on
                 update (F-4) — reviewer acknowledgement needed
Contradictions:  evidence.md was out of date (27/27 tests, no hotfix entry)
                 — reconciled in this commit.

4. Recommendation
-----------------
STATUS: CHANGES_REQUESTED (non-blocking for Class C/D)
  Must:   F-1 (real pipeline-level regression test)
  Should: F-2, F-3, F-4 decision
  Could:  F-5, F-6, F-7, N-1
```
