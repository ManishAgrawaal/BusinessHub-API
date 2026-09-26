namespace MTS_API.DTOs;

public class CreateQuoteRequest
{
    public int ClientId { get; set; }

    public int ProjectId { get; set; }

    public string QuoteNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public DateTime? ValidUntil { get; set; }
}