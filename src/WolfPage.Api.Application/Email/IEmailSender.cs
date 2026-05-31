namespace WolfPage.Api.Application.Email;

public interface IEmailSender
{
    Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default);
}
