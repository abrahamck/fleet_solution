using System;
using System.Collections.Generic;

namespace AppFleetNexus.Data.Models;

public class Contact : BaseEntity
{
    // Business Identity
    public string? UniqueId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;

    /// <summary>Driver | FleetManager | Dispatcher | Customer | Vendor | Other</summary>
    public string ContactType { get; set; } = "Other";

    /// <summary>Active | Inactive</summary>
    public string Status { get; set; } = "Active";

    public string? JobTitle { get; set; }

    // Driver Compliance
    public string? LicenseNumber { get; set; }
    public string? LicenseState { get; set; }
    public DateOnly? LicenseExpirationDate { get; set; }
    public DateOnly? MedicalCertExpirationDate { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Metadata
    public string? Notes { get; set; }

    // Navigation
    public ICollection<VehicleContact> VehicleAssignments { get; set; } = new List<VehicleContact>();
}
