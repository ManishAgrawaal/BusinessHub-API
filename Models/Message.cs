namespace MTS_API.Models;

public class Message
{
    public int MessageId { get; set; }

    public int SenderUserId { get; set; }

    public int ReceiverUserId { get; set; }

    public int? ProjectId { get; set; }

    public string? Subject { get; set; }

    public string MessageText { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    // Navigation properties

    public User SenderUser { get; set; } = null!;

    public User ReceiverUser { get; set; } = null!;

    public Project? Project { get; set; }
}