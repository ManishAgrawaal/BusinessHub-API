namespace MTS_API.Models;

public class ProjectInquiry
{
    public int ProjectInquiryId { get; set; }

    public int? ClientId { get; set; }
    public string? ContactName { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? CompanyName { get; set; }

    public string? ServiceRequired { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string ProjectType { get; set; } = string.Empty;

    public string? Budget { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Requirements { get; set; } = string.Empty;

    public string? Technologies { get; set; }

    public string? AdditionalRequirements { get; set; }

    public string Status { get; set; } = "New";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Client? Client { get; set; }
    
}