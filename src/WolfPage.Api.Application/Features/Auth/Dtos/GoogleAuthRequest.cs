namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class GoogleAuthRequest
{
    public string IdToken { get; set; } = default!;
    public string? WorkspaceName { get; set; }
    public string? WorkspaceEmail { get; set; }
    public string WorkspaceType { get; set; } = "Individual";
    public string ProfileType { get; set; } = "PersonalBrand";
}
