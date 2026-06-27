using System;
using System.ComponentModel.DataAnnotations;

namespace appfleet_nexus_ui.Models;

public class VehicleDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Unit Number is required.")]
    [StringLength(50, ErrorMessage = "Unit Number cannot exceed 50 characters.")]
    public string UnitNumber { get; set; } = string.Empty;
    
    public string? Vin { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    
    [Range(1900, 2100, ErrorMessage = "Please enter a valid year.")]
    public int? Year { get; set; }
    
    public string? LicensePlate { get; set; }
    
    [StringLength(2, ErrorMessage = "State must be a 2-letter abbreviation.")]
    public string? LicenseState { get; set; }
    
    public string? Type { get; set; }
    public string Status { get; set; } = "Active";
}
