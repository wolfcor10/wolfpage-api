using Microsoft.Extensions.Logging;
using WolfPage.Api.Application.Email;

namespace WolfPage.Api.Infrastructure.Email;

public class LogEmailSender : IEmailSender
{
    private readonly ILogger<LogEmailSender> _logger;

    public LogEmailSender(ILogger<LogEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email simulated. To: {To}. Subject: {Subject}. Text: {Text}",
            request.To,
            request.Subject,
            request.PlainTextBody ?? request.HtmlBody);

        return Task.CompletedTask;
    }
}
