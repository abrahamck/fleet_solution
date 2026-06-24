using System;

namespace appfleet_nexus_ui.Models;

public class DashboardKpiDto
{
    public DateTime CalculatedAt { get; set; }
    public VehicleKpiDto Inventory { get; set; } = new();
    public MaintenanceKpiDto Maintenance { get; set; } = new();
    public FuelKpiDto Fuel { get; set; } = new();
    public RegistrationKpiDto Registration { get; set; } = new();
    public DriverKpiDto Drivers { get; set; } = new();
}

public class VehicleKpiDto
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int MaintenanceCount { get; set; }
    public int InactiveCount { get; set; }
}

public class MaintenanceKpiDto
{
    public int InCompliance { get; set; }
    public int ComingDue { get; set; }
    public int OutOfCompliance { get; set; }
}

public class FuelKpiDto
{
    public decimal TotalCost { get; set; }
    public decimal AverageMpg { get; set; }
    public decimal CostPerMile { get; set; }
    public double PercentageChange { get; set; }
}

public class RegistrationKpiDto
{
    public int Active { get; set; }
    public int InProgress { get; set; }
    public int ExpiringSoon { get; set; }
    public int Expired { get; set; }
}

public class DriverKpiDto
{
    public int TotalDrivers { get; set; }
    public int ActiveDrivers { get; set; }
    public int ExpiredLicenses { get; set; }
    public int AverageSafetyScore { get; set; }
}
