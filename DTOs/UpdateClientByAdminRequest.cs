
namespace MTS_API.DTOs;

public class UpdateClientByAdminRequest
{
    public string FullName { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public bool IsActive { get; set; } = true;
}