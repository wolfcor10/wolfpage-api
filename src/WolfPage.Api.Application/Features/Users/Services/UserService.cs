using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Users.Dtos;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Features.Users.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IAppDbContext dbContext,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        var users = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return users.Select(Map).ToList();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var email = request.Email.Trim().ToLowerInvariant();
        var roleCodes = request.Roles.Length == 0
            ? ["viewer"]
            : request.Roles.Select(x => x.Trim().ToLowerInvariant()).Distinct().ToArray();

        var emailTaken = await _dbContext.Users
            .AnyAsync(x => x.TenantId == tenantId && x.Email == email, cancellationToken);

        if (emailTaken)
            throw new InvalidOperationException($"Ya existe un usuario con el email '{email}'.");

        var roles = await _dbContext.Roles
            .Where(x => roleCodes.Contains(x.Code))
            .ToListAsync(cancellationToken);

        if (roles.Count != roleCodes.Length)
            throw new InvalidOperationException("Uno o mas roles no existen.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            FullName = request.FullName.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                AssignedAt = now
            });
        }

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        user.UserRoles = roles
            .Select(role => new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                Role = role,
                User = user,
                AssignedAt = now
            })
            .ToList();

        return Map(user);
    }

    public async Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);
    }

    private Guid GetTenantId() =>
        _currentUser.TenantId ?? throw new UnauthorizedAccessException("Usuario sin tenant.");

    private static UserDto Map(User user) => new()
    {
        Id = user.Id,
        TenantId = user.TenantId,
        Email = user.Email,
        FullName = user.FullName,
        IsActive = user.IsActive,
        LastLoginAt = user.LastLoginAt,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        Roles = user.UserRoles
            .Select(x => x.Role.Code)
            .OrderBy(x => x)
            .ToArray()
    };
}
