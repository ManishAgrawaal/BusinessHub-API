namespace MTS_API.Models;

public class Project
{
    public int ProjectId { get; set; }

    public int ClientId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "Planning";

    public decimal? ProjectAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? ExpectedEndDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Client Client { get; set; } = null!;

    public ICollection<ProjectMilestone> Milestones { get; set; }
        = new List<ProjectMilestone>();
}