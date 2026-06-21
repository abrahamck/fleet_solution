using System;

namespace AppFleetNexus.Data.Tenancy;

/// <summary>
/// Provides the current tenant and user IDs to the data layer.
/// Implemented by the security library's TenantContext.
/// </summary>
public interface ITenantContextAccessor
{
    Guid CurrentUserId { get; }
    Guid CurrentTenantId { get; }
}
