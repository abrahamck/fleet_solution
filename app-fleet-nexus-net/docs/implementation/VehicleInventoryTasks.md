# Detailed Implementation Tasks: Vehicle Inventory Management

This document provides step-by-step task specifications for implementing the Vehicle Inventory Management Module in FleetNexus.

---

## Checklist & Progress Tracker

- [x] **Phase 1: Backend CRUD API Implementation**
  - [x] Create `VehiclesController.cs` in `appfleet-nexus-api/Controllers/`.
  - [x] Implement query and write actions with EF Core global filter integration.
  - [x] Implement server-side validation (uniqueness check on Unit Number per tenant).
  - [x] Implement cache invalidation (`dashboard_kpis_{tenantId}`) on write actions.
  - [x] Test API endpoints via Swagger or build verification.

- [x] **Phase 2: UI Shell Navigation & Models**
  - [x] Update `Layout/NavMenu.razor` to include the "Inventory" link for authorized users.
  - [x] Create `Models/VehicleModels.cs` in `appfleet-nexus-ui/Models/` containing client DTOs.
  - [x] Register Navigation links and route mappings.

- [x] **Phase 3: Unified Inventory Layout (`/inventory`)**
  - [x] Create `Pages/Inventory.razor` with the basic page route and layout structure.
  - [x] Implement Tab Selector: **Vehicles** (Active) and **Contacts** (Disabled/Mock).
  - [x] Build search & filter input bar (Text Search, Status filters, Type filters).

- [x] **Phase 4: Responsive Grid Layout & Scoped CSS**
  - [x] Design desktop table data-density view.
  - [x] Design mobile card-based responsive stacked views.
  - [x] Create `Pages/Inventory.razor.css` for custom badges, grid styling, and slide-in animations.

- [x] **Phase 5: CRUD Modal Form & Dialog Interactions**
  - [x] Add the Add/Edit Modal Dialog block in `Inventory.razor`.
  - [x] Create client validation using DataAnnotations.
  - [x] Connect Add, Edit, and Delete action handlers to the backend API.
  - [x] Add clear inline error messages and confirmation prompts for deletion.

