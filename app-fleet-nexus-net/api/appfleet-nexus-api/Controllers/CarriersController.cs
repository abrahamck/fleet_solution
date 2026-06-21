using Microsoft.AspNetCore.Mvc;
using appfleet_nexus_data.Repositories;

namespace appfleet_nexus_api.Controllers;

[ApiController]
[Route("api/carriers")]
public class CarriersController : ControllerBase
{
    private readonly ICarrierRepository _repository;
    private readonly ILogger<CarriersController> _logger;

    public CarriersController(ICarrierRepository repository, ILogger<CarriersController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get the top 100 carriers by number of power units.
    /// </summary>
    [HttpGet("top")]
    public async Task<IActionResult> GetTopCarriers()
    {
        try
        {
            var carriers = await _repository.GetTopCarriersByPowerUnitsAsync(100);
            return Ok(new { success = true, count = carriers.Count, data = carriers });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top carriers");
            return StatusCode(500, new { success = false, message = $"Error retrieving carriers: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get a carrier by DOT number.
    /// </summary>
    [HttpGet("{dotNumber}")]
    public async Task<IActionResult> GetCarrierByDotNumber(long dotNumber)
    {
        try
        {
            var carrier = await _repository.GetCarrierByDotNumberAsync(dotNumber);
            if (carrier is null)
                return NotFound(new { success = false, message = "Carrier not found" });

            return Ok(new { success = true, data = carrier });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving carrier with DOT number {DotNumber}", dotNumber);
            return StatusCode(500, new { success = false, message = $"Error retrieving carrier: {ex.Message}" });
        }
    }
}
