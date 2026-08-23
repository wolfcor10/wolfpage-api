using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using WolfPage.Api.Application.Email;
using WolfPage.Api.Infrastructure.Options;

namespace WolfPage.Api.Infrastructure.Email;

public class AzureCommunicationEmailSender : IEmailSender
{
    private readonly AzureCommunicationServicesOptions _acsOptions;
    private readonly EmailOptions _emailOptions;
    private EmailClient? _client;

    public AzureCommunicationEmailSender(
        IOptions<AzureCommunicationServicesOptions> acsOptions,
        IOptions<EmailOptions> emailOptions)
    {
        _acsOptions = acsOptions.Value;
        _emailOptions = emailOptions.Value;
    }

    public async Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_acsOptions.ConnectionString))
            throw new InvalidOperationException("Azure Communication Services connection string is not configured.");

        if (string.IsNullOrWhiteSpace(_emailOptions.FromAddress))
            throw new InvalidOperationException("Email FromAddress is not configured.");

        _client ??= new EmailClient(_acsOptions.ConnectionString);

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
