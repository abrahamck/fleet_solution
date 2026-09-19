using System;

namespace AppFleetNexus.Data.Models;

/// <summary>
/// Many-to-many join entity between Vehicle and Contact with role metadata. (ADR-022)
/// </summary>
public class VehicleContact : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Guid ContactId { get; set; }

    /// <summary>Driver | ResponsibleContact</summary>
    public string AssociationRole { get; set; } = "Driver";

    public bool IsPrimary { get; set; }
    public DateTime AssignedDate { get; set; }

    // Navigation
    public Vehicle Vehicle { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
}
