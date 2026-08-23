using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Domain.Entities;

public class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public WorkspaceType WorkspaceType { get; set; } = WorkspaceType.Business;
    public ProfileType ProfileType { get; set; } = ProfileType.Business;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public WorkspaceProfile? Profile { get; set; }
    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    public ICollection<WorkspaceCatalogItem> CatalogItems { get; set; } = new List<WorkspaceCatalogItem>();
    public ICollection<PageGenerationRequest> PageGenerationRequests { get; set; } = new List<PageGenerationRequest>();
    public ICollection<Page> Pages { get; set; } = new List<Page>();
}
