using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppFleetNexus.Security.Authentication;
using System.Threading.Tasks;
using System;

namespace AppFleetNexus.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISupabaseAuthService _authService;

    public AuthController(ISupabaseAuthService authService)
    {
        _authService = authService;
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
            var response = await _authService.SignInAsync(request);
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
}
