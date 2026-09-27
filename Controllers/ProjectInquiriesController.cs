using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTS_API.Data;
using MTS_API.DTOs;
using MTS_API.Models;
using MTS_API.Services;

namespace MTS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectInquiriesController : ControllerBase
{
    private readonly MtsDbContext _context;
    private readonly IEmailService _emailService;

    public ProjectInquiriesController(
        MtsDbContext context,
        IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }
    // =========================================================
    // CREATE PUBLIC INQUIRY - WEBSITE
    // =========================================================

    [HttpPost("public")]
    [AllowAnonymous]
    public async Task<IActionResult> CreatePublicInquiry(
        [FromBody] CreatePublicProjectInquiryRequest request)
    {
        // =========================================================
        // VALIDATION
        // =========================================================

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
                message = "Email address is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            return BadRequest(new
            {
                success = false,
                message = "Phone number is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Service))
        {
            return BadRequest(new
            {
                success = false,
                message = "Service is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new
            {
                success = false,
                message = "Project details are required."
            });
        }

        // =========================================================
        // CREATE PUBLIC INQUIRY
        // =========================================================

        var inquiry = new ProjectInquiry
        {
            ClientId = null,

            ContactName =
                request.FullName.Trim(),

            ContactEmail =
                request.Email.Trim(),

            ContactPhone =
                request.Phone.Trim(),

            CompanyName =
                string.IsNullOrWhiteSpace(request.Company)
                    ? null
                    : request.Company.Trim(),

            ServiceRequired =
                request.Service.Trim(),

            ProjectName =
    $"{request.Service.Trim()} Inquiry",

            ProjectType =
    request.Service.Trim(),

            Budget =
                string.IsNullOrWhiteSpace(request.Budget)
                    ? null
                    : request.Budget.Trim(),

            Description =
                request.Message.Trim(),

            Requirements =
                request.Message.Trim(),

            Technologies = null,

            AdditionalRequirements = null,

            Status = "New",

            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectInquiries.Add(inquiry);

        await _context.SaveChangesAsync();
        try
        {
            await _emailService.SendProjectInquiryEmailsAsync(
                inquiry.ContactName ?? request.FullName.Trim(),
                inquiry.ContactEmail ?? request.Email.Trim(),
                inquiry.ContactPhone ?? request.Phone.Trim(),
                inquiry.CompanyName,
                inquiry.ServiceRequired ?? request.Service.Trim(),
                inquiry.Budget,
                inquiry.Description ?? request.Message.Trim()
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Project inquiry email failed: {ex.Message}");
        }

        // =========================================================
        // RESPONSE
        // =========================================================

        return Ok(new
        {
            success = true,

            message =
                "Thank you! Your project inquiry has been submitted successfully.",

            projectInquiryId =
                inquiry.ProjectInquiryId,

            status =
                inquiry.Status,

            createdAt =
                inquiry.CreatedAt
        });
    }

    // =========================================================
    // CREATE INQUIRY - CLIENT
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> CreateInquiry(
     [FromBody] CreateProjectInquiryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Project name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.ProjectType))
        {
            return BadRequest(new
            {
                success = false,
                message = "Project type is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new
            {
                success = false,
                message = "Project description is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Requirements))
        {
            return BadRequest(new
            {
                success = false,
                message = "Business requirements are required."
            });
        }

        if (request.DeliveryDate.HasValue &&
            request.StartDate.HasValue &&
            request.DeliveryDate.Value < request.StartDate.Value)
        {
            return BadRequest(new
            {
                success = false,
                message = "Delivery date cannot be before start date."
            });
        }

        // =========================================================
        // GET USER ID FROM JWT
        // =========================================================

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

        if (!int.TryParse(
            userIdClaim,
            out var userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        // =========================================================
        // GET CLIENT
        // =========================================================

        var client = await _context.Clients
            .FirstOrDefaultAsync(
                x => x.UserId == userId);

        if (client == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client profile not found."
            });
        }

        // =========================================================
        // TECHNOLOGIES
        // =========================================================

        var technologies =
            request.Technologies != null &&
            request.Technologies.Count > 0
                ? string.Join(
                    ", ",
                    request.Technologies)
                : null;

        // =========================================================
        // 1. CREATE PROJECT INQUIRY
        // =========================================================

        var inquiry = new ProjectInquiry
        {
            ClientId = client.ClientId,

            ProjectName =
                request.ProjectName.Trim(),

            ProjectType =
                request.ProjectType.Trim(),

            Budget =
                string.IsNullOrWhiteSpace(
                    request.Budget)
                    ? null
                    : request.Budget.Trim(),

            StartDate =
                request.StartDate,

            DeliveryDate =
                request.DeliveryDate,

            Description =
                request.Description.Trim(),

            Requirements =
                request.Requirements.Trim(),

            Technologies =
                technologies,

            AdditionalRequirements =
                string.IsNullOrWhiteSpace(
                    request.AdditionalRequirements)
                    ? null
                    : request.AdditionalRequirements.Trim(),

            Status = "New",

            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectInquiries.Add(inquiry);

        await _context.SaveChangesAsync();

        // =========================================================
        // 2. CREATE PROJECT
        // =========================================================

        var project = new Project
        {
            ClientId = client.ClientId,

            ProjectName =
                request.ProjectName.Trim(),

            Description =
                request.Description.Trim(),

            Status = "Planning",

            ProjectAmount = null,

            StartDate =
                request.StartDate,

            ExpectedEndDate =
                request.DeliveryDate,

            CompletedDate = null,

            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);

        await _context.SaveChangesAsync();

        // =========================================================
        // RESPONSE
        // =========================================================

        return Ok(new
        {
            success = true,

            message =
                "Project inquiry submitted and project created successfully.",

            projectInquiryId =
                inquiry.ProjectInquiryId,

            projectId =
                project.ProjectId,

            clientId =
                client.ClientId,

            projectName =
                project.ProjectName,

            status =
                project.Status,

            createdAt =
                project.CreatedAt
        });
    }

    // =========================================================
    // CLIENT - MY INQUIRIES
    // =========================================================

    [HttpGet("my-inquiries")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyInquiries()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) ||
            !int.TryParse(
                userIdClaim,
                out var userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId);

        if (client == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client profile not found."
            });
        }

        var inquiries = await _context.ProjectInquiries
            .AsNoTracking()
            .Where(x =>
                x.ClientId == client.ClientId)
            .OrderByDescending(
                x => x.CreatedAt)
            .Select(x => new
            {
                projectInquiryId =
                    x.ProjectInquiryId,

                projectName =
                    x.ProjectName,

                projectType =
                    x.ProjectType,

                budget =
                    x.Budget,

                startDate =
                    x.StartDate,

                deliveryDate =
                    x.DeliveryDate,

                description =
                    x.Description,

                requirements =
                    x.Requirements,

                technologies =
                    x.Technologies,

                additionalRequirements =
                    x.AdditionalRequirements,

                status =
                    x.Status,

                createdAt =
                    x.CreatedAt,

                updatedAt =
                    x.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            inquiries
        });
    }

    // =========================================================
    // ADMIN - ALL INQUIRIES
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllInquiries()
    {
        var inquiries =
            await _context.ProjectInquiries
                .AsNoTracking()
                .Include(x => x.Client)
                .ThenInclude(x => x.User)
                .OrderByDescending(
                    x => x.CreatedAt)
                .Select(x => new
                {
                    projectInquiryId =
        x.ProjectInquiryId,

                    clientId =
        x.ClientId,

                    clientName =
        x.Client != null
            ? x.Client.User.FullName
            : x.ContactName,

                    clientEmail =
        x.Client != null
            ? x.Client.User.Email
            : x.ContactEmail,

                    companyName =
        x.Client != null
            ? x.Client.CompanyName
            : x.CompanyName,

                    contactPhone =
        x.ContactPhone,

                    serviceRequired =
        x.ServiceRequired,

                    projectName =
        x.ProjectName,

                    projectType =
        x.ProjectType,

                    budget =
        x.Budget,

                    startDate =
        x.StartDate,

                    deliveryDate =
        x.DeliveryDate,

                    description =
        x.Description,

                    requirements =
        x.Requirements,

                    technologies =
        x.Technologies,

                    additionalRequirements =
        x.AdditionalRequirements,

                    status =
        x.Status,

                    createdAt =
        x.CreatedAt,

                    updatedAt =
        x.UpdatedAt
                })
                .ToListAsync();

        return Ok(new
        {
            success = true,
            inquiries
        });
    }

    // =========================================================
    // ADMIN - GET SINGLE INQUIRY
    // =========================================================

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetInquiry(
        int id)
    {
        var inquiry =
            await _context.ProjectInquiries
                .AsNoTracking()
                .Include(x => x.Client)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.ProjectInquiryId == id);

        if (inquiry == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Project inquiry not found."
            });
        }

        return Ok(new
        {
            success = true,

            projectInquiryId =
                inquiry.ProjectInquiryId,

            clientId =
                inquiry.ClientId,

            clientName =
                inquiry.Client.User.FullName,

            clientEmail =
                inquiry.Client.User.Email,

            companyName =
                inquiry.Client.CompanyName,

            projectName =
                inquiry.ProjectName,

            projectType =
                inquiry.ProjectType,

            budget =
                inquiry.Budget,

            startDate =
                inquiry.StartDate,

            deliveryDate =
                inquiry.DeliveryDate,

            description =
                inquiry.Description,

            requirements =
                inquiry.Requirements,

            technologies =
                inquiry.Technologies,

            additionalRequirements =
                inquiry.AdditionalRequirements,

            status =
                inquiry.Status,

            createdAt =
                inquiry.CreatedAt,

            updatedAt =
                inquiry.UpdatedAt
        });
    }
    // ============================================================
    // APPROVE PROJECT INQUIRY - ADMIN ONLY
    // ============================================================

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveInquiry(int id)
    {
        var inquiry = await _context.ProjectInquiries
            .FirstOrDefaultAsync(x =>
                x.ProjectInquiryId == id);

        if (inquiry == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Project inquiry not found."
            });
        }

