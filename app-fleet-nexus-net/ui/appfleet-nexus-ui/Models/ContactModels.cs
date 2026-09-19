using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace appfleet_nexus_ui.Models;

public class ContactSummaryDto
{
    public Guid Id { get; set; }
    public string? UniqueId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string ContactType { get; set; } = "Other";
    public string Status { get; set; } = "Active";
    public string? PrimaryPhone { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryAddress { get; set; }
    public int AssignedVehicleCount { get; set; }
}

public class ContactDetailDto : ContactSummaryDto
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? JobTitle { get; set; }
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

public class ContactUpsertRequest
{
    public string? UniqueId { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Middle name cannot exceed 100 characters.")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact type is required.")]
    public string ContactType { get; set; } = "Driver";

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = "Active";

    [StringLength(100)]
    public string? JobTitle { get; set; }

    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    [StringLength(2)]
    public string? LicenseState { get; set; }

    public DateOnly? LicenseExpirationDate { get; set; }
    public DateOnly? MedicalCertExpirationDate { get; set; }

    [StringLength(100)]
    public string? EmergencyContactName { get; set; }

    [StringLength(30)]
    public string? EmergencyContactPhone { get; set; }

    public string? Notes { get; set; }

    public List<ContactPhoneDto> Phones { get; set; } = new();
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactAddressDto> Addresses { get; set; } = new();
    public List<VehicleContactAssignmentDto> VehicleAssignments { get; set; } = new();
}

public class ContactPhoneDto
{
    public Guid Id { get; set; }
    public string? Label { get; set; } = "Mobile";
    public bool IsPrimary { get; set; } = false;

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ContactEmailDto
{
    public Guid Id { get; set; }
    public string? Label { get; set; } = "Work";
    public bool IsPrimary { get; set; } = false;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(200, ErrorMessage = "Email address cannot exceed 200 characters.")]
    public string EmailAddress { get; set; } = string.Empty;
}

public class ContactAddressDto
{
    public Guid Id { get; set; }
    public string? Label { get; set; } = "Home";
    public bool IsPrimary { get; set; } = false;

    [Required(ErrorMessage = "Street address is required.")]
    [StringLength(200)]
    public string? AddressLine1 { get; set; }

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(200)]
    public string? AddressLine3 { get; set; }

    [StringLength(200)]
    public string? AddressLine4 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100)]
    public string? City { get; set; }

    [Required(ErrorMessage = "State is required.")]
    [StringLength(50)]
    public string? State { get; set; }

    [Required(ErrorMessage = "Postal code is required.")]
    [StringLength(20)]
    public string? PostalCode { get; set; }

    public string Country { get; set; } = "USA";
}

public class VehicleContactAssignmentDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleUnitNumber { get; set; }
    public Guid ContactId { get; set; }
    public string? ContactFullName { get; set; }
    public string AssociationRole { get; set; } = "Driver";
    public bool IsPrimary { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
}

public class ContactKpiDto
{
    public int TotalContacts { get; set; }
    public int ActiveDrivers { get; set; }
    public int LicensesExpiringIn30Days { get; set; }
}
