namespace MTS_API.DTOs;

public class CreateMessageRequest
{
    public int ReceiverUserId { get; set; }

    public int? ProjectId { get; set; }

    public string? Subject { get; set; }

    public string MessageText { get; set; } = string.Empty;
}