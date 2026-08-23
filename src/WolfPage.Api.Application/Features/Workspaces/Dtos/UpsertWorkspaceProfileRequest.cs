namespace WolfPage.Api.Application.Features.Workspaces.Dtos;

public class UpsertWorkspaceProfileRequest
{
    public string DisplayName { get; set; } = default!;
    public string? LegalName { get; set; }
    public string? Category { get; set; }
    public string Description { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WhatsApp { get; set; }
    public string? Address { get; set; }
    public string? OpeningHours { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TiktokUrl { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
}
