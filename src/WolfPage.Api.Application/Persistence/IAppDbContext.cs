using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Persistence;

public interface IAppDbContext
{
    DbSet<Workspace> Workspaces { get; }
    DbSet<WorkspaceProfile> WorkspaceProfiles { get; }
    DbSet<WorkspaceCatalogItem> WorkspaceCatalogItems { get; }
    DbSet<WorkspaceCatalogItemImage> WorkspaceCatalogItemImages { get; }
    DbSet<Template> Templates { get; }
    DbSet<TemplateVersion> TemplateVersions { get; }
    DbSet<PageGenerationRequest> PageGenerationRequests { get; }
    DbSet<Page> Pages { get; }
    DbSet<PageItem> PageItems { get; }
    DbSet<PageAsset> PageAssets { get; }
    DbSet<DomainBinding> DomainBindings { get; }
    DbSet<User> Users { get; }
    DbSet<UserExternalLogin> UserExternalLogins { get; }
    DbSet<UserToken> UserTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<WorkspaceMember> WorkspaceMembers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
