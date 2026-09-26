using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTS_API.Data;
using MTS_API.Models;

namespace MTS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminSetupController : ControllerBase
{
    private readonly MtsDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AdminSetupController(
        MtsDbContext context,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdmin()
    {
        var email = "admin@mts.com";

        var existingUser = _context.Users
            .FirstOrDefault(x => x.Email == email);

        if (existingUser != null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Admin user already exists."
            });
        }

        var user = new User
        {
            FullName = "MTS Admin",
            Email = email,
            Role = "Admin",
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            "Admin@123"
        );

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Admin user created successfully.",
            email = user.Email
        });
    }
}