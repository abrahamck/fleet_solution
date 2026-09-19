using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppFleetNexus.Api.Models;

/// <summary>
/// Vehicle list/detail DTO returned from API.
/// </summary>
public class VehicleDetailDto
{
    public Guid Id { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string? Vin { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicenseState { get; set; }
    public string? Type { get; set; }
    public string Status { get; set; } = "Active";

    // Contact management extensions (FEATURE-001)
    public List<VehicleContactAssignmentDto> AssignedContacts { get; set; } = new();
    public List<ContactAddressDto> ContactAddresses { get; set; } = new();
    public List<ContactPhoneDto> ContactPhones { get; set; } = new();

    /// <summary>Primary garage address (denormalized for convenience).</summary>
    public string? PrimaryGarageAddress { get; set; }
}
