namespace MTS_API.DTOs;

public class CreatePublicProjectInquiryRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string Service { get; set; } = string.Empty;

    public string? Budget { get; set; }

    public string Message { get; set; } = string.Empty;
}