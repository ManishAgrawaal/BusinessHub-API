using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MTS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetDashboard()
    {
        return Ok(new
        {
            success = true,
            message = "Welcome to MTS Admin Dashboard.",
            userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            fullName = User.FindFirst(
                System.Security.Claims.ClaimTypes.Name)?.Value,
            email = User.FindFirst(
                System.Security.Claims.ClaimTypes.Email)?.Value,
            role = User.FindFirst(
                System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }
}