using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Domain.Entities;

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
        var tenantName = Read("AuthSeed:TenantName", "WolfPage Demo");
        var tenantEmail = Read("AuthSeed:TenantEmail", adminEmail).Trim().ToLowerInvariant();

        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(cancellationToken);
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = tenantName,
                Email = tenantEmail,
                IsActive = true,
                CreatedAt = now
            };

            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var adminRole = await _dbContext.Roles.FirstAsync(x => x.Code == "admin", cancellationToken);
        var adminUser = await _dbContext.Users
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Email == adminEmail, cancellationToken);

        if (adminUser is null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = adminEmail,
                FullName = adminFullName,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, adminPassword);
            adminUser.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = now
            });

            _dbContext.Users.Add(adminUser);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var hasAdminRole = adminUser.UserRoles.Any(x => x.RoleId == adminRole.Id);
        if (!hasAdminRole)
        {
            adminUser.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = now
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private string Read(string key, string fallback) =>
        _configuration[key] is { Length: > 0 } value ? value : fallback;

    private sealed record RoleSeed(string Code, string Name, string Description);
}
