# Architecture Design & Implementation: Dashboard KPI Tiles

This document records the architectural decisions, server-side caching strategy, API endpoints, and styling guidelines for the authenticated user home page dashboard in FleetNexus.

---

## 1. Overview & Objective

When a user registers or logs in, they are redirected to the home route (`/`). Currently, they continue to see the public Alpha roadblock landing page because the root page `Home.razor` lacks state-aware rendering. 

This implementation plan refactors `Home.razor` to check the authorization state. 
- **Anonymous Users**: See the 4-step Alpha roadblock signup flow.
- **Authenticated Users**: See an interactive dashboard displaying Key Performance Indicators (KPIs) relevant to fleet management:
  1. **Fleet Inventory**: Shows counts of vehicles categorized by active statuses.
  2. **Maintenance Status**: Breaks down vehicle compliance (In Compliance, Coming Due, Out of Compliance).
  3. **Fuel Costs**: Tracks monthly expenditures and averages.
  4. **Registration Renewals**: Monitors documentation progress and expirations.
  5. **Drivers & Safety**: Measures driver metrics and average safety ratings.

---

## 2. API Design & Data Strategy

### Pre-Calculated Caching Strategy
To eliminate real-time calculation overhead on the database during web requests without the overhead of complex database migrations or RLS background service execution issues, we will compute KPIs on-the-fly and cache them in the server's `IMemoryCache` per tenant.

- Cache Key: `dashboard_kpis_{tenantId}`
- Cache Expiration: 30-minute absolute expiration
- Cache Invalidation: The cache entry for the tenant is evicted whenever a write operation (e.g. adding, updating, or deleting a vehicle) occurs.

### Dedicated Demo User Setup
When the specialized account `testuser@demo.com` (password `fLEETdEMO#2026`) logs in:
1. The auth handler intercepts the request and ensures the user exists in Supabase.
2. It seeds the user, tenant **"ABC Heating and Cooling"**, and **8 realistic commercial service vehicles** in the database:
   * **Unit 201**: 2021 Chevrolet Express 2500, Status: `Active`
   * **Unit 202**: 2022 Ford Transit-250, Status: `Active`
   * **Unit 203**: 2020 Chevrolet Express 2500, Status: `Active`
   * **Unit 204**: 2023 Ford Transit-350, Status: `Maintenance`
   * **Unit 301**: 2019 Ford F-150, Status: `Active`
   * **Unit 302**: 2020 Ram 1500, Status: `Active`
   * **Unit 101**: 2018 Ford E-350, Status: `Inactive`
   * **Unit 205**: 2022 Ford Transit-250, Status: `Maintenance`

3. The API will query the real database to serve inventory counts (8 vehicles) for this tenant, and return realistic, high-fidelity mock metrics (Maintenance compliance, Fuel costs, and Registrations) corresponding to this company's operational profile.

### KPI API Endpoint Lookups
The API controller retrieves metrics, checking the memory cache first:
```csharp
[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly FleetNexusDbContext _dbContext;
    private readonly ITenantContextAccessor _tenantAccessor;
    private readonly IMemoryCache _cache;

    public DashboardController(FleetNexusDbContext dbContext, ITenantContextAccessor tenantAccessor, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantAccessor = tenantAccessor;
        _cache = cache;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var tenantId = _tenantAccessor.CurrentTenantId;
        var cacheKey = $"dashboard_kpis_{tenantId}";

        if (!_cache.TryGetValue(cacheKey, out DashboardKpiDto dto))
        {
            // 1. Fetch real vehicles from the database (tenant-isolated automatically by EF query filter)
            var vehicles = await _dbContext.Vehicles.ToListAsync();
            
            var totalVehicles = vehicles.Count;
            var activeVehicles = vehicles.Count(v => v.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
            var maintenanceVehicles = vehicles.Count(v => v.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase));
            var inactiveVehicles = vehicles.Count(v => v.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase));

            // 2. Identify if the current tenant is the "ABC Heating and Cooling" demo tenant.
            var isDemoTenant = _dbContext.Tenants.Any(t => t.Id == tenantId && t.Name == "ABC Heating and Cooling");

            dto = new DashboardKpiDto
            {
                CalculatedAt = DateTime.UtcNow,
                Inventory = new VehicleKpiDto
                {
                    TotalCount = totalVehicles,
                    ActiveCount = activeVehicles,
                    MaintenanceCount = maintenanceVehicles,
                    InactiveCount = inactiveVehicles
                }
            };

            if (isDemoTenant)
            {
                // Populate high-fidelity mock metrics representing ABC Heating and Cooling
                dto.Maintenance = new MaintenanceKpiDto { InCompliance = 5, ComingDue = 2, OutOfCompliance = 1 };
                dto.Fuel = new FuelKpiDto { TotalCost = 5420.50M, AverageMpg = 14.8M, CostPerMile = 0.28M, PercentageChange = 4.2 };
                dto.Registration = new RegistrationKpiDto { Active = 6, InProgress = 1, ExpiringSoon = 1, Expired = 0 };
                dto.Drivers = new DriverKpiDto { TotalDrivers = 6, ActiveDrivers = 6, ExpiredLicenses = 0, AverageSafetyScore = 92 };
            }
            else
            {
                // For standard users, other KPIs show zero since those tables do not exist yet
                dto.Maintenance = new MaintenanceKpiDto { InCompliance = 0, ComingDue = 0, OutOfCompliance = 0 };
                dto.Fuel = new FuelKpiDto { TotalCost = 0, AverageMpg = 0, CostPerMile = 0, PercentageChange = 0 };
                dto.Registration = new RegistrationKpiDto { Active = 0, InProgress = 0, ExpiringSoon = 0, Expired = 0 };
                dto.Drivers = new DriverKpiDto { TotalDrivers = 0, ActiveDrivers = 0, ExpiredLicenses = 0, AverageSafetyScore = 0 };
            }

            // Cache for 30 minutes
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(30));
        }

        return Ok(dto);
    }
}
```

