using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Domain.Entities;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Infrastructure.Persistence;

public class AuthDataSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthDataSeeder(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var roles = new[]
        {
            new RoleSeed("admin", "Administrador", "Acceso administrativo completo."),
            new RoleSeed("editor", "Editor", "Puede gestionar templates y paginas."),
            new RoleSeed("viewer", "Lector", "Puede consultar informacion del portal.")
        };

        foreach (var roleSeed in roles)
        {
            var exists = await _dbContext.Roles.AnyAsync(x => x.Code == roleSeed.Code, cancellationToken);
            if (!exists)
            {
                _dbContext.Roles.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    Code = roleSeed.Code,
                    Name = roleSeed.Name,
                    Description = roleSeed.Description,
                    CreatedAt = now
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var adminEmail = Read("AuthSeed:AdminEmail", "admin@wolfpage.local").Trim().ToLowerInvariant();
        var adminPassword = Read("AuthSeed:AdminPassword", "Admin123!");
        var adminFullName = Read("AuthSeed:AdminFullName", "WolfPage Admin");
        var workspaceName = Read("AuthSeed:WorkspaceName", "WolfPage Demo");
        var workspaceEmail = Read("AuthSeed:WorkspaceEmail", adminEmail).Trim().ToLowerInvariant();

        var workspace = await _dbContext.Workspaces.FirstOrDefaultAsync(cancellationToken);
        if (workspace is null)
        {
            workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = workspaceName,
                Email = workspaceEmail,
                WorkspaceType = WorkspaceType.Business,
                ProfileType = ProfileType.Business,
                IsActive = true,
                CreatedAt = now
            };

            _dbContext.Workspaces.Add(workspace);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var adminRole = await _dbContext.Roles.FirstAsync(x => x.Code == "admin", cancellationToken);
        var adminUser = await _dbContext.Users
            .Include(x => x.WorkspaceMemberships)
            .FirstOrDefaultAsync(x => x.Email == adminEmail, cancellationToken);

        if (adminUser is null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                FullName = adminFullName,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, adminPassword);

            _dbContext.Users.Add(adminUser);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var hasAdminMembership = adminUser.WorkspaceMemberships.Any(
            x => x.WorkspaceId == workspace.Id
                && x.RoleId == adminRole.Id
                && x.Status == WorkspaceMemberStatus.Active);

        if (!hasAdminMembership)
        {
            adminUser.WorkspaceMemberships.Add(new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                WorkspaceId = workspace.Id,
                Status = WorkspaceMemberStatus.Active,
                CreatedAt = now,
                UpdatedAt = now,
                JoinedAt = now
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private string Read(string key, string fallback) =>
        _configuration[key] is { Length: > 0 } value ? value : fallback;

    private sealed record RoleSeed(string Code, string Name, string Description);
}
