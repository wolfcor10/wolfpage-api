namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class RegisterRequest
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string WorkspaceName { get; set; } = default!;
    public string? WorkspaceEmail { get; set; }
    public string WorkspaceType { get; set; } = "Business";
    public string ProfileType { get; set; } = "Business";
}
