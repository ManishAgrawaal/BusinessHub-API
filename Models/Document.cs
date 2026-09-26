namespace MTS_API.Models;

public class Document
{
    public int DocumentId { get; set; }

    public int ClientId { get; set; }

    public int? ProjectId { get; set; }

    public int UploadedByUserId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    public Client Client { get; set; } = null!;

    public Project? Project { get; set; }

    public User UploadedByUser { get; set; } = null!;
}