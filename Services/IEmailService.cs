namespace MTS_API.Services
{
    public interface IEmailService
    {
        Task SendProjectInquiryEmailsAsync(
        string customerName,
        string customerEmail,
        string phone,
        string? company,
        string service,
        string? budget,
        string projectDetails);
    }
}
