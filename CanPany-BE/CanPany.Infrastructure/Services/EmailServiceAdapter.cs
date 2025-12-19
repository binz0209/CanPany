using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Services;

namespace CanPany.Infrastructure.Services;

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

