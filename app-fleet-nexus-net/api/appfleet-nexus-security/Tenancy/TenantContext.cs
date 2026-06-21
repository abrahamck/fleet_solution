namespace AppFleetNexus.Security.Tenancy;

public class TenantContext : ITenantContext
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
}
