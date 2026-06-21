using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppFleetNexus.Security.Extensions;

public static class SecurityServiceExtensions
{
    /// <summary>
    /// Registers all FleetNexus security services: JWT auth, tenant context, Supabase client.
    /// </summary>
    public static IServiceCollection AddFleetNexusSecurity(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Phase 3 will fill this in
        return services;
    }

    /// <summary>
    /// Adds security middleware to the pipeline (must be called after UseAuthentication).
    /// </summary>
    public static IApplicationBuilder UseFleetNexusSecurity(this IApplicationBuilder app)
    {
        // Phase 3 will fill this in
        return app;
    }
}
