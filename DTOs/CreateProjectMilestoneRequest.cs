namespace MTS_API.DTOs;

public class CreateProjectMilestoneRequest
{
    public int ProjectId { get; set; }

    public string MilestoneName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int ProgressPercentage { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime? PlannedDate { get; set; }
}