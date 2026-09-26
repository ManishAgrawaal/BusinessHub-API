using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTS_API.Data;
using MTS_API.DTOs;
using MTS_API.Models;

namespace MTS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly MtsDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DocumentsController(
        MtsDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // =========================================================
    // GET: api/Documents/my-documents
    // CLIENT DOCUMENTS
    // =========================================================

    [HttpGet("my-documents")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyDocuments()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == userId.Value);

        if (client == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Client profile not found."
            });
        }

        var documents = await _context.Documents
            .AsNoTracking()
            .Where(x =>
                x.ClientId == client.ClientId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                documentId = x.DocumentId,
                clientId = x.ClientId,
                projectId = x.ProjectId,

                projectName = x.Project != null
                    ? x.Project.ProjectName
                    : null,

                fileName = x.FileName,
                contentType = x.ContentType,
                fileSize = x.FileSize,

                uploadedByUserId =
                    x.UploadedByUserId,

                uploadedBy =
                    x.UploadedByUser.FullName,

                createdAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            clientId = client.ClientId,
            totalDocuments = documents.Count,
            documents
        });
    }

    // =========================================================
    // GET: api/Documents/my-documents/{id}
    // CLIENT DOCUMENT DETAIL
    // =========================================================

    [HttpGet("my-documents/{id:int}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyDocument(
        int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Valid DocumentId is required."
            });
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message =
                    "Invalid user identity."
            });
        }

        var clientId = await _context.Clients
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId.Value)
            .Select(x => (int?)x.ClientId)
            .FirstOrDefaultAsync();

        if (clientId == null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Client profile not found."
            });
        }

        var document = await _context.Documents
            .AsNoTracking()
            .Where(x =>
                x.DocumentId == id &&
                x.ClientId == clientId.Value)
            .Select(x => new
            {
                documentId = x.DocumentId,
                clientId = x.ClientId,
                projectId = x.ProjectId,

                projectName = x.Project != null
                    ? x.Project.ProjectName
                    : null,

                fileName = x.FileName,
                storedFileName =
                    x.StoredFileName,

                filePath = x.FilePath,

                contentType =
                    x.ContentType,

                fileSize =
                    x.FileSize,

                uploadedByUserId =
                    x.UploadedByUserId,

                uploadedBy =
                    x.UploadedByUser.FullName,

                createdAt =
                    x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (document == null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Document not found."
            });
        }

        return Ok(new
        {
            success = true,
            document
        });
    }

    // =========================================================
    // GET: api/Documents/admin-documents
    // ADMIN - ALL DOCUMENTS
    // =========================================================

    [HttpGet("admin-documents")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminDocuments()
    {
        var documents = await _context.Documents
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                documentId = x.DocumentId,

                clientId = x.ClientId,

                clientName =
                    x.Client.User.FullName,

                clientEmail =
                    x.Client.User.Email,

                companyName =
                    x.Client.CompanyName,

                projectId =
                    x.ProjectId,

                projectName =
                    x.Project != null
                        ? x.Project.ProjectName
                        : null,

                fileName =
                    x.FileName,

                contentType =
                    x.ContentType,

                fileSize =
                    x.FileSize,

                uploadedByUserId =
                    x.UploadedByUserId,

                uploadedBy =
                    x.UploadedByUser.FullName,

                createdAt =
                    x.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,

            totalDocuments =
                documents.Count,

            documents
        });
    }

    // =========================================================
    // GET: api/Documents/admin-documents/{id}
    // ADMIN - DOCUMENT DETAIL
    // =========================================================

    [HttpGet("admin-documents/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminDocument(
        int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Valid DocumentId is required."
            });
        }

        var document = await _context.Documents
            .AsNoTracking()
            .Where(x =>
                x.DocumentId == id)
            .Select(x => new
            {
                documentId =
                    x.DocumentId,

                clientId =
                    x.ClientId,

                clientName =
                    x.Client.User.FullName,

                clientEmail =
                    x.Client.User.Email,

                companyName =
                    x.Client.CompanyName,

                projectId =
                    x.ProjectId,

                projectName =
                    x.Project != null
                        ? x.Project.ProjectName
                        : null,

                fileName =
                    x.FileName,

                storedFileName =
                    x.StoredFileName,

                filePath =
                    x.FilePath,

                contentType =
                    x.ContentType,

                fileSize =
                    x.FileSize,

                uploadedByUserId =
                    x.UploadedByUserId,

                uploadedBy =
                    x.UploadedByUser.FullName,

                createdAt =
                    x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (document == null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Document not found."
            });
        }

        return Ok(new
        {
            success = true,
            document
        });
    }

    // =========================================================
    // GET: api/Documents/{id}/download
    // CLIENT + ADMIN DOWNLOAD
    // =========================================================

    [HttpGet("{id:int}/download")]
    [Authorize(Roles = "Admin,Client")]
    public async Task<IActionResult> DownloadDocument(
        int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Valid DocumentId is required."
            });
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message =
                    "Invalid user identity."
            });
        }

        var isAdmin =
            User.IsInRole("Admin");

        Document? document;

        if (isAdmin)
        {
            // ADMIN CAN DOWNLOAD ANY DOCUMENT

            document = await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.DocumentId == id);
        }
        else
        {
            // CLIENT CAN DOWNLOAD OWN DOCUMENT ONLY

            var clientId =
                await _context.Clients
                    .AsNoTracking()
                    .Where(x =>
                        x.UserId == userId.Value)
                    .Select(x =>
                        (int?)x.ClientId)
                    .FirstOrDefaultAsync();

            if (clientId == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Client profile not found."
                });
            }

            document =
                await _context.Documents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.DocumentId == id &&
                        x.ClientId ==
                            clientId.Value);
        }

        if (document == null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Document not found."
            });
        }

        var fullPath = Path.Combine(
            _environment.ContentRootPath,
            document.FilePath);

        if (!System.IO.File.Exists(
            fullPath))
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Physical file not found."
            });
        }

        var fileBytes =
            await System.IO.File.ReadAllBytesAsync(
                fullPath);

        return File(
            fileBytes,
            document.ContentType,
            document.FileName);
    }

    // =========================================================
    // POST: api/Documents/upload
    // CLIENT UPLOAD
    // =========================================================

    [HttpPost("upload")]
    [Authorize(Roles = "Client")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(
        [FromForm] CreateDocumentRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message =
                    "Invalid user identity."
            });
        }

        if (request.File == null ||
            request.File.Length == 0)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Please select a file."
            });
        }

        var client =
            await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId.Value);

        if (client == null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Client profile not found."
            });
        }

        if (request.ProjectId.HasValue)
        {
            var projectExists =
                await _context.Projects
                    .AnyAsync(x =>
                        x.ProjectId ==
                            request.ProjectId.Value &&
                        x.ClientId ==
                            client.ClientId);

            if (!projectExists)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Project does not belong to this client."
                });
            }
        }

        var extension =
            Path.GetExtension(
                request.File.FileName);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var uploadFolder =
            Path.Combine(
                _environment.ContentRootPath,
                "Storage",
                "Documents");

        Directory.CreateDirectory(
            uploadFolder);

        var physicalPath =
            Path.Combine(
                uploadFolder,
                storedFileName);

        await using (
            var stream =
                new FileStream(
                    physicalPath,
                    FileMode.Create))
        {
            await request.File.CopyToAsync(
                stream);
        }

        var relativePath =
            Path.Combine(
                "Storage",
                "Documents",
                storedFileName);

        var document = new Document
        {
            ClientId =
                client.ClientId,

            ProjectId =
                request.ProjectId,

            UploadedByUserId =
                userId.Value,

            FileName =
                Path.GetFileName(
                    request.File.FileName),

            StoredFileName =
                storedFileName,

            FilePath =
                relativePath,

            ContentType =
                string.IsNullOrWhiteSpace(
                    request.File.ContentType)
                    ? "application/octet-stream"
                    : request.File.ContentType,

            FileSize =
                request.File.Length,

            CreatedAt =
                DateTime.UtcNow
        };

        _context.Documents.Add(
            document);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message =
                "Document uploaded successfully.",

            documentId =
                document.DocumentId,

            fileName =
                document.FileName,

            fileSize =
                document.FileSize,

            contentType =
                document.ContentType,

            projectId =
                document.ProjectId,

            createdAt =
                document.CreatedAt
        });
    }

    // =========================================================
    // DELETE: api/Documents/{id}
    // CLIENT + ADMIN DELETE
    // =========================================================

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Client")]
    public async Task<IActionResult> DeleteDocument(
        int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Valid DocumentId is required."
            });
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message =
                    "Invalid user identity."
            });
        }

        var isAdmin =
            User.IsInRole("Admin");

        Document? document;

        if (isAdmin)
        {
            // ADMIN CAN DELETE ANY DOCUMENT

            document =
                await _context.Documents
                    .FirstOrDefaultAsync(x =>
                        x.DocumentId == id);
        }
        else
        {
            // CLIENT CAN DELETE OWN DOCUMENT ONLY

            var clientId =
                await _context.Clients
                    .AsNoTracking()
                    .Where(x =>
                        x.UserId ==
                            userId.Value)
                    .Select(x =>
                        (int?)x.ClientId)
                    .FirstOrDefaultAsync();

            if (clientId == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Client profile not found."
                });
            }

            document =
                await _context.Documents
                    .FirstOrDefaultAsync(x =>
                        x.DocumentId == id &&
                        x.ClientId ==
                            clientId.Value);
        }

        if (document == null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Document not found."
            });
        }

        var fullPath =
            Path.Combine(
                _environment.ContentRootPath,
                document.FilePath);

        if (System.IO.File.Exists(
            fullPath))
        {
            System.IO.File.Delete(
                fullPath);
        }

        _context.Documents.Remove(
            document);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message =
                "Document deleted successfully."
        });
    }

    // =========================================================
    // HELPER
    // =========================================================

    private int? GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(
            userIdClaim))
        {
            return null;
        }

        if (!int.TryParse(
            userIdClaim,
            out var userId))
        {
            return null;
        }

        return userId;
    }
}