using Microsoft.AspNetCore.Http;

namespace AppFleetNexus.Security.Tenancy;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        // Phase 3 will fill this in
        await _next(context);
    }
}
