namespace MTS_API.Models;

public class ProjectMilestone
{
    public int ProjectMilestoneId { get; set; }

    public int ProjectId { get; set; }

    public string MilestoneName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int ProgressPercentage { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime? PlannedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
}