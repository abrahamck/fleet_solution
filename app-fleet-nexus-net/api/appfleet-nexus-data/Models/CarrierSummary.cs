namespace AppFleetNexus.Data.Models;

/// <summary>
/// Lightweight projection used for the top-carriers query.
/// Matches: SELECT dot_number, legal_name, phy_city, phy_state, nbr_power_unit
/// Property names match the Blazor UI's CarrierDto contract.
/// </summary>
public record CarrierSummary(
    long DotNumber,
    string? LegalName,
    string? City,
    string? State,
    int? NumberOfPowerUnits
);
