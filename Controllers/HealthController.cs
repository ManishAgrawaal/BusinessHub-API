using Microsoft.AspNetCore.Mvc;

namespace MTS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            success = true,
            message = "MTS API is running successfully.",
            application = "Manish Technology Solution"
        });
    }
}