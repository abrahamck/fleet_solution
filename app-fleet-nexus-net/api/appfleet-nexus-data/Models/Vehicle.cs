namespace AppFleetNexus.Data.Models;

public class Vehicle : BaseEntity
{
    public string UnitNumber { get; set; } = string.Empty;
    public string? Vin { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicenseState { get; set; }
    public string? Type { get; set; }          // "Truck", "Trailer", etc.
    public string Status { get; set; } = "Active";  // "Active", "Inactive", "Maintenance"
}
