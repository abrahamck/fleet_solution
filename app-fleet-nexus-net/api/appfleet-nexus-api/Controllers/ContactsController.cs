using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AppFleetNexus.Api.Models;
using AppFleetNexus.Data.Data;
using AppFleetNexus.Data.Models;
using AppFleetNexus.Data.Tenancy;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AppFleetNexus.Api.Controllers;

/// <summary>
/// REST controller for the Contact directory.
/// Enforces:
///   I-001 — Contact must have ≥1 phone AND ≥1 address at all times (AC-003, AC-004, AC-009).
///   I-005 — Cannot soft-delete a contact that is the sole active assignment on any vehicle (AC-015).
/// </summary>
[Authorize]
[ApiController]
[Route("api/contacts")]
public class ContactsController : ControllerBase
{
    private readonly FleetNexusDbContext _db;
    private readonly ITenantContextAccessor _tenantAccessor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(
        FleetNexusDbContext db,
        ITenantContextAccessor tenantAccessor,
        IMemoryCache cache,
        ILogger<ContactsController> logger)
    {
        _db = db;
        _tenantAccessor = tenantAccessor;
        _cache = cache;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // GET /api/contacts
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a filtered list of contacts with their primary phone/email/address and vehicle count.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContacts(
        [FromQuery] string? q,
        [FromQuery] string? contactType,
        [FromQuery] string? status)
    {
        try
        {
            var query = _db.Contacts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(q) ||
                    c.LastName.Contains(q) ||
                    (c.UniqueId != null && c.UniqueId.Contains(q)) ||
                    (c.JobTitle != null && c.JobTitle.Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(contactType))
                query = query.Where(c => c.ContactType == contactType);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);

            var contacts = await query.ToListAsync();
            var tenantId = _tenantAccessor.CurrentTenantId;

            // Load primary phones, emails, addresses in bulk for this tenant
            var contactIds = contacts.Select(c => c.Id).ToList();

            var primaryPhones = await _db.ContactPhones
                .Where(p => p.OwnerType == "Contact" && contactIds.Contains(p.OwnerId) && p.IsPrimary)
                .ToDictionaryAsync(p => p.OwnerId, p => p.PhoneNumber);

            var primaryEmails = await _db.ContactEmails
                .Where(e => e.OwnerType == "Contact" && contactIds.Contains(e.OwnerId) && e.IsPrimary)
                .ToDictionaryAsync(e => e.OwnerId, e => e.EmailAddress);

            var primaryAddresses = await _db.ContactAddresses
                .Where(a => a.OwnerType == "Contact" && contactIds.Contains(a.OwnerId) && a.IsPrimary)
                .ToDictionaryAsync(a => a.OwnerId, a =>
                    string.Join(", ", new[] { a.AddressLine1, a.City, a.State, a.PostalCode }
                        .Where(s => !string.IsNullOrWhiteSpace(s))));

            var vehicleCounts = await _db.VehicleContacts
                .Where(vc => contactIds.Contains(vc.ContactId))
                .GroupBy(vc => vc.ContactId)
                .Select(g => new { ContactId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ContactId, x => x.Count);

            var dtos = contacts.Select(c => new ContactSummaryDto
            {
                Id = c.Id,
                UniqueId = c.UniqueId,
                FullName = BuildFullName(c.FirstName, c.MiddleName, c.LastName),
                ContactType = c.ContactType,
                Status = c.Status,
                PrimaryPhone = primaryPhones.GetValueOrDefault(c.Id),
                PrimaryEmail = primaryEmails.GetValueOrDefault(c.Id),
                PrimaryAddress = primaryAddresses.GetValueOrDefault(c.Id),
                AssignedVehicleCount = vehicleCounts.GetValueOrDefault(c.Id, 0)
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts");
            return StatusCode(500, new { message = "An error occurred while retrieving contacts." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // GET /api/contacts/summary
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns KPI tile counts for the dashboard: total contacts, active drivers, licenses expiring in 30 days.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetContactSummary()
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var in30Days = today.AddDays(30);

            var totalContacts = await _db.Contacts.CountAsync();
            var activeDrivers = await _db.Contacts
                .CountAsync(c => c.ContactType == "Driver" && c.Status == "Active");
            var expiringLicenses = await _db.Contacts
                .CountAsync(c => c.LicenseExpirationDate.HasValue
                    && c.LicenseExpirationDate.Value >= today
                    && c.LicenseExpirationDate.Value <= in30Days);

            return Ok(new ContactKpiDto
            {
                TotalContacts = totalContacts,
                ActiveDrivers = activeDrivers,
                LicensesExpiringIn30Days = expiringLicenses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contact summary");
            return StatusCode(500, new { message = "An error occurred while retrieving contact summary." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // GET /api/contacts/{id}
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns full contact detail including all phones, emails, addresses, and vehicle assignments.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetContactById(Guid id)
    {
        try
        {
            var contact = await _db.Contacts.FindAsync(id);
            if (contact == null)
                return NotFound(new { message = "Contact not found." });

            var dto = await BuildDetailDtoAsync(contact);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the contact." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // POST /api/contacts
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new contact with identity, typed contact points, and optional vehicle assignments.
    /// Enforces I-001 (≥1 phone AND ≥1 address) and tenant UniqueId uniqueness.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateContact([FromBody] ContactUpsertRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // I-001: At least one phone required (AC-003)
            if (request.Phones.Count == 0)
                return BadRequest(new { message = "At least one phone number is required (I-001)." });

            // I-001: At least one address required (AC-004)
            if (request.Addresses.Count == 0)
                return BadRequest(new { message = "At least one address is required (I-001)." });

            // AC-006: Tenant UniqueId uniqueness check
            if (!string.IsNullOrWhiteSpace(request.UniqueId))
            {
                var exists = await _db.Contacts.AnyAsync(c => c.UniqueId == request.UniqueId.Trim());
                if (exists)
                    return BadRequest(new { message = $"A contact with UniqueId '{request.UniqueId}' already exists in this tenant." });
            }

            var contact = new Contact
            {
                UniqueId = request.UniqueId?.Trim(),
                FirstName = request.FirstName.Trim(),
                MiddleName = request.MiddleName?.Trim(),
                LastName = request.LastName.Trim(),
                ContactType = request.ContactType,
                Status = request.Status,
                JobTitle = request.JobTitle?.Trim(),
                LicenseNumber = request.LicenseNumber?.Trim(),
                LicenseState = request.LicenseState?.Trim(),
                LicenseExpirationDate = request.LicenseExpirationDate,
                MedicalCertExpirationDate = request.MedicalCertExpirationDate,
                EmergencyContactName = request.EmergencyContactName?.Trim(),
                EmergencyContactPhone = request.EmergencyContactPhone?.Trim(),
                Notes = request.Notes?.Trim()
            };

            _db.Contacts.Add(contact);

            // Persist phones — enforce single primary
            EnsureSinglePrimary(request.Phones);
            foreach (var p in request.Phones)
            {
                _db.ContactPhones.Add(new ContactPhone
                {
                    OwnerId = contact.Id,
                    OwnerType = "Contact",
                    Label = p.Label,
                    IsPrimary = p.IsPrimary,
                    PhoneNumber = p.PhoneNumber
                });
            }

            // Persist emails
            EnsureSinglePrimary(request.Emails);
            foreach (var e in request.Emails)
            {
                _db.ContactEmails.Add(new ContactEmail
                {
                    OwnerId = contact.Id,
                    OwnerType = "Contact",
                    Label = e.Label,
                    IsPrimary = e.IsPrimary,
                    EmailAddress = e.EmailAddress
                });
            }

            // Persist addresses — enforce single primary
            EnsureSinglePrimary(request.Addresses);
            foreach (var a in request.Addresses)
            {
                _db.ContactAddresses.Add(new ContactAddress
                {
                    OwnerId = contact.Id,
                    OwnerType = "Contact",
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

            // Optional vehicle assignments
            foreach (var va in request.VehicleAssignments)
            {
                var vehicleExists = await _db.Vehicles.AnyAsync(v => v.Id == va.VehicleId);
                if (!vehicleExists)
                    return BadRequest(new { message = $"Vehicle {va.VehicleId} not found or not accessible." });

                _db.VehicleContacts.Add(new VehicleContact
                {
                    VehicleId = va.VehicleId,
                    ContactId = contact.Id,
                    AssociationRole = va.AssociationRole,
                    IsPrimary = va.IsPrimary,
                    AssignedDate = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Contact {Id} ({Type}) created for tenant {TenantId}",
                contact.Id, contact.ContactType, _tenantAccessor.CurrentTenantId);

            _cache.Remove($"dashboard_kpis_{_tenantAccessor.CurrentTenantId}");

            var resultDto = await BuildDetailDtoAsync(contact);
            return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact");
            return StatusCode(500, new { message = "An error occurred while creating the contact." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // PUT /api/contacts/{id}
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates contact identity, syncs contact point collections, and syncs vehicle assignments.
    /// Guards: Must not remove the last Phone or last Address (AC-009).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] ContactUpsertRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var contact = await _db.Contacts.FindAsync(id);
            if (contact == null)
                return NotFound(new { message = "Contact not found." });

            // AC-009: Guard — Must not remove the last phone
            if (request.Phones.Count == 0)
                return BadRequest(new { message = "At least one phone number must remain on a contact (I-001)." });

            // AC-009: Guard — Must not remove the last address
            if (request.Addresses.Count == 0)
                return BadRequest(new { message = "At least one address must remain on a contact (I-001)." });

            // AC-006: UniqueId uniqueness check (excluding this contact)
            if (!string.IsNullOrWhiteSpace(request.UniqueId))
            {
                var dupExists = await _db.Contacts
                    .AnyAsync(c => c.UniqueId == request.UniqueId.Trim() && c.Id != id);
                if (dupExists)
                    return BadRequest(new { message = $"A contact with UniqueId '{request.UniqueId}' already exists in this tenant." });
            }

            // Update identity fields
            contact.UniqueId = request.UniqueId?.Trim();
            contact.FirstName = request.FirstName.Trim();
            contact.MiddleName = request.MiddleName?.Trim();
            contact.LastName = request.LastName.Trim();
            contact.ContactType = request.ContactType;
            contact.Status = request.Status;
            contact.JobTitle = request.JobTitle?.Trim();
            contact.LicenseNumber = request.LicenseNumber?.Trim();
            contact.LicenseState = request.LicenseState?.Trim();
            contact.LicenseExpirationDate = request.LicenseExpirationDate;
            contact.MedicalCertExpirationDate = request.MedicalCertExpirationDate;
            contact.EmergencyContactName = request.EmergencyContactName?.Trim();
            contact.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
            contact.Notes = request.Notes?.Trim();

            // Sync phones — remove all existing, re-insert
            var existingPhones = await _db.ContactPhones
                .Where(p => p.OwnerType == "Contact" && p.OwnerId == id)
                .ToListAsync();
            foreach (var ep in existingPhones) _db.ContactPhones.Remove(ep);

            EnsureSinglePrimary(request.Phones);
            foreach (var p in request.Phones)
            {
                _db.ContactPhones.Add(new ContactPhone
                {
                    OwnerId = id,
                    OwnerType = "Contact",
                    Label = p.Label,
                    IsPrimary = p.IsPrimary,
                    PhoneNumber = p.PhoneNumber
                });
            }

            // Sync emails
            var existingEmails = await _db.ContactEmails
                .Where(e => e.OwnerType == "Contact" && e.OwnerId == id)
                .ToListAsync();
            foreach (var ee in existingEmails) _db.ContactEmails.Remove(ee);

            EnsureSinglePrimary(request.Emails);
            foreach (var e in request.Emails)
            {
                _db.ContactEmails.Add(new ContactEmail
                {
                    OwnerId = id,
                    OwnerType = "Contact",
                    Label = e.Label,
                    IsPrimary = e.IsPrimary,
                    EmailAddress = e.EmailAddress
                });
            }

            // Sync addresses
            var existingAddresses = await _db.ContactAddresses
                .Where(a => a.OwnerType == "Contact" && a.OwnerId == id)
                .ToListAsync();
            foreach (var ea in existingAddresses) _db.ContactAddresses.Remove(ea);

            EnsureSinglePrimary(request.Addresses);
            foreach (var a in request.Addresses)
            {
                _db.ContactAddresses.Add(new ContactAddress
                {
                    OwnerId = id,
                    OwnerType = "Contact",
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

            // Sync vehicle assignments
            var existingAssignments = await _db.VehicleContacts
                .Where(vc => vc.ContactId == id)
                .ToListAsync();
            foreach (var ea in existingAssignments) _db.VehicleContacts.Remove(ea);

            foreach (var va in request.VehicleAssignments)
            {
                var vehicleExists = await _db.Vehicles.AnyAsync(v => v.Id == va.VehicleId);
                if (!vehicleExists)
                    return BadRequest(new { message = $"Vehicle {va.VehicleId} not found or not accessible." });

                _db.VehicleContacts.Add(new VehicleContact
                {
                    VehicleId = va.VehicleId,
                    ContactId = id,
                    AssociationRole = va.AssociationRole,
                    IsPrimary = va.IsPrimary,
                    AssignedDate = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            _cache.Remove($"dashboard_kpis_{_tenantAccessor.CurrentTenantId}");

            var resultDto = await BuildDetailDtoAsync(contact);
            return Ok(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the contact." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // DELETE /api/contacts/{id}
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Soft-deletes a contact.
    /// Guard I-005: Blocked (409 Conflict) if this contact is the sole active assigned contact on any vehicle.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteContact(Guid id)
    {
        try
        {
            var contact = await _db.Contacts.FindAsync(id);
            if (contact == null)
                return NotFound(new { message = "Contact not found." });

            // I-005: Check if this contact is the sole active contact on any vehicle (AC-015)
            var vehiclesWhereContactIsSole = await _db.VehicleContacts
                .Where(vc => vc.ContactId == id)
                .Select(vc => vc.VehicleId)
                .ToListAsync();

            foreach (var vehicleId in vehiclesWhereContactIsSole)
            {
                var otherContactCount = await _db.VehicleContacts
                    .CountAsync(vc => vc.VehicleId == vehicleId && vc.ContactId != id);

                if (otherContactCount == 0)
                {
                    _logger.LogWarning("Delete blocked: Contact {ContactId} is sole contact on vehicle(s)", id);
                    return Conflict(new
                    {
                        message = "This contact is the sole active contact on one or more vehicles and cannot be deleted. Please assign another contact to those vehicles first."
                    });
                }
            }

            // Soft delete — DbContext.Remove converts to IsDeleted = true
            _db.Contacts.Remove(contact);
            await _db.SaveChangesAsync();

            _cache.Remove($"dashboard_kpis_{_tenantAccessor.CurrentTenantId}");

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the contact." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private async Task<ContactDetailDto> BuildDetailDtoAsync(Contact contact)
    {
        var id = contact.Id;

        var phones = await _db.ContactPhones
            .Where(p => p.OwnerType == "Contact" && p.OwnerId == id)
            .Select(p => new ContactPhoneDto { Id = p.Id, Label = p.Label, IsPrimary = p.IsPrimary, PhoneNumber = p.PhoneNumber })
            .ToListAsync();

        var emails = await _db.ContactEmails
            .Where(e => e.OwnerType == "Contact" && e.OwnerId == id)
            .Select(e => new ContactEmailDto { Id = e.Id, Label = e.Label, IsPrimary = e.IsPrimary, EmailAddress = e.EmailAddress })
            .ToListAsync();

        var addresses = await _db.ContactAddresses
            .Where(a => a.OwnerType == "Contact" && a.OwnerId == id)
            .Select(a => new ContactAddressDto
            {
                Id = a.Id,
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
            })
            .ToListAsync();

        var vehicleAssignments = await _db.VehicleContacts
            .Include(vc => vc.Vehicle)
            .Where(vc => vc.ContactId == id)
            .Select(vc => new VehicleContactAssignmentDto
            {
                Id = vc.Id,
                VehicleId = vc.VehicleId,
                VehicleUnitNumber = vc.Vehicle.UnitNumber,
                ContactId = vc.ContactId,
                ContactFullName = BuildFullName(contact.FirstName, contact.MiddleName, contact.LastName),
                AssociationRole = vc.AssociationRole,
                IsPrimary = vc.IsPrimary,
                AssignedDate = vc.AssignedDate
            })
            .ToListAsync();

        var primaryPhone = phones.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber;
        var primaryEmail = emails.FirstOrDefault(e => e.IsPrimary)?.EmailAddress;
        var primaryAddr = addresses.FirstOrDefault(a => a.IsPrimary);
        var primaryAddrStr = primaryAddr == null ? null :
            string.Join(", ", new[] { primaryAddr.AddressLine1, primaryAddr.City, primaryAddr.State, primaryAddr.PostalCode }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        return new ContactDetailDto
        {
            Id = contact.Id,
            UniqueId = contact.UniqueId,
            FullName = BuildFullName(contact.FirstName, contact.MiddleName, contact.LastName),
            FirstName = contact.FirstName,
            MiddleName = contact.MiddleName,
            LastName = contact.LastName,
            ContactType = contact.ContactType,
            Status = contact.Status,
            JobTitle = contact.JobTitle,
            LicenseNumber = contact.LicenseNumber,
            LicenseState = contact.LicenseState,
            LicenseExpirationDate = contact.LicenseExpirationDate,
            MedicalCertExpirationDate = contact.MedicalCertExpirationDate,
            EmergencyContactName = contact.EmergencyContactName,
            EmergencyContactPhone = contact.EmergencyContactPhone,
            Notes = contact.Notes,
            PrimaryPhone = primaryPhone,
            PrimaryEmail = primaryEmail,
            PrimaryAddress = primaryAddrStr,
            AssignedVehicleCount = vehicleAssignments.Count,
            Phones = phones,
            Emails = emails,
            Addresses = addresses,
            VehicleAssignments = vehicleAssignments
        };
    }

    private static string BuildFullName(string firstName, string? middleName, string lastName)
    {
        return string.IsNullOrWhiteSpace(middleName)
            ? $"{firstName} {lastName}"
            : $"{firstName} {middleName} {lastName}";
    }

    /// <summary>
    /// Ensures only one item has IsPrimary = true.
    /// If none is marked primary, the first item is designated.
    /// </summary>
    private static void EnsureSinglePrimary<T>(System.Collections.Generic.List<T> items) where T : class
    {
        if (items.Count == 0) return;

        var isPrimaryProp = typeof(T).GetProperty("IsPrimary");
        if (isPrimaryProp == null) return;

        var primaryCount = items.Count(i => (bool)isPrimaryProp.GetValue(i)!);

        if (primaryCount == 0)
        {
            // Designate first item as primary
            isPrimaryProp.SetValue(items[0], true);
        }
        else if (primaryCount > 1)
        {
            // Keep only the last marked primary, clear the rest
            bool foundFirst = false;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if ((bool)isPrimaryProp.GetValue(items[i])!)
                {
                    if (!foundFirst)
                        foundFirst = true;
                    else
                        isPrimaryProp.SetValue(items[i], false);
                }
            }
        }
    }
}
