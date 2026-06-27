# Vehicle Inventory Management: User Features & Capabilities

This document outlines the features and capabilities available to users in the **Vehicle Inventory Management Module** of FleetNexus.

---

## 1. Core User Features

A user interacting with the Vehicle Inventory Management dashboard has access to the following capabilities:

### A. Browse and Inspect Fleet Inventory
* **High-Fidelity Grid/List**: View a comprehensive list of all fleet vehicles registered under the user's organization (Tenant).
* **Responsive Visuals**:
  * **Desktop**: Displayed in a compact, highly scannable data table.
  * **Mobile**: Automatically collapses into touch-friendly cards containing unit information, make, model, and plate number.
* **Status Badges**: Instantly scan vehicle availability using color-coded status badges:
  * **Active**: Operational and available (emerald green).
  * **Maintenance**: In the shop, out of service, or undergoing repair (amber).
  * **Inactive**: Decommissioned, sold, or long-term storage (cool gray).

### B. Search, Filter, and Sort
* **Universal Search**: Type into a single search input to instantly filter the list by **Unit Number**, **VIN**, **Make**, **Model**, **License Plate**, or **State**.
* **Status Filter Tabs**: Toggle between `All`, `Active`, `Maintenance`, and `Inactive` status categories.
* **Type Filter**: Narrow down the inventory to show only specific types (e.g. show only *Vans*, *Trucks*, or *Trailers*).

### C. Create (Add) New Vehicles
* **Guided Modal Form**: Add a vehicle using a clean input form.
* **Smart Defaults**: New vehicles default to `Active` status.
* **Inline Form Validation**:
  * **Unit Number**: Required. The application checks for duplication and blocks creation if another active vehicle in the tenant already uses this unit number.
  * **Year**: Must be a realistic four-digit year (e.g., between 1900 and next year).
  * **State**: Limited to valid 2-letter postal abbreviation.

### E. Update (Edit) Vehicle Profiles
* **Inline Editing**: Click the Edit icon on any vehicle to load its current profile details into the form.
* **Operational Status Transition**: Move vehicles dynamically between `Active`, `Maintenance`, and `Inactive`. Updating a vehicle's status instantly updates the main Home Dashboard KPI counters.

### F. Decommission (Soft Delete) Vehicles
* **Safe Deletion**: Deleting a vehicle hides it from all active inventory lists and search results.
* **Audit Compliance**: Rather than permanently purging the record, it performs a **soft-delete** (`is_deleted = true`). This preserves historical links for past fuel purchases, driver logs, and maintenance records.

---

## 2. Multi-Tenant Security & Isolation

* **Strict Tenancy Boundaries**: Users are strictly confined to their organization's data boundary. A user will **never** see or interact with another company's vehicles, enforced both by ASP.NET Core API authorization and Entity Framework global query filters.
* **Audit Logging**: Any creation, update, or soft-deletion of a vehicle automatically writes the user's ID and current timestamp (`created_by`, `created_date`, `modified_by`, `modified_date`) to the vehicle database record for administrative accountability.

---

## 3. Extensibility Design (Vehicles & Contacts)

The layout is built as a parent **Inventory Management Section** that splits into sub-modules:
1. **Vehicle Inventory** (Active)
2. **Contact Inventory** (Coming soon): Allows managing tenant customers, vendors, and driver profiles.