---

## 3. UI/UX Design & Styling Choices

To align with modern web application guidelines, the dashboard will utilize:
- **Glassmorphism**: Translucent cards (`rgba(255, 255, 255, 0.8)`) with `backdrop-filter: blur(12px)` and thin, bright borders to make components float on the page background.
- **Color Accents**: Curated status indicator colors matching the context (Cobalt for general inventory, Yellow/Orange for warnings, Green for success, Red for expired/critical issues).
- **Responsive Layout**: Utilizing standard CSS flex/grid grids that support stacked single columns on small mobile devices and expand to multi-column rows on wide viewports.
- **Micro-Animations**: Elevate hover events on tiles by scaling cards up slightly and showing a soft glowing shadow.

### CSS Class Outline (`app.css`)
```css
.dashboard-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 1.5rem;
    margin-top: 1.5rem;
}

.kpi-tile-card {
    background: rgba(255, 255, 255, 0.8);
    backdrop-filter: blur(12px);
    border: 1px solid rgba(226, 232, 240, 0.8);
    border-radius: 16px;
    padding: 1.5rem;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    transition: transform 0.25s ease, box-shadow 0.25s ease;
    position: relative;
    overflow: hidden;
}

.kpi-tile-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 12px 30px rgba(31, 38, 135, 0.08);
}

/* Accent strip at the top of tiles */
.kpi-accent-bar {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 4px;
}
.accent-cobalt { background: var(--brand-cobalt); }
.accent-green { background: var(--brand-green); }
.accent-amber { background: #f59e0b; }
.accent-danger { background: #ef4444; }
```

---

## 4. Implementation Phases

### Phase 1: Models & API Contracts
- Define `DashboardKpiDto` and sub-dtos.

### Phase 2: Interceptor & Database Seeder
- Modify `AuthController.cs` to intercept `testuser@demo.com` login.
- Implement `EnsureDemoDataSetupAsync` to populate local Postgres tables (`users`, `tenants`, `tenant_users`, and `vehicles`) for "ABC Heating and Cooling" upon login.

### Phase 3: Dashboard Controller with Caching
- Register `IMemoryCache` in `Program.cs` if not already present (`builder.Services.AddMemoryCache()`).
- Implement `DashboardController` in `appfleet-nexus-api` exposing `GET /api/dashboard/kpis` with `IMemoryCache` check/setting logic.

### Phase 4: Blazor Page Restructuring (`Home.razor`)
- Wrap the page body in `<AuthorizeView>`.
- In `<NotAuthorized>`, keep the roadblock registration SPA wizard.
- In `<Authorized>`, make an API call to `/api/dashboard/kpis` to display a beautiful grid of KPI tiles.
- Implement responsive layout columns, glassmorphic card styling, and custom empty states (0 counts) with call-to-actions.

### Phase 5: CSS Tweaks & Styling
- Append responsive layout classes, card transitions, and accent gradients to `wwwroot/css/app.css`.

### Phase 6: Verification & Validation
- Compile frontend & backend.
- Sign in with a standard newly registered user to verify that the dashboard shows `0` counts and clean empty states.
- Sign in with `testuser@demo.com` and verify that "ABC Heating and Cooling" is seeded and its 8 vehicles and derived KPI stats load properly.
