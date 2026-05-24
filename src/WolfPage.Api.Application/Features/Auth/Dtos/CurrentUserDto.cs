namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public Guid? ActiveWorkspaceId { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string[] Roles { get; set; } = [];
    public CurrentUserWorkspaceDto[] Workspaces { get; set; } = [];
}

public class CurrentUserWorkspaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string WorkspaceType { get; set; } = default!;
    public string ProfileType { get; set; } = default!;
    public string[] Roles { get; set; } = [];
}
