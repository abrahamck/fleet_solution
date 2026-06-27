using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AppFleetNexus.Data.Data;
using AppFleetNexus.Data.Tenancy;
using AppFleetNexus.Api.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppFleetNexus.Api.Controllers;

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
        try
        {
            var tenantId = _tenantAccessor.CurrentTenantId;
            var cacheKey = $"dashboard_kpis_{tenantId}";

            if (!_cache.TryGetValue(cacheKey, out DashboardKpiDto? dto) || dto == null)
            {
                // 1. Fetch real vehicles from the database (tenant-isolated automatically by EF query filter)
                var vehicles = await _dbContext.Vehicles.ToListAsync();
                
                var totalVehicles = vehicles.Count;
                var activeVehicles = vehicles.Count(v => v.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
                var maintenanceVehicles = vehicles.Count(v => v.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase));
                var inactiveVehicles = vehicles.Count(v => v.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase));

                // 2. Identify if the current tenant is the "ABC Heating and Cooling" demo tenant.
                // We check if a tenant with this ID is named "ABC Heating and Cooling" in the DB.
                var isDemoTenant = await _dbContext.Tenants
                    .IgnoreQueryFilters()
                    .AnyAsync(t => t.Id == tenantId && t.Name == "ABC Heating and Cooling");

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
                    dto.Maintenance = new MaintenanceKpiDto
                    {
                        InCompliance = 5,
                        ComingDue = 2,
                        OutOfCompliance = 1
                    };
                    dto.Fuel = new FuelKpiDto
                    {
                        TotalCost = 5420.50M,
                        AverageMpg = 14.8M,
                        CostPerMile = 0.28M,
                        PercentageChange = 4.2
                    };
                    dto.Registration = new RegistrationKpiDto
                    {
                        Active = 6,
                        InProgress = 1,
                        ExpiringSoon = 1,
                        Expired = 0
                    };
                    dto.Drivers = new DriverKpiDto
                    {
                        TotalDrivers = 6,
                        ActiveDrivers = 6,
                        ExpiredLicenses = 0,
                        AverageSafetyScore = 92
                    };
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
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
