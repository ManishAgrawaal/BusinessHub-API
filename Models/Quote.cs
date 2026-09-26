namespace MTS_API.Models;

public class Quote
{
    public int QuoteId { get; set; }

    public int ClientId { get; set; }

    public int ProjectId { get; set; }

    public string QuoteNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = "Draft";

    public DateTime? ValidUntil { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Client Client { get; set; } = null!;

    public Project Project { get; set; } = null!;
}