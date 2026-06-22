using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppFleetNexus.Data.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppFleetNexus.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly FleetNexusDbContext _dbContext;

    public AdminController(FleetNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// GET /api/admin/tenants
    /// List all tenants in the system.
    /// </summary>
    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants()
    {
        try
        {
            var tenants = await _dbContext.Tenants
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.CreatedDate,
                    t.IsActive
                })
                .ToListAsync();

            return Ok(tenants);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving tenants: " + ex.Message });
        }
    }

    /// <summary>
    /// GET /api/admin/tenants/{id}
    /// Get details of a specific tenant including user count.
    /// </summary>
    [HttpGet("tenants/{id}")]
    public async Task<IActionResult> GetTenant(Guid id)
    {
        try
        {
            var tenant = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
            {
                return NotFound(new { message = "Tenant not found" });
            }

            var userCount = await _dbContext.TenantUsers
                .CountAsync(tu => tu.TenantId == id);

            return Ok(new
            {
                tenant.Id,
                tenant.Name,
                tenant.CreatedDate,
                tenant.IsActive,
                UserCount = userCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving tenant details: " + ex.Message });
        }
    }

    /// <summary>
    /// GET /api/admin/users
    /// List all users across tenants.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _dbContext.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.CreatedDate,
                    u.IsDeleted
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving users: " + ex.Message });
        }
    }
}
