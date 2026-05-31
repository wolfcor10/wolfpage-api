namespace WolfPage.Api.Application.Features.Auth.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public bool RequireConfirmedEmail { get; set; }
    public int EmailConfirmationTokenHours { get; set; } = 24;
}
