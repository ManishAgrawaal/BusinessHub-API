using Microsoft.AspNetCore.Http;

namespace MTS_API.DTOs;

public class CreateDocumentRequest
{
    public int? ProjectId { get; set; }

    public IFormFile File { get; set; } = null!;
}