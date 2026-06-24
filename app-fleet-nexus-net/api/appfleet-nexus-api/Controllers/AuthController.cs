using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppFleetNexus.Security.Authentication;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using AppFleetNexus.Data.Data;
using AppFleetNexus.Data.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using AppFleetNexus.Data.Tenancy;
using AppFleetNexus.Security.Tenancy;

namespace AppFleetNexus.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISupabaseAuthService _authService;
    private readonly FleetNexusDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ITenantContextAccessor _tenantAccessor;

    public AuthController(
        ISupabaseAuthService authService, 
        FleetNexusDbContext dbContext, 
        IMemoryCache cache, 
        ITenantContextAccessor tenantAccessor)
    {
        _authService = authService;
        _dbContext = dbContext;
        _cache = cache;
        _tenantAccessor = tenantAccessor;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignupRequest request)
    {
        try
        {
            var response = await _authService.SignUpAsync(request);
            return Ok(response);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.BadRequest), new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            AuthResponse response;
            if (request.Email.Equals("testuser@demo.com", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    response = await _authService.SignInAsync(request);
                }
                catch (HttpRequestException)
                {
                    // Check if the user is already in our local database
                    var userExistsLocally = await _dbContext.Users.IgnoreQueryFilters()
                        .AnyAsync(u => u.Email == "testuser@demo.com");

                    if (userExistsLocally)
                    {
                        // User exists; login failed because the password was incorrect. Propagate the error.
                        throw;
                    }

                    // User does not exist locally (e.g. brand new deploy); attempt signup
                    var signupRequest = new SignupRequest(
                        "testuser@demo.com",
                        request.Password,
                        "Demo",
                        "User"
                    );
                    response = await _authService.SignUpAsync(signupRequest);
                }

                await EnsureDemoDataSetupAsync(response.UserId);
            }
            else
            {
                response = await _authService.SignInAsync(request);
            }

            return Ok(response);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.BadRequest), new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.BadRequest), new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT is stateless; client-side token discard is standard.
        return Ok(new { message = "Logged out successfully" });
    }

    private async Task EnsureDemoDataSetupAsync(Guid userId)
    {
        var demoTenantId = Guid.Parse("abc00000-0000-0000-0000-000000000000");

        // 0. Establish the tenant context programmatically so DB auditing inherits the correct TenantId
        if (_tenantAccessor is TenantContext tc)
        {
            tc.TenantId = demoTenantId;
            tc.UserId = userId;
        }

        // 1. Ensure the Tenant exists
        var tenant = await _dbContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == demoTenantId);
        if (tenant == null)
        {
            tenant = new Tenant
            {
                Id = demoTenantId,
                Name = "ABC Heating and Cooling",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _dbContext.Tenants.Add(tenant);
        }

        // 2. Ensure the User exists in public.users
        var user = await _dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            user = new User
            {
                Id = userId,
                Email = "testuser@demo.com",
                FirstName = "Demo",
                LastName = "User",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _dbContext.Users.Add(user);
        }

        // 3. Ensure the TenantUser link exists (checking by UserId to satisfy the unique constraint IX_tenant_users_UserId)
        var tenantUser = await _dbContext.TenantUsers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(tu => tu.UserId == userId);
        if (tenantUser == null)
        {
            tenantUser = new TenantUser
            {
                UserId = userId,
                TenantId = demoTenantId,
                Role = "Admin",
                IsSuperAdmin = false,
                JoinedDate = DateTime.UtcNow
            };
            _dbContext.TenantUsers.Add(tenantUser);
        }
        else if (tenantUser.TenantId != demoTenantId)
        {
            tenantUser.TenantId = demoTenantId;
            tenantUser.Role = "Admin";
        }

        await _dbContext.SaveChangesAsync();

        // 4. Ensure demo vehicles exist for this tenant
        var hasVehicles = await _dbContext.Vehicles.IgnoreQueryFilters().AnyAsync(v => v.TenantId == demoTenantId && !v.IsDeleted);
        if (!hasVehicles)
        {
            var vehicles = new List<Vehicle>
            {
                new() { TenantId = demoTenantId, UnitNumber = "201", Make = "Chevrolet", Model = "Express 2500", Year = 2021, LicensePlate = "TX-V1234A", LicenseState = "TX", Type = "Van", Status = "Active", CreatedDate = DateTime.UtcNow, CreatedBy = userId },
                new() { TenantId = demoTenantId, UnitNumber = "202", Make = "Ford", Model = "Transit-250", Year = 2022, LicensePlate = "TX-V5678B", LicenseState = "TX", Type = "Van", Status = "Active", CreatedDate = DateTime.UtcNow, CreatedBy = userId },
                new() { TenantId = demoTenantId, UnitNumber = "203", Make = "Chevrolet", Model = "Express 2500", Year = 2020, LicensePlate = "TX-V9012C", LicenseState = "TX", Type = "Van", Status = "Active", CreatedDate = DateTime.UtcNow, CreatedBy = userId },
                new() { TenantId = demoTenantId, UnitNumber = "204", Make = "Ford", Model = "Transit-350", Year = 2023, LicensePlate = "TX-V3456D", LicenseState = "TX", Type = "Van", Status = "Maintenance", CreatedDate = DateTime.UtcNow, CreatedBy = userId },
                new() { TenantId = demoTenantId, UnitNumber = "301", Make = "Ford", Model = "F-150", Year = 2019, LicensePlate = "TX-P7890E", LicenseState = "TX", Type = "Truck", Status = "Active", CreatedDate = DateTime.UtcNow, CreatedBy = userId },
                new() { TenantId = demoTenantId, UnitNumber = "302", Make = "Ram", Model = "1500", Year = 2020, LicensePlate = "TX-P1234F", LicenseState = "TX", Type = "Truck", Status = "Active", CreatedDate = DateTime.UtcNow, CreatedBy = userId },
                new() { TenantId = demoTenantId, UnitNumber = "101", Make = "Ford", Model = "E-350", Year = 2018, LicensePlate = "TX-B5678G", LicenseState = "TX", Type = "Truck", Status = "Inactive", CreatedDate = DateTime.UtcNow, CreatedBy = userId },
                new() { TenantId = demoTenantId, UnitNumber = "205", Make = "Ford", Model = "Transit-250", Year = 2022, LicensePlate = "TX-V9012H", LicenseState = "TX", Type = "Van", Status = "Maintenance", CreatedDate = DateTime.UtcNow, CreatedBy = userId }
            };

            foreach (var vehicle in vehicles)
            {
                _dbContext.Vehicles.Add(vehicle);
            }

            await _dbContext.SaveChangesAsync();
        }

        // 5. Invalidate cache for the demo tenant
        _cache.Remove($"dashboard_kpis_{demoTenantId}");
    }
}
