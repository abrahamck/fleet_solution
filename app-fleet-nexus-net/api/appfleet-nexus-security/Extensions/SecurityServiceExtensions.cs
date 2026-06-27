using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using AppFleetNexus.Security.Authentication;
using AppFleetNexus.Security.Tenancy;
using AppFleetNexus.Data.Tenancy;
using AppFleetNexus.Security.Authorization;
using AppFleetNexus.Security.Middleware;

namespace AppFleetNexus.Security.Extensions;

public static class SecurityServiceExtensions
{
    /// <summary>
    /// Registers all FleetNexus security services: JWT auth, tenant context, Supabase client.
    /// </summary>
    public static IServiceCollection AddFleetNexusSecurity(
        this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Bind SupabaseOptions from config
        services.Configure<SupabaseOptions>(configuration.GetSection(SupabaseOptions.SectionName));

        // 2. Register JWT Bearer authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var supabaseSection = configuration.GetSection(SupabaseOptions.SectionName);
                var url = supabaseSection["Url"] ?? "https://placeholder.supabase.co";
                
                // Normalize URL: remove trailing slashes and any trailing /auth/v1 or /rest/v1
                var baseUrl = url.TrimEnd('/');
                if (baseUrl.EndsWith("/auth/v1", StringComparison.OrdinalIgnoreCase))
                {
                    baseUrl = baseUrl.Substring(0, baseUrl.Length - "/auth/v1".Length).TrimEnd('/');
                }
                else if (baseUrl.EndsWith("/rest/v1", StringComparison.OrdinalIgnoreCase))
                {
                    baseUrl = baseUrl.Substring(0, baseUrl.Length - "/rest/v1".Length).TrimEnd('/');
                }

                // Configure OpenID Connect discovery endpoint to automatically fetch public keys (JWKS) for ES256
                options.MetadataAddress = $"{baseUrl}/auth/v1/.well-known/openid-configuration";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"{baseUrl}/auth/v1",
                    ValidateAudience = true,
                    ValidAudience = "authenticated",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAlgorithms = new[] { SecurityAlgorithms.EcdsaSha256 }
                };
            });

        // 3. Default-deny authorization policy + SuperAdmin policy
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("SuperAdmin", policy =>
                policy.Requirements.Add(new SuperAdminRequirement()));
        });

        // 4. Register scoped TenantContext (implements both interfaces)
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContextAccessor>(sp => sp.GetRequiredService<TenantContext>());

        // Register SuperAdmin authorization handler
        services.AddScoped<IAuthorizationHandler, SuperAdminHandler>();

        // 5. Register RLS interceptor
        services.AddScoped<RlsConnectionInterceptor>();

        // 6. Register Supabase auth service
        services.AddHttpClient<ISupabaseAuthService, SupabaseAuthService>();

        return services;
    }

    /// <summary>
    /// Adds security middleware to the pipeline (must be called after UseRouting).
    /// </summary>
    public static IApplicationBuilder UseFleetNexusSecurity(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<TenantMiddleware>();
        return app;
    }
}
