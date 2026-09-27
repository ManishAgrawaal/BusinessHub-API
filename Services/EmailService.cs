using Resend;

namespace MTS_API.Services;

public class EmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;

    public EmailService(
        IResend resend,
        IConfiguration configuration)
    {
        _resend = resend;
        _configuration = configuration;
    }

    public async Task SendProjectInquiryEmailsAsync(
        string customerName,
        string customerEmail,
        string phone,
        string? company,
        string service,
        string? budget,
        string projectDetails)
    {
        var fromEmail = _configuration["Email:FromEmail"];
        var adminEmail = _configuration["Email:AdminEmail"];

        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException(
                "Email:FromEmail is not configured.");

        if (string.IsNullOrWhiteSpace(adminEmail))
            throw new InvalidOperationException(
                "Email:AdminEmail is not configured.");

        // ---------------------------------------------
        // 1. Admin notification
        // ---------------------------------------------

        var adminMessage = new EmailMessage
        {
            From = fromEmail,
            To = adminEmail,
            Subject = $"New Project Inquiry - {customerName}",
            HtmlBody = $"""
                <html>
                <body>
                    <h2>New Project Inquiry</h2>

                    <p><strong>Name:</strong> {customerName}</p>
                    <p><strong>Email:</strong> {customerEmail}</p>
                    <p><strong>Phone:</strong> {phone}</p>
                    <p><strong>Company:</strong> {company ?? "-"}</p>
                    <p><strong>Service:</strong> {service}</p>
                    <p><strong>Budget:</strong> {budget ?? "-"}</p>

                    <h3>Project Details</h3>
                    <p>{projectDetails}</p>

                    <hr />

                    <p>
                        This inquiry was submitted through the
                        BusinessHub website.
                    </p>
                </body>
                </html>
                """
        };

        await _resend.EmailSendAsync(adminMessage);

        // ---------------------------------------------
        // 2. Customer confirmation
        // ---------------------------------------------

        var customerMessage = new EmailMessage
        {
            From = fromEmail,
            To = customerEmail,
            Subject = "Thank You for Your Project Inquiry - BusinessHub",
            HtmlBody = $"""
                <html>
                <body>
                    <h2>Thank You, {customerName}!</h2>

                    <p>
                        We have successfully received your
                        project inquiry.
                    </p>

                    <p>
                        Our team will review your requirements
                        and get back to you shortly.
                    </p>

                    <h3>Inquiry Details</h3>

                    <p>
                        <strong>Service:</strong> {service}
                    </p>

                    <p>
                        <strong>Budget:</strong> {budget ?? "-"}
                    </p>

                    <p>
                        <strong>Project Details:</strong><br />
                        {projectDetails}
                    </p>

                    <br />

                    <p>
                        Regards,<br />
                        <strong>Manish Technology Solution</strong><br />
                        THINK | BUILD | GROW TOGETHER
                    </p>
                </body>
                </html>
                """
        };

        await _resend.EmailSendAsync(customerMessage);
    }
}