using Microsoft.AspNetCore.Mvc;

namespace CloudAppServer.Controllers;

[ApiController]
[Route("/api/v1/health/")]
public class HealthController : ControllerBase
{
    [HttpGet("check")]
    public IActionResult Check()
    {
        return Ok("OK");
    }
}