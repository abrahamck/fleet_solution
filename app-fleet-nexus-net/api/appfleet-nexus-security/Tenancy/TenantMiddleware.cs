using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AppFleetNexus.Data.Data;

namespace AppFleetNexus.Security.Tenancy;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, FleetNexusDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                              ?? context.User.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var tenantUser = await dbContext.TenantUsers
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(tu => tu.UserId == userId);

                if (tenantUser != null)
                {
                    tenantContext.UserId = userId;
                    tenantContext.Role = tenantUser.Role;
                    tenantContext.IsSuperAdmin = tenantUser.IsSuperAdmin;

                    // SuperAdmin impersonation support: check for X-Tenant-Id header
                    if (tenantUser.IsSuperAdmin && context.Request.Headers.TryGetValue("X-Tenant-Id", out var impersonatedTenantIdStr))
                    {
                        if (Guid.TryParse(impersonatedTenantIdStr, out var impersonatedTenantId))
                        {
                            tenantContext.TenantId = impersonatedTenantId;
                        }
                        else
                        {
                            tenantContext.TenantId = tenantUser.TenantId;
                        }
                    }
                    else
                    {
                        tenantContext.TenantId = tenantUser.TenantId;
                    }
                }
            }
        }

        await _next(context);
    }
}
