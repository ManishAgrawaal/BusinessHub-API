namespace MTS_API.DTOs;

public class CreateProjectRequest
{
    public int ClientId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "Planning";

    public decimal? ProjectAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? ExpectedEndDate { get; set; }
}