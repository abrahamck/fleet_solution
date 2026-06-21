using System;
using System.Collections.Generic;

namespace appfleet_nexus_data.Models;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
}
