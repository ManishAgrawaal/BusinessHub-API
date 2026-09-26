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
public class ProjectsController : ControllerBase
{
    private readonly MtsDbContext _context;

    public ProjectsController(MtsDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // CREATE PROJECT - ADMIN ONLY
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProject(
        [FromBody] CreateProjectRequest request)
    {
        if (request.ClientId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ClientId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.ProjectName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Project name is required."
            });
        }

        // =====================================================
        // VALIDATE CLIENT
        // =====================================================

        var clientExists = await _context.Clients
            .AnyAsync(x => x.ClientId == request.ClientId);

        if (!clientExists)
        {
            return NotFound(new
            {
                success = false,
                message = "Client not found."
            });
        }

        // =====================================================
        // CREATE PROJECT
        // =====================================================

        var project = new Project
        {
            ClientId = request.ClientId,

            ProjectName =
                request.ProjectName.Trim(),

            Description =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),

            Status =
                string.IsNullOrWhiteSpace(request.Status)
                    ? "Planning"
                    : request.Status.Trim(),

            ProjectAmount =
                request.ProjectAmount,

            StartDate =
                request.StartDate,

            ExpectedEndDate =
                request.ExpectedEndDate,

            CreatedAt =
                DateTime.UtcNow
        };

        _context.Projects.Add(project);

        // Save first so ProjectId is generated
        await _context.SaveChangesAsync();

        // =====================================================
        // CREATE DEFAULT PROJECT MILESTONES
        // =====================================================

        var startDate = project.StartDate;

        var defaultMilestones = new List<ProjectMilestone>
    {
        new ProjectMilestone
        {
            ProjectId = project.ProjectId,

            MilestoneName = "Requirements",

            Description =
                "Business and technical requirements gathering.",

            ProgressPercentage = 0,

            Status = "Pending",

            PlannedDate =
                startDate?.AddDays(7)
        },

        new ProjectMilestone
        {
            ProjectId = project.ProjectId,

            MilestoneName = "UI/UX Design",

            Description =
                "Design of application screens and user experience.",

            ProgressPercentage = 0,

            Status = "Pending",

            PlannedDate =
                startDate?.AddDays(21)
        },

        new ProjectMilestone
        {
            ProjectId = project.ProjectId,

            MilestoneName = "Development",

            Description =
                "Application development and feature implementation.",

            ProgressPercentage = 0,

            Status = "Pending",

            PlannedDate =
                startDate?.AddDays(60)
        },

        new ProjectMilestone
        {
            ProjectId = project.ProjectId,

            MilestoneName = "Testing",

            Description =
                "Functional, integration and user acceptance testing.",

            ProgressPercentage = 0,

            Status = "Pending",

            PlannedDate =
                startDate?.AddDays(75)
        },

        new ProjectMilestone
        {
            ProjectId = project.ProjectId,

            MilestoneName = "Deployment",

            Description =
                "Production deployment and release.",

            ProgressPercentage = 0,

            Status = "Pending",

            PlannedDate =
                project.ExpectedEndDate
                ?? startDate?.AddDays(90)
        }
    };

        _context.ProjectMilestones.AddRange(
            defaultMilestones
        );

        await _context.SaveChangesAsync();

        // =====================================================
        // RESPONSE
        // =====================================================

        return Ok(new
        {
            success = true,

            message =
                "Project created successfully with default milestones.",

            projectId =
                project.ProjectId,

            clientId =
                project.ClientId,

            projectName =
                project.ProjectName,

            status =
                project.Status,

            projectAmount =
                project.ProjectAmount,

            startDate =
                project.StartDate,

            expectedEndDate =
                project.ExpectedEndDate,

            milestonesCreated =
                defaultMilestones.Count
        });
    }
    // =========================================================
    // GET PROJECT BY ID - ADMIN ONLY
    // =========================================================

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetProject(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ProjectId is required."
            });
        }

        var project = await _context.Projects
            .AsNoTracking()
            .Include(x => x.Milestones)
            .FirstOrDefaultAsync(x => x.ProjectId == id);

        if (project == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Project not found."
            });
        }

        return Ok(new
        {
            success = true,

            projectId = project.ProjectId,
            clientId = project.ClientId,
            projectName = project.ProjectName,
            description = project.Description,
            status = project.Status,
            projectAmount = project.ProjectAmount,
            startDate = project.StartDate,
            expectedEndDate = project.ExpectedEndDate,
            completedDate = project.CompletedDate,

            milestones = project.Milestones
                .OrderBy(x => x.ProjectMilestoneId)
                .Select(x => new
                {
                    projectMilestoneId = x.ProjectMilestoneId,
                    milestoneName = x.MilestoneName,
                    description = x.Description,
                    progressPercentage = x.ProgressPercentage,
                    status = x.Status,
                    plannedDate = x.PlannedDate,
                    completedDate = x.CompletedDate
                })
                .ToList()
        });
    }


    // =========================================================
    // GET: api/Projects
    // GET ALL PROJECTS - ADMIN ONLY
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllProjects()
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .Include(x => x.Client)
                .ThenInclude(x => x.User)
            .Include(x => x.Milestones)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var projectResponse = projects
            .Select(project =>
            {
                var totalMilestones =
                    project.Milestones.Count;

                var completedMilestones =
                    project.Milestones.Count(x =>
                        x.Status.Equals(
                            "Completed",
                            StringComparison.OrdinalIgnoreCase));

                var overallProgress =
                    totalMilestones == 0
                        ? 0
                        : (int)Math.Round(
                            project.Milestones
                                .Average(x =>
                                    x.ProgressPercentage));

                return new
                {
                    projectId = project.ProjectId,

                    clientId = project.ClientId,

                    clientName =
                        project.Client?.User?.FullName,

                    companyName =
                        project.Client?.CompanyName,

                    projectName =
                        project.ProjectName,

                    description =
                        project.Description,

                    status =
                        project.Status,

                    projectAmount =
                        project.ProjectAmount,

                    startDate =
                        project.StartDate,

                    expectedEndDate =
                        project.ExpectedEndDate,

                    completedDate =
                        project.CompletedDate,

                    overallProgress,

                    totalMilestones,

                    completedMilestones,

                    createdAt =
                        project.CreatedAt
                };
            })
            .ToList();

        return Ok(new
        {
            success = true,
            message = "Projects loaded successfully.",
            count = projectResponse.Count,
            projects = projectResponse
        });
    }


    // =========================================================
    // PUT: api/Projects/{id}
    // UPDATE PROJECT - ADMIN ONLY
    // =========================================================

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProject(
        int id,
        [FromBody] UpdateProjectRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ProjectId is required."
            });
        }

        if (request.ClientId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ClientId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.ProjectName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Project name is required."
            });
        }

        var project = await _context.Projects
            .FirstOrDefaultAsync(x =>
                x.ProjectId == id);

        if (project == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Project not found."
            });
        }

        var clientExists = await _context.Clients
            .AnyAsync(x =>
                x.ClientId == request.ClientId);

        if (!clientExists)
        {
            return NotFound(new
            {
                success = false,
                message = "Client not found."
            });
        }

        project.ClientId =
            request.ClientId;

        project.ProjectName =
            request.ProjectName.Trim();

        project.Description =
            string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim();

        project.Status =
            string.IsNullOrWhiteSpace(
                request.Status)
                ? "Planning"
                : request.Status.Trim();

        project.ProjectAmount =
            request.ProjectAmount;

        project.StartDate =
            request.StartDate;

        project.ExpectedEndDate =
            request.ExpectedEndDate;

        project.CompletedDate =
            request.CompletedDate;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Project updated successfully.",

            projectId =
                project.ProjectId,

            clientId =
                project.ClientId,

            projectName =
                project.ProjectName,

            description =
                project.Description,

            status =
                project.Status,

            projectAmount =
                project.ProjectAmount,

            startDate =
                project.StartDate,

            expectedEndDate =
                project.ExpectedEndDate,

            completedDate =
                project.CompletedDate
        });
    }

    // =========================================================
    // GET MY PROJECTS - CLIENT ONLY
    // =========================================================

    [HttpGet("my-projects")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyProjects()
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

        var projects = await _context.Projects
            .AsNoTracking()
            .Where(x => x.ClientId == client.ClientId)
            .Include(x => x.Milestones)
            .Select(x => new
            {
                projectId = x.ProjectId,
                projectName = x.ProjectName,
                description = x.Description,
                status = x.Status,
                projectAmount = x.ProjectAmount,
                startDate = x.StartDate,
                expectedEndDate = x.ExpectedEndDate,
                completedDate = x.CompletedDate,

                milestones = x.Milestones
                    .OrderBy(m => m.ProjectMilestoneId)
                    .Select(m => new
                    {
                        projectMilestoneId = m.ProjectMilestoneId,
                        milestoneName = m.MilestoneName,
                        description = m.Description,
                        progressPercentage = m.ProgressPercentage,
                        status = m.Status,
                        plannedDate = m.PlannedDate,
                        completedDate = m.CompletedDate
                    })
                    .ToList()
            })
            .ToListAsync();

        var projectResponse = projects
            .Select(project =>
            {
                var totalMilestones = project.milestones.Count;

                var overallProgress = totalMilestones == 0
                    ? 0
                    : (int)Math.Round(
                        project.milestones
                            .Average(x => x.progressPercentage));

                var completedMilestones = project.milestones
                    .Count(x =>
                        x.status.Equals(
                            "Completed",
                            StringComparison.OrdinalIgnoreCase));

                return new
                {
                    projectId = project.projectId,
                    projectName = project.projectName,
                    description = project.description,
                    status = project.status,
                    projectAmount = project.projectAmount,
                    startDate = project.startDate,
                    expectedEndDate = project.expectedEndDate,
                    completedDate = project.completedDate,

                    overallProgress,
                    totalMilestones,
                    completedMilestones,

                    milestones = project.milestones
                };
            })
            .ToList();

        return Ok(new
        {
            success = true,
            clientId = client.ClientId,
            projects = projectResponse
        });

    }
    // =========================================================
    // GET MY PROJECT BY ID - CLIENT ONLY
    // =========================================================

    [HttpGet("my-projects/{id:int}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyProject(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ProjectId is required."
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

        var project = await _context.Projects
            .AsNoTracking()
            .Include(x => x.Milestones)
            .FirstOrDefaultAsync(x =>
                x.ProjectId == id &&
                x.ClientId == clientId.Value);

        if (project == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Project not found."
            });
        }

        var totalMilestones = project.Milestones.Count;

        var overallProgress = totalMilestones == 0
            ? 0
            : (int)Math.Round(
                project.Milestones.Average(
                    x => x.ProgressPercentage));

        var completedMilestones = project.Milestones
            .Count(x =>
                x.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase));

        return Ok(new
        {
            success = true,

            projectId = project.ProjectId,
            clientId = project.ClientId,
            projectName = project.ProjectName,
            description = project.Description,
            status = project.Status,
            projectAmount = project.ProjectAmount,
            startDate = project.StartDate,
            expectedEndDate = project.ExpectedEndDate,
            completedDate = project.CompletedDate,

            overallProgress,
            totalMilestones,
            completedMilestones,

            milestones = project.Milestones
                .OrderBy(x => x.ProjectMilestoneId)
                .Select(x => new
                {
                    projectMilestoneId = x.ProjectMilestoneId,
                    milestoneName = x.MilestoneName,
                    description = x.Description,
                    progressPercentage = x.ProgressPercentage,
                    status = x.Status,
                    plannedDate = x.PlannedDate,
                    completedDate = x.CompletedDate
                })
                .ToList()
        });
    }


}
