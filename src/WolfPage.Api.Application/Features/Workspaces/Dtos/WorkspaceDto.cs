namespace WolfPage.Api.Application.Features.Workspaces.Dtos;

public class WorkspaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string WorkspaceType { get; set; } = default!;
    public string ProfileType { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string[] Roles { get; set; } = [];
}

public class CreateWorkspaceDto
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string WorkspaceType { get; set; } = "Business";
    public string ProfileType { get; set; } = "Business";
}
