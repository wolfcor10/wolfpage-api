using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Persistence;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Template> Templates { get; }
    DbSet<TemplateVersion> TemplateVersions { get; }
    DbSet<PageGenerationRequest> PageGenerationRequests { get; }
    DbSet<Page> Pages { get; }
    DbSet<PageAsset> PageAssets { get; }
    DbSet<DomainBinding> DomainBindings { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
