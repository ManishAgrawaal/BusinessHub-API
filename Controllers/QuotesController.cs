using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTS_API.Data;
using MTS_API.DTOs;
using MTS_API.Models;
using System.Security.Claims;

namespace MTS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotesController : ControllerBase
{
    private readonly MtsDbContext _context;

    public QuotesController(MtsDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // CREATE QUOTE - ADMIN ONLY
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuote(
        [FromBody] CreateQuoteRequest request)
    {
        if (request.ClientId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ClientId is required."
            });
        }

        if (request.ProjectId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ProjectId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.QuoteNumber))
        {
            return BadRequest(new
            {
                success = false,
                message = "Quote number is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new
            {
                success = false,
                message = "Quote title is required."
            });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Quote amount must be greater than zero."
            });
        }

        var clientExists = await _context.Clients
            .AsNoTracking()
            .AnyAsync(x => x.ClientId == request.ClientId);

        if (!clientExists)
        {
            return NotFound(new
            {
                success = false,
                message = "Client not found."
            });
        }

        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ProjectId == request.ProjectId &&
                x.ClientId == request.ClientId);

        if (project == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Project not found for the specified client."
            });
        }

        var quoteNumberExists = await _context.Quotes
            .AnyAsync(x => x.QuoteNumber == request.QuoteNumber.Trim());

        if (quoteNumberExists)
        {
            return Conflict(new
            {
                success = false,
                message = "A quote with this quote number already exists."
            });
        }

        var quote = new Quote
        {
            ClientId = request.ClientId,
            ProjectId = request.ProjectId,
            QuoteNumber = request.QuoteNumber.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Amount = request.Amount,
            Status = "Draft",
            ValidUntil = request.ValidUntil,
            CreatedAt = DateTime.UtcNow
        };

        _context.Quotes.Add(quote);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Quote created successfully.",
            quoteId = quote.QuoteId,
            quoteNumber = quote.QuoteNumber,
            clientId = quote.ClientId,
            projectId = quote.ProjectId,
            title = quote.Title,
            amount = quote.Amount,
            status = quote.Status,
            validUntil = quote.ValidUntil
        });

    }
    // =========================================================
    // GET MY QUOTES - CLIENT ONLY
    // =========================================================

    [HttpGet("my-quotes")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyQuotes()
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
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client profile not found."
            });
        }

        var quotes = await _context.Quotes
            .AsNoTracking()
            .Where(x => x.ClientId == client.ClientId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                quoteId = x.QuoteId,
                quoteNumber = x.QuoteNumber,
                projectId = x.ProjectId,
                title = x.Title,
                description = x.Description,
                amount = x.Amount,
                status = x.Status,
                validUntil = x.ValidUntil,
                createdAt = x.CreatedAt,
                updatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            clientId = client.ClientId,
            quotes
        });
    }
    // =========================================================
    // GET MY QUOTE BY ID - CLIENT ONLY
    // =========================================================

    [HttpGet("my-quotes/{id:int}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyQuote(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid QuoteId is required."
            });
        }

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

        var clientId = await _context.Clients
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => (int?)x.ClientId)
            .FirstOrDefaultAsync();

        if (clientId == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client profile not found."
            });
        }

        var quote = await _context.Quotes
            .AsNoTracking()
            .Where(x =>
                x.QuoteId == id &&
                x.ClientId == clientId.Value)
            .Select(x => new
            {
                quoteId = x.QuoteId,
                quoteNumber = x.QuoteNumber,
                clientId = x.ClientId,
                projectId = x.ProjectId,
                title = x.Title,
                description = x.Description,
                amount = x.Amount,
                status = x.Status,
                validUntil = x.ValidUntil,
                createdAt = x.CreatedAt,
                updatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (quote == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Quote not found."
            });
        }

        return Ok(new
        {
            success = true,
            quote
        });
    }
    // =========================================================
    // ACCEPT QUOTE - CLIENT ONLY
    // =========================================================

    [HttpPut("my-quotes/{id:int}/accept")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> AcceptQuote(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid QuoteId is required."
            });
        }

        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) ||
            !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        var clientId = await _context.Clients
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => (int?)x.ClientId)
            .FirstOrDefaultAsync();

        if (clientId == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client profile not found."
            });
        }

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(x =>
                x.QuoteId == id &&
                x.ClientId == clientId.Value);

        if (quote == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Quote not found."
            });
        }

        if (quote.Status != "Draft" &&
            quote.Status != "Sent")
        {
            return BadRequest(new
            {
                success = false,
                message = $"Quote cannot be accepted because its current status is '{quote.Status}'."
            });
        }

        if (quote.ValidUntil.HasValue &&
            quote.ValidUntil.Value < DateTime.UtcNow)
        {
            quote.Status = "Expired";
            quote.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                success = false,
                message = "This quote has expired."
            });
        }

        quote.Status = "Accepted";
        quote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Quote accepted successfully.",
            quoteId = quote.QuoteId,
            quoteNumber = quote.QuoteNumber,
            status = quote.Status,
            updatedAt = quote.UpdatedAt
        });
    }
    // =========================================================
    // REJECT QUOTE - CLIENT ONLY
    // =========================================================

    [HttpPut("my-quotes/{id:int}/reject")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> RejectQuote(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid QuoteId is required."
            });
        }

        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) ||
            !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        var clientId = await _context.Clients
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => (int?)x.ClientId)
            .FirstOrDefaultAsync();

        if (clientId == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client profile not found."
            });
        }

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(x =>
                x.QuoteId == id &&
                x.ClientId == clientId.Value);

        if (quote == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Quote not found."
            });
        }

        if (quote.Status != "Draft" &&
            quote.Status != "Sent")
        {
            return BadRequest(new
            {
                success = false,
                message =
                    $"Quote cannot be rejected because its current status is '{quote.Status}'."
            });
        }

        if (quote.ValidUntil.HasValue &&
            quote.ValidUntil.Value < DateTime.UtcNow)
        {
            quote.Status = "Expired";
            quote.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                success = false,
                message = "This quote has expired."
            });
        }

        quote.Status = "Rejected";
        quote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Quote rejected successfully.",
            quoteId = quote.QuoteId,
            quoteNumber = quote.QuoteNumber,
            status = quote.Status,
            updatedAt = quote.UpdatedAt
        });
    }
    // =========================================================
    // GET ALL QUOTES - ADMIN ONLY
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllQuotes()
    {
        var quotes = await _context.Quotes
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                quoteId = x.QuoteId,
                quoteNumber = x.QuoteNumber,

                clientId = x.ClientId,
                clientName = x.Client.User.FullName,
                companyName = x.Client.CompanyName,

                projectId = x.ProjectId,
                projectName = x.Project.ProjectName,

                title = x.Title,
                description = x.Description,
                amount = x.Amount,
                status = x.Status,
                validUntil = x.ValidUntil,
                createdAt = x.CreatedAt,
                updatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            totalQuotes = quotes.Count,
            quotes
        });
    }
    // ============================================================
    // CREATE QUOTE FROM APPROVED PROJECT INQUIRY
    // ADMIN ONLY
    // POST: api/Quotes/from-inquiry
    // ============================================================

    [HttpPost("from-inquiry")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuoteFromInquiry(
        [FromBody] CreateQuoteFromInquiryRequest request)
    {
        // ========================================================
        // VALIDATION
        // ========================================================

        if (request.ProjectInquiryId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ProjectInquiryId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.QuoteNumber))
        {
            return BadRequest(new
            {
                success = false,
                message = "Quote number is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new
            {
                success = false,
                message = "Quote title is required."
            });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Quote amount must be greater than zero."
            });
        }

        // ========================================================
        // FIND INQUIRY
        // ========================================================

        var inquiry = await _context.ProjectInquiries
            .FirstOrDefaultAsync(x =>
                x.ProjectInquiryId ==
                request.ProjectInquiryId);

        if (inquiry == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Project inquiry not found."
            });
        }

        // ========================================================
        // ONLY APPROVED INQUIRY CAN CREATE PROPOSAL
        // ========================================================

        if (!string.Equals(
                inquiry.Status,
                "Approved",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                success = false,
                message =
                    $"Proposal can only be created from an Approved inquiry. Current status is '{inquiry.Status}'."
            });
        }

        // ========================================================
        // RESOLVE CONTACT DETAILS
        // ========================================================

        Client? linkedClient = null;
        User? linkedUser = null;

        // --------------------------------------------------------
        // FIRST: CHECK IF INQUIRY IS ALREADY LINKED TO CLIENT
        // --------------------------------------------------------

        if (inquiry.ClientId.HasValue)
        {
            linkedClient = await _context.Clients
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.ClientId == inquiry.ClientId.Value);

            if (linkedClient != null)
            {
                linkedUser = linkedClient.User;
            }
        }

        // --------------------------------------------------------
        // RESOLVE NAME
        // --------------------------------------------------------

        var contactName =
            !string.IsNullOrWhiteSpace(inquiry.ContactName)
                ? inquiry.ContactName.Trim()
                : linkedUser?.FullName;

        // --------------------------------------------------------
        // RESOLVE EMAIL
        // --------------------------------------------------------

        var contactEmail =
            !string.IsNullOrWhiteSpace(inquiry.ContactEmail)
                ? inquiry.ContactEmail.Trim()
                : linkedUser?.Email;

        // --------------------------------------------------------
        // RESOLVE PHONE
        // --------------------------------------------------------

        var contactPhone =
            !string.IsNullOrWhiteSpace(inquiry.ContactPhone)
                ? inquiry.ContactPhone.Trim()
                : linkedClient?.PhoneNumber;

        // --------------------------------------------------------
        // RESOLVE COMPANY
        // --------------------------------------------------------

        var companyName =
            !string.IsNullOrWhiteSpace(inquiry.CompanyName)
                ? inquiry.CompanyName.Trim()
                : linkedClient?.CompanyName;

        // --------------------------------------------------------
        // VALIDATE NAME
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(contactName))
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Inquiry contact name is missing."
            });
        }

        // --------------------------------------------------------
        // VALIDATE EMAIL
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(contactEmail))
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Inquiry contact email is missing."
            });
        }

        var email =
            contactEmail.Trim().ToLower();
        // ========================================================
        // FIND EXISTING USER
        // ========================================================

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Email.ToLower() == email);

        Client? client = null;

        // ========================================================
        // EXISTING USER
        // ========================================================

        if (user != null)
        {
            client = await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.UserId);

            // ----------------------------------------------------
            // USER EXISTS BUT CLIENT PROFILE DOES NOT
            // ----------------------------------------------------

            if (client == null)
            {
                client = new Client
                {
                    UserId = user.UserId,

                    CompanyName =
    string.IsNullOrWhiteSpace(companyName)
        ? "Individual Client"
        : companyName,

                    PhoneNumber = contactPhone,

                    CreatedAt = DateTime.UtcNow
                };

                _context.Clients.Add(client);

                await _context.SaveChangesAsync();
            }
        }
        else
        {
            // ====================================================
            // CREATE USER
            // ====================================================

            user = new User
            {
                FullName =
    contactName,

                Email = email,

                Role = "Client",

                IsActive = true,

                CreatedAt = DateTime.UtcNow
            };

            // ----------------------------------------------------
            // TEMPORARY PASSWORD HASH
            // ----------------------------------------------------
            //
            // Client account is created for portal linkage.
            // Actual password/invite flow can be implemented later.
            //

            user.PasswordHash =
                Guid.NewGuid()
                    .ToString("N");

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            // ====================================================
            // CREATE CLIENT
            // ====================================================

            client = new Client
            {
                UserId = user.UserId,

                CompanyName =
                    string.IsNullOrWhiteSpace(
                        inquiry.CompanyName)
                        ? "Individual Client"
                        : inquiry.CompanyName.Trim(),

                PhoneNumber =
                    string.IsNullOrWhiteSpace(
                        inquiry.ContactPhone)
                        ? null
                        : inquiry.ContactPhone.Trim(),

                CreatedAt = DateTime.UtcNow
            };

            _context.Clients.Add(client);

            await _context.SaveChangesAsync();
        }

        // ========================================================
        // FIND EXISTING PROJECT FOR THIS INQUIRY
        // ========================================================

        var existingProject =
            await _context.Projects
                .FirstOrDefaultAsync(x =>
                    x.ClientId == client.ClientId &&
                    x.ProjectName ==
                        inquiry.ProjectName);

        Project project;

        if (existingProject != null)
        {
            project = existingProject;
        }
        else
        {
            // ====================================================
            // CREATE PROJECT
            // ====================================================

            project = new Project
            {
                ClientId = client.ClientId,

                ProjectName =
                    string.IsNullOrWhiteSpace(
                        inquiry.ProjectName)
                        ? $"{inquiry.ServiceRequired ?? inquiry.ProjectType} Project"
                        : inquiry.ProjectName.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(
                        inquiry.Description)
                        ? inquiry.Requirements
                        : inquiry.Description.Trim(),

                Status = "Planning",

                ProjectAmount = request.Amount,

                StartDate =
                    inquiry.StartDate,

                ExpectedEndDate =
                    inquiry.DeliveryDate,

                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();
        }

        // ========================================================
        // CHECK DUPLICATE QUOTE NUMBER
        // ========================================================

        var existingQuote =
            await _context.Quotes
                .FirstOrDefaultAsync(x =>
                    x.QuoteNumber ==
                    request.QuoteNumber.Trim());

        if (existingQuote != null)
        {
            return Conflict(new
            {
                success = false,
                message =
                    "A quote with this quote number already exists."
            });
        }

        // ========================================================
        // CREATE QUOTE
        // ========================================================

        var quote = new Quote
        {
            ClientId = client.ClientId,

            ProjectId = project.ProjectId,

            QuoteNumber =
                request.QuoteNumber.Trim(),

            Title =
                request.Title.Trim(),

            Description =
                string.IsNullOrWhiteSpace(
                    request.Description)
                    ? inquiry.Description
                    : request.Description.Trim(),

            Amount = request.Amount,

            Status = "Draft",

            ValidUntil =
                request.ValidUntil,

            CreatedAt = DateTime.UtcNow
        };

        _context.Quotes.Add(quote);

        // ========================================================
        // LINK INQUIRY TO CLIENT
        // ========================================================

        inquiry.ClientId =
            client.ClientId;

        inquiry.Status = "Approved";

        inquiry.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ========================================================
        // RESPONSE
        // ========================================================

        return Ok(new
        {
            success = true,

            message =
                "Proposal draft created successfully.",

            quoteId =
                quote.QuoteId,

            quoteNumber =
                quote.QuoteNumber,

            clientId =
                client.ClientId,

            clientName =
                user.FullName,

            clientEmail =
                user.Email,

            companyName =
                client.CompanyName,

            projectId =
                project.ProjectId,

            projectName =
                project.ProjectName,

            status =
                quote.Status,

            amount =
                quote.Amount,

            validUntil =
                quote.ValidUntil,

            createdAt =
                quote.CreatedAt
        });
    }
    // =========================================================
    // SEND QUOTE - ADMIN ONLY
    // =========================================================

    [HttpPut("{id:int}/send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendQuote(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid QuoteId is required."
            });
        }

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(x => x.QuoteId == id);

        if (quote == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Quote not found."
            });
        }

        if (quote.Status != "Draft")
        {
            return BadRequest(new
            {
                success = false,
                message =
                    $"Quote cannot be sent because its current status is '{quote.Status}'."
            });
        }

        if (quote.ValidUntil.HasValue &&
            quote.ValidUntil.Value < DateTime.UtcNow)
        {
            quote.Status = "Expired";
            quote.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                success = false,
                message = "This quote has already expired."
            });
        }

        quote.Status = "Sent";
        quote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Quote sent successfully.",
            quoteId = quote.QuoteId,
            quoteNumber = quote.QuoteNumber,
            status = quote.Status,
            updatedAt = quote.UpdatedAt
        });
    }
}