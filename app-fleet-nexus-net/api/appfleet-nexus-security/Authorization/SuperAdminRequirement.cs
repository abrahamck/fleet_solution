using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using AppFleetNexus.Security.Tenancy;

namespace AppFleetNexus.Security.Authorization;

public class SuperAdminRequirement : IAuthorizationRequirement
{
}

public class SuperAdminHandler : AuthorizationHandler<SuperAdminRequirement>
{
    private readonly ITenantContext _tenantContext;

    public SuperAdminHandler(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, SuperAdminRequirement requirement)
    {
        if (_tenantContext.IsSuperAdmin)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
