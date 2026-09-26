namespace MTS_API.DTOs;

public class UpdateProjectMilestoneRequest
{
    public int ProjectId { get; set; }

    public string MilestoneName { get; set; }
        = string.Empty;

    public string? Description { get; set; }

    public int ProgressPercentage { get; set; }

    public string? Status { get; set; }

    public DateTime? PlannedDate { get; set; }

    public DateTime? CompletedDate { get; set; }
}