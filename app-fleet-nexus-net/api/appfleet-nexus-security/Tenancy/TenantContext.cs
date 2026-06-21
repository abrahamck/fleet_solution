using AppFleetNexus.Data.Tenancy;

namespace AppFleetNexus.Security.Tenancy;

/// <summary>
/// Scoped service that holds the current request's tenant and user context.
/// Populated by TenantMiddleware after JWT authentication.
/// Implements both ITenantContext (from security) and ITenantContextAccessor (from data)
/// to bridge the security→data layer dependency.
/// </summary>
public class TenantContext : ITenantContext, ITenantContextAccessor
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }

    // ITenantContextAccessor explicit implementation
    Guid ITenantContextAccessor.CurrentUserId => UserId;
    Guid ITenantContextAccessor.CurrentTenantId => TenantId;
}
