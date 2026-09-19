using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AppFleetNexus.Api.Models;
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

    // ─────────────────────────────────────────────────────────────────────────────
    // GET /api/vehicles
    // ─────────────────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetVehicles()
    {
        try
        {
            var vehicles = await _dbContext.Vehicles
                .Include(v => v.ContactAssignments)
                    .ThenInclude(vc => vc.Contact)
                .ToListAsync();

            var vehicleIds = vehicles.Select(v => v.Id).ToList();

            // Load primary garage addresses for all vehicles
            var primaryAddresses = await _dbContext.ContactAddresses
                .Where(a => a.OwnerType == "Vehicle" && vehicleIds.Contains(a.OwnerId) && a.IsPrimary)
                .ToDictionaryAsync(a => a.OwnerId, a =>
                    string.Join(", ", new[] { a.AddressLine1, a.City, a.State, a.PostalCode }
                        .Where(s => !string.IsNullOrWhiteSpace(s))));

            var dtos = vehicles.Select(v => new VehicleDetailDto
            {
                Id = v.Id,
                UnitNumber = v.UnitNumber,
                Vin = v.Vin,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LicensePlate = v.LicensePlate,
                LicenseState = v.LicenseState,
                Type = v.Type,
                Status = v.Status,
                PrimaryGarageAddress = primaryAddresses.GetValueOrDefault(v.Id),
                AssignedContacts = v.ContactAssignments
                    .Where(vc => !vc.IsDeleted)
                    .Select(vc => new VehicleContactAssignmentDto
                    {
                        Id = vc.Id,
                        VehicleId = vc.VehicleId,
                        VehicleUnitNumber = v.UnitNumber,
                        ContactId = vc.ContactId,
                        ContactFullName = $"{vc.Contact.FirstName} {vc.Contact.LastName}",
                        AssociationRole = vc.AssociationRole,
                        IsPrimary = vc.IsPrimary,
                        AssignedDate = vc.AssignedDate
                    }).ToList()
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles");
            return StatusCode(500, new { message = "An error occurred while retrieving vehicles." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // GET /api/vehicles/{id}
    // ─────────────────────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVehicleById(Guid id)
    {
        try
        {
            var vehicle = await _dbContext.Vehicles
                .Include(v => v.ContactAssignments)
                    .ThenInclude(vc => vc.Contact)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found." });

            var addresses = await _dbContext.ContactAddresses
                .Where(a => a.OwnerType == "Vehicle" && a.OwnerId == id)
                .Select(a => new ContactAddressDto
                {
                    Id = a.Id, Label = a.Label, IsPrimary = a.IsPrimary,
                    AddressLine1 = a.AddressLine1, AddressLine2 = a.AddressLine2,
                    AddressLine3 = a.AddressLine3, AddressLine4 = a.AddressLine4,
                    City = a.City, State = a.State, PostalCode = a.PostalCode, Country = a.Country
                }).ToListAsync();

            var phones = await _dbContext.ContactPhones
                .Where(p => p.OwnerType == "Vehicle" && p.OwnerId == id)
                .Select(p => new ContactPhoneDto { Id = p.Id, Label = p.Label, IsPrimary = p.IsPrimary, PhoneNumber = p.PhoneNumber })
                .ToListAsync();

            var primaryAddr = addresses.FirstOrDefault(a => a.IsPrimary);
            var primaryAddrStr = primaryAddr == null ? null :
                string.Join(", ", new[] { primaryAddr.AddressLine1, primaryAddr.City, primaryAddr.State, primaryAddr.PostalCode }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            var dto = new VehicleDetailDto
            {
                Id = vehicle.Id,
                UnitNumber = vehicle.UnitNumber,
                Vin = vehicle.Vin,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                LicensePlate = vehicle.LicensePlate,
                LicenseState = vehicle.LicenseState,
                Type = vehicle.Type,
                Status = vehicle.Status,
                PrimaryGarageAddress = primaryAddrStr,
                ContactAddresses = addresses,
                ContactPhones = phones,
                AssignedContacts = vehicle.ContactAssignments
                    .Where(vc => !vc.IsDeleted)
                    .Select(vc => new VehicleContactAssignmentDto
                    {
                        Id = vc.Id,
                        VehicleId = vc.VehicleId,
                        VehicleUnitNumber = vehicle.UnitNumber,
                        ContactId = vc.ContactId,
                        ContactFullName = $"{vc.Contact.FirstName} {vc.Contact.LastName}",
                        AssociationRole = vc.AssociationRole,
                        IsPrimary = vc.IsPrimary,
                        AssignedDate = vc.AssignedDate
                    }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle {VehicleId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the vehicle." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // POST /api/vehicles
    // ─────────────────────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] VehicleUpsertRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // I-002: At least 1 contact assignment required (AC-011)
            if (request.AssignedContacts.Count == 0)
                return BadRequest(new { message = "At least one contact must be assigned to a vehicle (I-002)." });

            // Check if unit number is already in use by active vehicles of this tenant.
            var unitNumberExists = await _dbContext.Vehicles
                .AnyAsync(v => v.UnitNumber.ToLower() == request.UnitNumber.ToLower());

            if (unitNumberExists)
                return BadRequest(new { message = $"A vehicle with Unit Number '{request.UnitNumber}' already exists." });

            // Validate all contact IDs exist
            foreach (var ca in request.AssignedContacts)
            {
                var contactExists = await _dbContext.Contacts.AnyAsync(c => c.Id == ca.ContactId);
                if (!contactExists)
                    return BadRequest(new { message = $"Contact {ca.ContactId} not found or not accessible." });
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

            // Persist contact assignments — ensure only one primary (default to first driver)
            bool hasSetPrimary = false;
            foreach (var ca in request.AssignedContacts)
            {
                var isPrimary = ca.IsPrimary;
                if (!hasSetPrimary && ca.AssociationRole == "Driver")
                {
                    isPrimary = true;
                    hasSetPrimary = true;
                }
                _dbContext.VehicleContacts.Add(new VehicleContact
                {
                    VehicleId = vehicle.Id,
                    ContactId = ca.ContactId,
                    AssociationRole = ca.AssociationRole,
                    IsPrimary = isPrimary,
                    AssignedDate = DateTime.UtcNow
                });
            }

            // Persist optional vehicle contact addresses (e.g. garage address) (AC-008)
            foreach (var a in request.ContactAddresses)
            {
                _dbContext.ContactAddresses.Add(new ContactAddress
                {
                    OwnerId = vehicle.Id,
                    OwnerType = "Vehicle",
                    Label = a.Label,
                    IsPrimary = a.IsPrimary,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    AddressLine3 = a.AddressLine3,
                    AddressLine4 = a.AddressLine4,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Country = a.Country
                });
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Vehicle {Id} created with {Count} contact(s)",
                vehicle.Id, request.AssignedContacts.Count);

            var tenantId = _tenantAccessor.CurrentTenantId;
            _cache.Remove($"dashboard_kpis_{tenantId}");

            return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.Id }, new { vehicle.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle");
            return StatusCode(500, new { message = "An error occurred while creating the vehicle." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // PUT /api/vehicles/{id}
    // ─────────────────────────────────────────────────────────────────────────────

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] VehicleUpsertRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // I-002: At least 1 contact assignment required on update (AC-011)
            if (request.AssignedContacts.Count == 0)
                return BadRequest(new { message = "At least one contact must be assigned to a vehicle (I-002)." });

            var vehicle = await _dbContext.Vehicles.FindAsync(id);
            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found." });

            // Check if unit number is in use by another active vehicle of this tenant
            var unitNumberExists = await _dbContext.Vehicles
                .AnyAsync(v => v.Id != id && v.UnitNumber.ToLower() == request.UnitNumber.ToLower());

            if (unitNumberExists)
                return BadRequest(new { message = $"A vehicle with Unit Number '{request.UnitNumber}' already exists." });

            // Validate all contact IDs exist
            foreach (var ca in request.AssignedContacts)
            {
                var contactExists = await _dbContext.Contacts.AnyAsync(c => c.Id == ca.ContactId);
                if (!contactExists)
                    return BadRequest(new { message = $"Contact {ca.ContactId} not found or not accessible." });
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

            // Sync contact assignments
            var existingAssignments = await _dbContext.VehicleContacts
                .Where(vc => vc.VehicleId == id)
                .ToListAsync();
            foreach (var ea in existingAssignments) _dbContext.VehicleContacts.Remove(ea);

            bool hasSetPrimary = false;
            foreach (var ca in request.AssignedContacts)
            {
                var isPrimary = ca.IsPrimary;
                if (!hasSetPrimary && ca.AssociationRole == "Driver")
                {
                    isPrimary = true;
                    hasSetPrimary = true;
                }
                _dbContext.VehicleContacts.Add(new VehicleContact
                {
                    VehicleId = id,
                    ContactId = ca.ContactId,
                    AssociationRole = ca.AssociationRole,
                    IsPrimary = isPrimary,
                    AssignedDate = DateTime.UtcNow
                });
            }

            // Sync vehicle contact addresses
            var existingAddresses = await _dbContext.ContactAddresses
                .Where(a => a.OwnerType == "Vehicle" && a.OwnerId == id)
                .ToListAsync();
            foreach (var ea in existingAddresses) _dbContext.ContactAddresses.Remove(ea);

            foreach (var a in request.ContactAddresses)
            {
                _dbContext.ContactAddresses.Add(new ContactAddress
                {
                    OwnerId = id,
                    OwnerType = "Vehicle",
                    Label = a.Label,
                    IsPrimary = a.IsPrimary,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    AddressLine3 = a.AddressLine3,
                    AddressLine4 = a.AddressLine4,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Country = a.Country
                });
            }

            await _dbContext.SaveChangesAsync();

            var tenantId = _tenantAccessor.CurrentTenantId;
            _cache.Remove($"dashboard_kpis_{tenantId}");

            return Ok(new { vehicle.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the vehicle." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // DELETE /api/vehicles/{id}
    // ─────────────────────────────────────────────────────────────────────────────

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        try
        {
            var vehicle = await _dbContext.Vehicles.FindAsync(id);
            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found." });

            // Soft delete VehicleContact join rows
            var assignments = await _dbContext.VehicleContacts
                .Where(vc => vc.VehicleId == id)
                .ToListAsync();
            foreach (var a in assignments) _dbContext.VehicleContacts.Remove(a);

            // Remove converts to soft delete automatically inside SaveChangesAsync
            _dbContext.Vehicles.Remove(vehicle);
            await _dbContext.SaveChangesAsync();

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

    // ─────────────────────────────────────────────────────────────────────────────
    // GET /api/vehicles/{id}/contacts
    // ─────────────────────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/contacts")]
    public async Task<IActionResult> GetVehicleContacts(Guid id)
    {
        try
        {
            var vehicleExists = await _dbContext.Vehicles.AnyAsync(v => v.Id == id);
            if (!vehicleExists)
                return NotFound(new { message = "Vehicle not found." });

            var assignments = await _dbContext.VehicleContacts
                .Include(vc => vc.Contact)
                .Where(vc => vc.VehicleId == id)
                .Select(vc => new VehicleContactAssignmentDto
                {
                    Id = vc.Id,
                    VehicleId = vc.VehicleId,
                    ContactId = vc.ContactId,
                    ContactFullName = vc.Contact.FirstName + " " + vc.Contact.LastName,
                    AssociationRole = vc.AssociationRole,
                    IsPrimary = vc.IsPrimary,
                    AssignedDate = vc.AssignedDate
                })
                .ToListAsync();

            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts for vehicle {VehicleId}", id);
            return StatusCode(500, new { message = "An error occurred." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // GET /api/vehicles/{id}/addresses
    // ─────────────────────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/addresses")]
    public async Task<IActionResult> GetVehicleAddresses(Guid id)
    {
        try
        {
            var vehicleExists = await _dbContext.Vehicles.AnyAsync(v => v.Id == id);
            if (!vehicleExists)
                return NotFound(new { message = "Vehicle not found." });

            var addresses = await _dbContext.ContactAddresses
                .Where(a => a.OwnerType == "Vehicle" && a.OwnerId == id)
                .Select(a => new ContactAddressDto
                {
                    Id = a.Id, Label = a.Label, IsPrimary = a.IsPrimary,
                    AddressLine1 = a.AddressLine1, AddressLine2 = a.AddressLine2,
                    AddressLine3 = a.AddressLine3, AddressLine4 = a.AddressLine4,
                    City = a.City, State = a.State, PostalCode = a.PostalCode, Country = a.Country
                }).ToListAsync();

            return Ok(addresses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving addresses for vehicle {VehicleId}", id);
            return StatusCode(500, new { message = "An error occurred." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // POST /api/vehicles/{id}/addresses
    // ─────────────────────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/addresses")]
    public async Task<IActionResult> AddVehicleAddress(Guid id, [FromBody] ContactAddressDto request)
    {
        try
        {
            var vehicleExists = await _dbContext.Vehicles.AnyAsync(v => v.Id == id);
            if (!vehicleExists)
                return NotFound(new { message = "Vehicle not found." });

            var address = new ContactAddress
            {
                OwnerId = id,
                OwnerType = "Vehicle",
                Label = request.Label,
                IsPrimary = request.IsPrimary,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                AddressLine3 = request.AddressLine3,
                AddressLine4 = request.AddressLine4,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country
            };

            _dbContext.ContactAddresses.Add(address);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("ContactAddress {Id} added to Vehicle {OwnerId}", address.Id, id);

            return StatusCode(201, new ContactAddressDto
            {
                Id = address.Id, Label = address.Label, IsPrimary = address.IsPrimary,
                AddressLine1 = address.AddressLine1, AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3, AddressLine4 = address.AddressLine4,
                City = address.City, State = address.State, PostalCode = address.PostalCode, Country = address.Country
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding address to vehicle {VehicleId}", id);
            return StatusCode(500, new { message = "An error occurred." });
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Request Model (extended with contact assignment support — FEATURE-001)
// ─────────────────────────────────────────────────────────────────────────────

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

    // Contact Management extensions (FEATURE-001)
    /// <summary>Required: At least one contact must be assigned (I-002 / AC-011).</summary>
    public System.Collections.Generic.List<VehicleContactAssignmentDto> AssignedContacts { get; set; } = new();

    /// <summary>Optional vehicle contact points — e.g. Garage Address (AC-008).</summary>
    public System.Collections.Generic.List<ContactAddressDto> ContactAddresses { get; set; } = new();
}
