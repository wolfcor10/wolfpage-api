using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using WolfPage.Api.Application.Email;
using WolfPage.Api.Infrastructure.Options;

namespace WolfPage.Api.Infrastructure.Email;

public class AzureCommunicationEmailSender : IEmailSender
{
    private readonly EmailClient _client;
    private readonly EmailOptions _emailOptions;

    public AzureCommunicationEmailSender(
        IOptions<AzureCommunicationServicesOptions> acsOptions,
        IOptions<EmailOptions> emailOptions)
    {
        if (string.IsNullOrWhiteSpace(acsOptions.Value.ConnectionString))
            throw new InvalidOperationException("Azure Communication Services connection string is not configured.");

        _client = new EmailClient(acsOptions.Value.ConnectionString);
        _emailOptions = emailOptions.Value;
    }

    public async Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_emailOptions.FromAddress))
            throw new InvalidOperationException("Email FromAddress is not configured.");

        await _client.SendAsync(
            WaitUntil.Completed,
            senderAddress: _emailOptions.FromAddress,
            recipientAddress: request.To,
            subject: request.Subject,
            htmlContent: request.HtmlBody,
            plainTextContent: request.PlainTextBody,
            cancellationToken: cancellationToken);
    }
}
