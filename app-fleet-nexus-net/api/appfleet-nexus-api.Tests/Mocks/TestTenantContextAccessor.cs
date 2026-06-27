using System;
using AppFleetNexus.Data.Tenancy;

namespace AppFleetNexus.Api.Tests.Mocks;

public class TestTenantContextAccessor : ITenantContextAccessor
{
    public Guid CurrentUserId { get; set; } = Guid.NewGuid();
    public Guid CurrentTenantId { get; set; } = Guid.NewGuid();
}