        if (inquiry.Status == "Approved")
        {
            return BadRequest(new
            {
                success = false,
                message = "This inquiry is already approved."
            });
        }

        if (inquiry.Status == "Rejected")
        {
            return BadRequest(new
            {
                success = false,
                message = "A rejected inquiry cannot be approved."
            });
        }

        inquiry.Status = "Approved";
        inquiry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Project inquiry approved successfully.",
            projectInquiryId = inquiry.ProjectInquiryId,
            status = inquiry.Status,
            updatedAt = inquiry.UpdatedAt
        });
    }


    // ============================================================
    // REJECT PROJECT INQUIRY - ADMIN ONLY
    // ============================================================

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectInquiry(int id)
    {
        var inquiry = await _context.ProjectInquiries
            .FirstOrDefaultAsync(x =>
                x.ProjectInquiryId == id);

        if (inquiry == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Project inquiry not found."
            });
        }

        if (inquiry.Status == "Rejected")
        {
            return BadRequest(new
            {
                success = false,
                message = "This inquiry is already rejected."
            });
        }

        if (inquiry.Status == "Approved")
        {
            return BadRequest(new
            {
                success = false,
                message = "An approved inquiry cannot be rejected."
            });
        }

        inquiry.Status = "Rejected";
        inquiry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Project inquiry rejected successfully.",
            projectInquiryId = inquiry.ProjectInquiryId,
            status = inquiry.Status,
            updatedAt = inquiry.UpdatedAt
        });
    }
}