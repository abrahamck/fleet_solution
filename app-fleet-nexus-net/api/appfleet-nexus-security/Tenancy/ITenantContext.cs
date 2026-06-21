namespace AppFleetNexus.Security.Tenancy;

/// <summary>
/// Provides the current request's tenant and user context.
/// Populated by TenantMiddleware after JWT authentication.
/// </summary>
public interface ITenantContext
{
    Guid UserId { get; }
    Guid TenantId { get; }
    string Role { get; }
    bool IsSuperAdmin { get; }
}
