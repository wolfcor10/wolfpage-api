using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Persistence;

public interface IAppDbContext
{
    DbSet<Workspace> Workspaces { get; }
    DbSet<Template> Templates { get; }
    DbSet<TemplateVersion> TemplateVersions { get; }
    DbSet<PageGenerationRequest> PageGenerationRequests { get; }
    DbSet<Page> Pages { get; }
    DbSet<PageAsset> PageAssets { get; }
    DbSet<DomainBinding> DomainBindings { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<WorkspaceMember> WorkspaceMembers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
