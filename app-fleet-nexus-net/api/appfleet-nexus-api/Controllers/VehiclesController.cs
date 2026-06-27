using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AppFleetNexus.Data.Data;
using AppFleetNexus.Data.Models;
using AppFleetNexus.Data.Tenancy;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AppFleetNexus.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly FleetNexusDbContext _dbContext;
    private readonly ITenantContextAccessor _tenantAccessor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(
        FleetNexusDbContext dbContext,
        ITenantContextAccessor tenantAccessor,
        IMemoryCache cache,
        ILogger<VehiclesController> logger)
    {
        _dbContext = dbContext;
        _tenantAccessor = tenantAccessor;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetVehicles()
    {
        try
        {
            var vehicles = await _dbContext.Vehicles.ToListAsync();
            return Ok(vehicles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles");
            return StatusCode(500, new { message = "An error occurred while retrieving vehicles." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleById(Guid id)
    {
        try
        {
            var vehicle = await _dbContext.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(new { message = "Vehicle not found." });
            }
            return Ok(vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle {VehicleId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the vehicle." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] VehicleUpsertRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if unit number is already in use by active vehicles of this tenant.
            // Note: EF Global Query Filter automatically applies TenantId and !IsDeleted filters.
            var unitNumberExists = await _dbContext.Vehicles
                .AnyAsync(v => v.UnitNumber.ToLower() == request.UnitNumber.ToLower());

            if (unitNumberExists)
            {
                return BadRequest(new { message = $"A vehicle with Unit Number '{request.UnitNumber}' already exists." });
            }

            var vehicle = new Vehicle
            {
                UnitNumber = request.UnitNumber.Trim(),
                Vin = request.Vin?.Trim(),
                Make = request.Make?.Trim(),
                Model = request.Model?.Trim(),
                Year = request.Year,
                LicensePlate = request.LicensePlate?.Trim(),
                LicenseState = request.LicenseState?.Trim(),
                Type = request.Type?.Trim(),
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim()
            };

            _dbContext.Vehicles.Add(vehicle);
            await _dbContext.SaveChangesAsync();

            // Evict dashboard metrics cache for this tenant
            var tenantId = _tenantAccessor.CurrentTenantId;
            _cache.Remove($"dashboard_kpis_{tenantId}");

            return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.Id }, vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle");
            return StatusCode(500, new { message = "An error occurred while creating the vehicle." });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] VehicleUpsertRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vehicle = await _dbContext.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(new { message = "Vehicle not found." });
            }

            // Check if unit number is in use by another active vehicle of this tenant
            var unitNumberExists = await _dbContext.Vehicles
                .AnyAsync(v => v.Id != id && v.UnitNumber.ToLower() == request.UnitNumber.ToLower());

            if (unitNumberExists)
            {
                return BadRequest(new { message = $"A vehicle with Unit Number '{request.UnitNumber}' already exists." });
            }

            vehicle.UnitNumber = request.UnitNumber.Trim();
            vehicle.Vin = request.Vin?.Trim();
            vehicle.Make = request.Make?.Trim();
            vehicle.Model = request.Model?.Trim();
            vehicle.Year = request.Year;
            vehicle.LicensePlate = request.LicensePlate?.Trim();
            vehicle.LicenseState = request.LicenseState?.Trim();
            vehicle.Type = request.Type?.Trim();
            vehicle.Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim();

            await _dbContext.SaveChangesAsync();

            // Evict dashboard metrics cache for this tenant
            var tenantId = _tenantAccessor.CurrentTenantId;
            _cache.Remove($"dashboard_kpis_{tenantId}");

            return Ok(vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the vehicle." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        try
        {
            var vehicle = await _dbContext.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(new { message = "Vehicle not found." });
            }

            // Remove converts to soft delete automatically inside SaveChangesAsync
            _dbContext.Vehicles.Remove(vehicle);
            await _dbContext.SaveChangesAsync();

            // Evict dashboard metrics cache for this tenant
            var tenantId = _tenantAccessor.CurrentTenantId;
            _cache.Remove($"dashboard_kpis_{tenantId}");

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the vehicle." });
        }
    }
}

public class VehicleUpsertRequest
{
    [Required(ErrorMessage = "Unit Number is required.")]
    [StringLength(50, ErrorMessage = "Unit Number cannot exceed 50 characters.")]
    public string UnitNumber { get; set; } = string.Empty;

    public string? Vin { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }

    [Range(1900, 2100, ErrorMessage = "Please enter a valid year.")]
    public int? Year { get; set; }

    public string? LicensePlate { get; set; }

    [StringLength(2, ErrorMessage = "License State must be a 2-letter abbreviation.")]
    public string? LicenseState { get; set; }

    public string? Type { get; set; }
    public string Status { get; set; } = "Active";
}
