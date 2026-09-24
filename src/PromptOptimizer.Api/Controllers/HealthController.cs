using Microsoft.AspNetCore.Mvc;

namespace PromptOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Check()
    {
        return Ok(new
        {
            status = "Healthy",
            framework = ".NET 10.0",
            timestamp = DateTime.UtcNow
        });
    }
}
