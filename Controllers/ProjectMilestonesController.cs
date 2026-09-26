using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTS_API.Data;
using MTS_API.DTOs;
using MTS_API.Models;

namespace MTS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ProjectMilestonesController : ControllerBase
{
    private readonly MtsDbContext _context;

    public ProjectMilestonesController(
        MtsDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // POST: api/ProjectMilestones
    // CREATE MILESTONE
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> CreateMilestone(
        [FromBody] CreateProjectMilestoneRequest request)
    {
        if (request.ProjectId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ProjectId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.MilestoneName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Milestone name is required."
            });
        }

        if (request.ProgressPercentage < 0 ||
            request.ProgressPercentage > 100)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Progress percentage must be between 0 and 100."
            });
        }

        var project = await _context.Projects
            .FirstOrDefaultAsync(
                x => x.ProjectId == request.ProjectId);

        if (project == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Project not found."
            });
        }

        var milestone = new ProjectMilestone
        {
            ProjectId = request.ProjectId,

            MilestoneName =
                request.MilestoneName.Trim(),

            Description =
                request.Description?.Trim(),

            ProgressPercentage =
                request.ProgressPercentage,

            Status =
                string.IsNullOrWhiteSpace(
                    request.Status)
                    ? "Pending"
                    : request.Status.Trim(),

            PlannedDate =
                request.PlannedDate,

            CreatedAt =
                DateTime.UtcNow
        };

        _context.ProjectMilestones.Add(
            milestone);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message =
                "Project milestone created successfully.",

            projectMilestoneId =
                milestone.ProjectMilestoneId,

            projectId =
                milestone.ProjectId,

            milestoneName =
                milestone.MilestoneName,

            description =
                milestone.Description,

            progressPercentage =
                milestone.ProgressPercentage,

            status =
                milestone.Status,

            plannedDate =
                milestone.PlannedDate,

            completedDate =
                milestone.CompletedDate
        });
    }

    // =========================================================
    // GET: api/ProjectMilestones
    // GET ALL MILESTONES - ADMIN
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetAllMilestones()
    {
        var milestones = await _context.ProjectMilestones
            .AsNoTracking()
            .Include(x => x.Project)
            .OrderByDescending(
                x => x.CreatedAt)
            .ToListAsync();

        var response = milestones
            .Select(milestone => new
            {
                projectMilestoneId =
                    milestone.ProjectMilestoneId,

                projectId =
                    milestone.ProjectId,

                projectName =
                    milestone.Project.ProjectName,

                milestoneName =
                    milestone.MilestoneName,

                description =
                    milestone.Description,

                progressPercentage =
                    milestone.ProgressPercentage,

                status =
                    milestone.Status,

                plannedDate =
                    milestone.PlannedDate,

                completedDate =
                    milestone.CompletedDate,

                createdAt =
                    milestone.CreatedAt
            })
            .ToList();

        return Ok(new
        {
            success = true,
            message =
                "Project milestones loaded successfully.",

            count =
                response.Count,

            milestones =
                response
        });
    }

    // =========================================================
    // GET: api/ProjectMilestones/{id}
    // GET MILESTONE BY ID
    // =========================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMilestone(
        int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Valid ProjectMilestoneId is required."
            });
        }

        var milestone =
            await _context.ProjectMilestones
                .AsNoTracking()
                .Include(x => x.Project)
                .FirstOrDefaultAsync(
                    x =>
                        x.ProjectMilestoneId == id);

        if (milestone == null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Project milestone not found."
            });
        }

        return Ok(new
        {
            success = true,

            projectMilestoneId =
                milestone.ProjectMilestoneId,

            projectId =
                milestone.ProjectId,

            projectName =
                milestone.Project.ProjectName,

            milestoneName =
                milestone.MilestoneName,

            description =
                milestone.Description,

            progressPercentage =
                milestone.ProgressPercentage,

            status =
                milestone.Status,

            plannedDate =
                milestone.PlannedDate,

            completedDate =
                milestone.CompletedDate,

            createdAt =
                milestone.CreatedAt
        });
    }

    // =========================================================
    // PUT: api/ProjectMilestones/{id}
    // UPDATE MILESTONE
    // =========================================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMilestone(
        int id,
        [FromBody] UpdateProjectMilestoneRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Valid ProjectMilestoneId is required."
            });
        }

        if (request.ProjectId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Valid ProjectId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.MilestoneName))
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Milestone name is required."
            });
        }

        if (request.ProgressPercentage < 0 ||
            request.ProgressPercentage > 100)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Progress percentage must be between 0 and 100."
            });
        }

        var milestone =
            await _context.ProjectMilestones
                .FirstOrDefaultAsync(
                    x =>
                        x.ProjectMilestoneId == id);

        if (milestone == null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Project milestone not found."
            });
        }

        var projectExists =
            await _context.Projects
                .AnyAsync(
                    x =>
                        x.ProjectId ==
                        request.ProjectId);

        if (!projectExists)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Project not found."
            });
        }

        milestone.ProjectId =
            request.ProjectId;

        milestone.MilestoneName =
            request.MilestoneName.Trim();

        milestone.Description =
            string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim();

        milestone.ProgressPercentage =
            request.ProgressPercentage;

        milestone.Status =
            string.IsNullOrWhiteSpace(
                request.Status)
                ? "Pending"
                : request.Status.Trim();

        milestone.PlannedDate =
            request.PlannedDate;

        milestone.CompletedDate =
            request.CompletedDate;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message =
                "Project milestone updated successfully.",

            projectMilestoneId =
                milestone.ProjectMilestoneId,

            projectId =
                milestone.ProjectId,

            milestoneName =
                milestone.MilestoneName,

            description =
                milestone.Description,

            progressPercentage =
                milestone.ProgressPercentage,

            status =
                milestone.Status,

            plannedDate =
                milestone.PlannedDate,

            completedDate =
                milestone.CompletedDate
        });
    }
}