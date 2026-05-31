using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Users.Dtos;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Application.Features.Users.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IWorkspaceAccessService _workspaceAccess;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IAppDbContext dbContext,
        ICurrentUser currentUser,
        IWorkspaceAccessService workspaceAccess,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _workspaceAccess = workspaceAccess;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var workspaceId = await GetAdminWorkspaceIdAsync(cancellationToken);

        var memberships = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.WorkspaceId == workspaceId && x.Status == WorkspaceMemberStatus.Active)
            .ToListAsync(cancellationToken);

        return memberships
            .GroupBy(x => x.UserId)
            .Select(group => Map(group.First().User, workspaceId, group))
            .OrderBy(x => x.FullName)
            .ToList();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var workspaceId = await GetAdminWorkspaceIdAsync(cancellationToken);
        var email = request.Email.Trim().ToLowerInvariant();
        var roleCodes = request.Roles.Length == 0
            ? ["viewer"]
            : request.Roles.Select(x => x.Trim().ToLowerInvariant()).Distinct().ToArray();

        var roles = await _dbContext.Roles
            .Where(x => roleCodes.Contains(x.Code))
            .ToListAsync(cancellationToken);

        if (roles.Count != roleCodes.Length)
            throw new InvalidOperationException("Uno o mas roles no existen.");

        var now = DateTime.UtcNow;
        var user = await _dbContext.Users
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = request.FullName.Trim(),
                IsActive = true,
                EmailConfirmed = true,
                EmailConfirmedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _dbContext.Users.Add(user);
        }
        else if (user.WorkspaceMemberships.Any(x => x.WorkspaceId == workspaceId && x.Status == WorkspaceMemberStatus.Active))
        {
            throw new InvalidOperationException($"El usuario '{email}' ya pertenece a este workspace.");
        }

        foreach (var role in roles)
        {
            user.WorkspaceMemberships.Add(new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = role.Id,
                Role = role,
                WorkspaceId = workspaceId,
                InvitedByUserId = _currentUser.UserId,
                Status = WorkspaceMemberStatus.Active,
                CreatedAt = now,
                UpdatedAt = now,
                JoinedAt = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(user, workspaceId, user.WorkspaceMemberships.Where(x => x.WorkspaceId == workspaceId));
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

    private async Task<Guid> GetAdminWorkspaceIdAsync(CancellationToken cancellationToken)
    {
        var workspaceId = _currentUser.WorkspaceId ?? throw new UnauthorizedAccessException("Workspace no especificado.");

        if (!await _workspaceAccess.HasRoleAsync(workspaceId, "admin", cancellationToken))
            throw new UnauthorizedAccessException("Se requiere rol admin en el workspace.");

        return workspaceId;
    }

    private static UserDto Map(User user, Guid workspaceId, IEnumerable<WorkspaceMember> memberships) => new()
    {
        Id = user.Id,
        WorkspaceId = workspaceId,
        Email = user.Email,
        FullName = user.FullName,
        IsActive = user.IsActive,
        LastLoginAt = user.LastLoginAt,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        Roles = memberships
            .Where(x => x.Status == WorkspaceMemberStatus.Active)
            .Select(x => x.Role.Code)
            .OrderBy(x => x)
            .ToArray()
    };
}
