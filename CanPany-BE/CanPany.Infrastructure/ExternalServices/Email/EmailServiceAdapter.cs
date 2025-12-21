using CanPany.Application.Interfaces.Services;

namespace CanPany.Infrastructure.ExternalServices.Email;

public class EmailServiceAdapter : IEmailService
{
    private readonly EmailService _emailService;

    public EmailServiceAdapter(EmailService emailService)
    {
        _emailService = emailService;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        return _emailService.SendEmailAsync(toEmail, subject, body, isHtml);
    }
}

