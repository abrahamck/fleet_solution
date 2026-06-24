using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AppFleetNexus.Data.Data;
using AppFleetNexus.Data.Models;

namespace AppFleetNexus.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/alpha")]
public class AlphaController : ControllerBase
{
    private readonly FleetNexusDbContext _dbContext;
    private readonly ILogger<AlphaController> _logger;

    public AlphaController(FleetNexusDbContext dbContext, ILogger<AlphaController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAlphaRequest request)
    {
        try
        {
            // 1. Validate Email
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@") || !request.Email.Contains("."))
            {
                return BadRequest(new { error = "Please enter a valid business email address." });
            }

            // 2. Validate Roadblocks
            if (request.Roadblocks == null || !request.Roadblocks.Any())
            {
                return BadRequest(new { error = "Please select at least one roadblock first!" });
            }

            // 3. Validate Custom Roadblock if "Other" is checked
            if (request.Roadblocks.Contains("Other") && string.IsNullOrWhiteSpace(request.CustomRoadblock))
            {
                return BadRequest(new { error = "Please specify your custom roadblock." });
            }

            // 4. Create and Save Registration
            var registration = new AlphaRegistration
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Roadblocks = request.Roadblocks.Select(r => r.Trim()).ToList(),
                CustomRoadblock = request.Roadblocks.Contains("Other") ? request.CustomRoadblock?.Trim() : null,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.AlphaRegistrations.Add(registration);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully registered alpha user: {Email}", registration.Email);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during alpha registration for email {Email}", request.Email);
            return StatusCode(500, new { error = "An internal server error occurred. Please try again later." });
        }
    }
}

public record RegisterAlphaRequest(
    string Email,
    List<string> Roadblocks,
    string? CustomRoadblock
);
