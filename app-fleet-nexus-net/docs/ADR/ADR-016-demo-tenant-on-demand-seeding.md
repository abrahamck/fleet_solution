# ADR-016: On-Demand Demo Tenant & Vehicle Seeding

* **Status**: Accepted
* **Date**: 2026-06-22

---

## Context & Problem Statement
Developers, testers, and stakeholders require a predictable, rich demo environment populated with realistic commercial fleet data without needing manual database seeding scripts or setup steps.

---

## Decision
Implement an **on-demand demo account interceptor** in `AuthController` for the dedicated credential `testuser@demo.com`. Upon login, `EnsureDemoDataSetupAsync` ensures the user exists, provisions the tenant **"ABC Heating and Cooling"**, and seeds **8 realistic commercial fleet vehicles** (Chevrolet Express, Ford Transit, Ford F-150, Ram 1500 with varying statuses).

---

## Alternatives Considered
* **Not Available** (Manual database script execution or static migration seed data).

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Provides an instant, self-healing demo experience that works reliably even after database teardown or schema migrations.
  * Populates realistic metrics on the Home Dashboard KPI tiles.
* **Risks & Trade-offs**: Demo seeding logic is embedded within auth controller handling.
* **Migration Paths**: Not Available.
