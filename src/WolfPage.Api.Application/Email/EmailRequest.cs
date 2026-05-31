namespace WolfPage.Api.Application.Email;

public sealed record EmailRequest(
    string To,
    string Subject,
    string HtmlBody,
    string? PlainTextBody = null);
