using Agile360.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

[ApiController]
[Route("api/health")]
public class HealthCheckController : ControllerBase
{
    private const string Version = "1.0.0";

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthCheckResponse>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var response = new HealthCheckResponse("Healthy", Version, DateTimeOffset.UtcNow);

        return Ok(ApiResponse<HealthCheckResponse>.Ok(response));
    }
}

public record HealthCheckResponse(string Status, string Version, DateTimeOffset Timestamp);
