namespace MTS_API.DTOs;

public class CreateProjectInquiryRequest
{
    public string ProjectName { get; set; } = string.Empty;

    public string ProjectType { get; set; } = string.Empty;

    public string? Budget { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Requirements { get; set; } = string.Empty;

    public List<string> Technologies { get; set; } = new();

    public string? AdditionalRequirements { get; set; }
}