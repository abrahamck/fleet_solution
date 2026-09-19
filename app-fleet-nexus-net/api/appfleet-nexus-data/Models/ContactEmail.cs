namespace AppFleetNexus.Data.Models;

/// <summary>
/// Polymorphic email address record owned by a Contact or a Vehicle.
/// OwnerType: "Contact" | "Vehicle". (ADR-023)
/// </summary>
public class ContactEmail : BaseEntity
{
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }

    /// <summary>Primary | Work | Billing</summary>
    public string? Label { get; set; }

    public bool IsPrimary { get; set; }

    public string EmailAddress { get; set; } = string.Empty;
}
