using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppFleetNexus.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Application health check endpoint.
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