- [x] **Phase 6: End-to-End Verification**
  - [x] Validate multi-tenancy boundaries (user can only see own tenant's vehicles).
  - [x] Validate cache eviction (adding/editing a vehicle updates home dashboard metrics).
  - [x] Verify mobile viewport responsiveness.

---

## Detailed Task Details

### 1. Backend Controller (`VehiclesController.cs`)
- File: `app-fleet-nexus-net/api/appfleet-nexus-api/Controllers/VehiclesController.cs`
- Routing: `[Authorize]`, `[ApiController]`, `[Route("api/vehicles")]`
- Dependency Injection:
  - Inject `FleetNexusDbContext dbContext`
  - Inject `ITenantContextAccessor tenantAccessor`
  - Inject `IMemoryCache cache`
- Actions:
  - **GetVehicles**: `[HttpGet]`
    - Returns all active vehicles: `await _dbContext.Vehicles.ToListAsync()`. (EF query filters filter by tenant automatically).
  - **GetVehicleById**: `[HttpGet("{id}")]`
    - Returns `await _dbContext.Vehicles.FindAsync(id)`. Returns `NotFound` if null.
  - **CreateVehicle**: `[HttpPost]`
    - Body Dto: contains unit number, VIN, make, model, year, license plate, state, type, status.
    - Check if unit number is already in use by active vehicles of this tenant:
      ```csharp
      var exists = await _dbContext.Vehicles.AnyAsync(v => v.UnitNumber.Equals(request.UnitNumber, StringComparison.OrdinalIgnoreCase));
      if (exists) return BadRequest(new { message = "A vehicle with this Unit Number already exists." });
      ```
      *(Note: query filters handle is_deleted and tenant_id check automatically)*.
    - Add vehicle, run `await _dbContext.SaveChangesAsync()`.
    - Evict cache: `_cache.Remove($"dashboard_kpis_{tenantId}");`.
  - **UpdateVehicle**: `[HttpPut("{id}")]`
    - Retrieve vehicle: `var vehicle = await _dbContext.Vehicles.FindAsync(id);`.
    - If null, return `NotFound`.
    - Check if the updated unit number belongs to another active vehicle:
      ```csharp
      var exists = await _dbContext.Vehicles.AnyAsync(v => v.Id != id && v.UnitNumber.Equals(request.UnitNumber, StringComparison.OrdinalIgnoreCase));
      ```
    - Map properties, save changes, and evict tenant dashboard cache.
  - **DeleteVehicle**: `[HttpDelete("{id}")]`
    - Retrieve vehicle, if null return `NotFound`.
    - Call `_dbContext.Vehicles.Remove(vehicle)` which soft-deletes the record inside `SaveChangesAsync`.
    - Evict cache.

### 2. UI Navigation & DTO Models
- **DTO Model File**: `app-fleet-nexus-net/ui/appfleet-nexus-ui/Models/VehicleModels.cs`
  ```csharp
  using System.ComponentModel.DataAnnotations;

  namespace appfleet_nexus_ui.Models;

  public class VehicleDto
  {
      public Guid Id { get; set; }
      
      [Required(ErrorMessage = "Unit Number is required.")]
      [StringLength(50, ErrorMessage = "Unit Number cannot exceed 50 characters.")]
      public string UnitNumber { get; set; } = string.Empty;
      
      public string? Vin { get; set; }
      public string? Make { get; set; }
      public string? Model { get; set; }
      
      [Range(1900, 2100, ErrorMessage = "Please enter a valid year.")]
      public int? Year { get; set; }
      
      public string? LicensePlate { get; set; }
      
      [StringLength(2, ErrorMessage = "State must be a 2-letter abbreviation.")]
      public string? LicenseState { get; set; }
      
      public string? Type { get; set; }
      public string Status { get; set; } = "Active";
  }
  ```

- **Layout Link**: `app-fleet-nexus-net/ui/appfleet-nexus-ui/Layout/NavMenu.razor`
  Inside `<AuthorizeView><Authorized>` section, insert:
  ```razor
  <div class="nav-item px-3">
      <NavLink class="nav-link" href="inventory">
          <span class="bi bi-collection-fill-nav-menu" aria-hidden="true"></span> Inventory
      </NavLink>
  </div>
  ```

### 3. UI Scoped Styles (`Inventory.razor.css`)
- File: `app-fleet-nexus-net/ui/appfleet-nexus-ui/Pages/Inventory.razor.css`
- Core Styles:
  - Badge designs: `.badge-active`, `.badge-maintenance`, `.badge-inactive`.
  - Responsive collapse:
    ```css
    @media (max-width: 768px) {
        .desktop-table { display: none; }
        .mobile-cards { display: block; }
        .mobile-bottom-sheet {
            position: fixed;
            bottom: 0;
            left: 0;
            width: 100%;
            border-radius: 20px 20px 0 0;
            transform: translateY(0);
            transition: transform 0.3s ease-out;
        }
    }
    ```

### 4. Page Logic & CRUD Modal (`Inventory.razor`)
- File: `app-fleet-nexus-net/ui/appfleet-nexus-ui/Pages/Inventory.razor`
- Route: `@page "/inventory"`
- State flags:
  - `List<VehicleDto> _vehicles`
  - `bool _isLoading = true`
  - `bool _showModal = false`
  - `VehicleDto _activeModel = new()`
  - `string? _errorMessage`
  - `string _searchQuery = ""`
  - `string _statusFilter = "All"`
- Methods:
  - `OnInitializedAsync()`: fetch vehicles from `api/vehicles`.
  - `OpenAddModal()`: clear `_activeModel`, set defaults, set `_showModal = true`.
  - `OpenEditModal(VehicleDto item)`: clone item attributes to `_activeModel` to avoid mutating list directly, set `_showModal = true`.
  - `SubmitForm()`: POST to `api/vehicles` if `_activeModel.Id == Guid.Empty`, else PUT to `api/vehicles/{Id}`.
  - `DeleteVehicle(Guid id)`: confirm with `JS` confirm dialog, then DELETE to `api/vehicles/{id}`.
