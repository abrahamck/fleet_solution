using System;

namespace AppFleetNexus.Data.Models;

public class TenantUser
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Role { get; set; } = "Member";   // "Admin" or "Member"
    public bool IsSuperAdmin { get; set; }
    public DateTime JoinedDate { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
