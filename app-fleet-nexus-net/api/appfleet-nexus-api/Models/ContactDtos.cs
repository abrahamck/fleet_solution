using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppFleetNexus.Api.Models;

// ─── Contact Summary (list view) ───────────────────────────────────────────────

public class ContactSummaryDto
{
    public Guid Id { get; set; }
    public string? UniqueId { get; set; }

    /// <summary>Computed: "First [Middle] Last"</summary>
    public string FullName { get; set; } = string.Empty;

    public string ContactType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    /// <summary>From contact_phones WHERE is_primary = true</summary>
    public string? PrimaryPhone { get; set; }

    /// <summary>From contact_emails WHERE is_primary = true</summary>
    public string? PrimaryEmail { get; set; }

    /// <summary>From contact_addresses WHERE is_primary = true</summary>
    public string? PrimaryAddress { get; set; }

    public int AssignedVehicleCount { get; set; }
}

// ─── Contact Detail (full view) ────────────────────────────────────────────────

public class ContactDetailDto : ContactSummaryDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }

    // Driver Compliance
    public string? LicenseNumber { get; set; }
    public string? LicenseState { get; set; }
    public DateOnly? LicenseExpirationDate { get; set; }
    public DateOnly? MedicalCertExpirationDate { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Notes { get; set; }

    public List<ContactPhoneDto> Phones { get; set; } = new();
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactAddressDto> Addresses { get; set; } = new();
    public List<VehicleContactAssignmentDto> VehicleAssignments { get; set; } = new();
}

// ─── Contact Upsert Request ─────────────────────────────────────────────────────

public class ContactUpsertRequest
{
    [StringLength(50, ErrorMessage = "UniqueId cannot exceed 50 characters.")]
    public string? UniqueId { get; set; }

    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Middle Name cannot exceed 100 characters.")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Driver | FleetManager | Dispatcher | Customer | Vendor | Other</summary>
    public string ContactType { get; set; } = "Other";

    /// <summary>Active | Inactive</summary>
    public string Status { get; set; } = "Active";

    [StringLength(200, ErrorMessage = "Job Title cannot exceed 200 characters.")]
    public string? JobTitle { get; set; }

    // Driver Compliance (optional)
    public string? LicenseNumber { get; set; }

    [StringLength(2, ErrorMessage = "License State must be a 2-letter abbreviation.")]
    public string? LicenseState { get; set; }

    public DateOnly? LicenseExpirationDate { get; set; }
    public DateOnly? MedicalCertExpirationDate { get; set; }

    [StringLength(200, ErrorMessage = "Emergency Contact Name cannot exceed 200 characters.")]
    public string? EmergencyContactName { get; set; }

    [StringLength(30, ErrorMessage = "Emergency Contact Phone cannot exceed 30 characters.")]
    public string? EmergencyContactPhone { get; set; }

    [StringLength(4000, ErrorMessage = "Notes cannot exceed 4000 characters.")]
    public string? Notes { get; set; }

    // Contact point collections — at least 1 phone and 1 address required (AC-003, AC-004)
    public List<ContactPhoneDto> Phones { get; set; } = new();
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactAddressDto> Addresses { get; set; } = new();

    // Optional initial vehicle assignments
    public List<VehicleContactAssignmentDto> VehicleAssignments { get; set; } = new();
}

// ─── Typed Contact Point DTOs ───────────────────────────────────────────────────

public class ContactPhoneDto
{
    public Guid Id { get; set; }

    public string? Label { get; set; }

    public bool IsPrimary { get; set; }

    [Required(ErrorMessage = "Phone Number is required.")]
    [StringLength(30, ErrorMessage = "Phone Number cannot exceed 30 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ContactEmailDto
{
    public Guid Id { get; set; }

    public string? Label { get; set; }

    public bool IsPrimary { get; set; }

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Email Address must be a valid email format.")]
    [StringLength(256, ErrorMessage = "Email Address cannot exceed 256 characters.")]
    public string EmailAddress { get; set; } = string.Empty;
}

public class ContactAddressDto
{
    public Guid Id { get; set; }

    public string? Label { get; set; }

    public bool IsPrimary { get; set; }

    [Required(ErrorMessage = "Address Line 1 is required.")]
    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? AddressLine4 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string? City { get; set; }

    [Required(ErrorMessage = "State is required.")]
    [StringLength(2, ErrorMessage = "State must be a 2-letter abbreviation.")]
    public string? State { get; set; }

    [Required(ErrorMessage = "Postal Code is required.")]
    public string? PostalCode { get; set; }

    public string Country { get; set; } = "USA";
}

// ─── Vehicle-Contact Association ────────────────────────────────────────────────

public class VehicleContactAssignmentDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string VehicleUnitNumber { get; set; } = string.Empty;
    public Guid ContactId { get; set; }
    public string ContactFullName { get; set; } = string.Empty;

    /// <summary>Driver | ResponsibleContact</summary>
    public string AssociationRole { get; set; } = "Driver";

    public bool IsPrimary { get; set; }
    public DateTime AssignedDate { get; set; }
}

// ─── Contact Summary for Dashboard KPIs ─────────────────────────────────────────

public class ContactKpiDto
{
    public int TotalContacts { get; set; }
    public int ActiveDrivers { get; set; }
    public int LicensesExpiringIn30Days { get; set; }
}
