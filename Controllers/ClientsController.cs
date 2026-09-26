using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTS_API.Data;
using MTS_API.DTOs;
using MTS_API.Models;

namespace MTS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly MtsDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public ClientsController(
        MtsDbContext context,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // =========================================================
    // POST: api/Clients
    // CREATE CLIENT - ADMIN ONLY
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateClient(
        [FromBody] CreateClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Full name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new
            {
                success = false,
                message = "Email is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                success = false,
                message = "Password is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Company name is required."
            });
        }

        var email = request.Email.Trim().ToLower();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Email.ToLower() == email);

        if (existingUser != null)
        {
            return Conflict(new
            {
                success = false,
                message = "A user with this email already exists."
            });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Role = "Client",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password
        );

        var client = new Client
        {
            User = user,
            CompanyName = request.CompanyName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            Country = request.Country?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Client created successfully.",
            clientId = client.ClientId,
            userId = user.UserId,
            fullName = user.FullName,
            email = user.Email,
            companyName = client.CompanyName,
            role = user.Role
        });
    }
    // =========================================================
    // GET: api/Clients
    // GET ALL CLIENTS - ADMIN ONLY
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllClients()
    {
        var clients = await _context.Clients
            .AsNoTracking()
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                clientId = x.ClientId,
                userId = x.UserId,

                fullName = x.User.FullName,
                email = x.User.Email,
                role = x.User.Role,
                isActive = x.User.IsActive,

                companyName = x.CompanyName,
                phoneNumber = x.PhoneNumber,
                address = x.Address,
                city = x.City,
                country = x.Country,

                createdAt = x.CreatedAt,
                updatedAt = x.User.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Clients loaded successfully.",
            count = clients.Count,
            clients
        });
    }
    // =========================================================
    // PUT: api/Clients/{clientId}
    // UPDATE CLIENT - ADMIN ONLY
    // =========================================================

    [HttpPut("{clientId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateClient(
        int clientId,
        [FromBody] UpdateClientByAdminRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Full name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Company name is required."
            });
        }

        var client = await _context.Clients
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.ClientId == clientId);

        if (client == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client not found."
            });
        }

        // =====================================================
        // UPDATE USER
        // =====================================================

        client.User.FullName =
            request.FullName.Trim();

        client.User.IsActive =
            request.IsActive;

        client.User.UpdatedAt =
            DateTime.UtcNow;

        // =====================================================
        // UPDATE CLIENT
        // =====================================================

        client.CompanyName =
            request.CompanyName.Trim();

        client.PhoneNumber =
            string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim();

        client.Address =
            string.IsNullOrWhiteSpace(request.Address)
                ? null
                : request.Address.Trim();

        client.City =
            string.IsNullOrWhiteSpace(request.City)
                ? null
                : request.City.Trim();

        client.Country =
            string.IsNullOrWhiteSpace(request.Country)
                ? null
                : request.Country.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Client updated successfully.",

            clientId = client.ClientId,
            userId = client.UserId,

            fullName = client.User.FullName,
            email = client.User.Email,

            companyName = client.CompanyName,
            phoneNumber = client.PhoneNumber,
            address = client.Address,
            city = client.City,
            country = client.Country,

            isActive = client.User.IsActive
        });
    }
    // =====================================================
    // PUT: api/Clients/{clientId}/password
    // RESET CLIENT PASSWORD - ADMIN ONLY
    // =====================================================

    [HttpPut("{clientId:int}/password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetClientPassword(
        int clientId,
        [FromBody] ResetClientPasswordRequest request)
    {
        if (clientId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ClientId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new
            {
                success = false,
                message = "New password is required."
            });
        }

        if (request.NewPassword.Trim().Length < 8)
        {
            return BadRequest(new
            {
                success = false,
                message = "Password must be at least 8 characters."
            });
        }

        var client = await _context.Clients
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.ClientId == clientId);

        if (client == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client not found."
            });
        }

        client.User.PasswordHash =
            _passwordHasher.HashPassword(
                client.User,
                request.NewPassword
            );

        client.User.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Client password updated successfully.",
            clientId = client.ClientId,
            userId = client.UserId,
            email = client.User.Email
        });
    }
    // =========================================================
    // GET: api/Clients/me
    // GET CURRENT CLIENT PROFILE
    // =========================================================

    [HttpGet("me")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized(new
            {
                success = false,
                message = "User identity not found."
            });
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        var client = await _context.Clients
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId);

        if (client == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client profile not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Client profile loaded successfully.",

            userId = client.UserId,
            clientId = client.ClientId,

            fullName = client.User.FullName,
            email = client.User.Email,

            companyName = client.CompanyName,
            phoneNumber = client.PhoneNumber,
            address = client.Address,
            city = client.City,
            country = client.Country
        });
    }

    // =========================================================
    // PUT: api/Clients/me
    // UPDATE CURRENT CLIENT PROFILE
    // =========================================================

    [HttpPut("me")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateClientProfileRequest request)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized(new
            {
                success = false,
                message = "User identity not found."
            });
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Full name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Company name is required."
            });
        }

        var client = await _context.Clients
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId);

        if (client == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client profile not found."
            });
        }

        // =====================================================
        // UPDATE USER
        // =====================================================

        client.User.FullName =
            request.FullName.Trim();

        client.User.UpdatedAt =
            DateTime.UtcNow;

        // =====================================================
        // UPDATE CLIENT
        // =====================================================

        client.CompanyName =
            request.CompanyName.Trim();

        client.PhoneNumber =
            string.IsNullOrWhiteSpace(
                request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim();

        client.Address =
            string.IsNullOrWhiteSpace(
                request.Address)
                ? null
                : request.Address.Trim();

        client.City =
            string.IsNullOrWhiteSpace(
                request.City)
                ? null
                : request.City.Trim();

        client.Country =
            string.IsNullOrWhiteSpace(
                request.Country)
                ? null
                : request.Country.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Client profile updated successfully.",

            userId = client.UserId,
            clientId = client.ClientId,

            fullName = client.User.FullName,
            email = client.User.Email,

            companyName = client.CompanyName,
            phoneNumber = client.PhoneNumber,
            address = client.Address,
            city = client.City,
            country = client.Country
        });
    }
}