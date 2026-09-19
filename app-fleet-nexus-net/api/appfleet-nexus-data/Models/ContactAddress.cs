namespace AppFleetNexus.Data.Models;

/// <summary>
/// Polymorphic physical/mailing address owned by a Contact or a Vehicle.
/// OwnerType: "Contact" | "Vehicle". (ADR-023)
/// </summary>
public class ContactAddress : BaseEntity
{
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }

    /// <summary>Home | Mailing | Garage | Billing</summary>
    public string? Label { get; set; }

    public bool IsPrimary { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? AddressLine4 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "USA";
}
