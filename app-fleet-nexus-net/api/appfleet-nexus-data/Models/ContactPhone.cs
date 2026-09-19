namespace AppFleetNexus.Data.Models;

/// <summary>
/// Polymorphic phone number record owned by a Contact or a Vehicle.
/// OwnerType: "Contact" | "Vehicle". (ADR-023)
/// </summary>
public class ContactPhone : BaseEntity
{
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }

    /// <summary>Primary | Mobile | Office | Emergency</summary>
    public string? Label { get; set; }

    public bool IsPrimary { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
}
