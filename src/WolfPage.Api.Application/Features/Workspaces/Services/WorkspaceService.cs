using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Workspaces.Dtos;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Application.Features.Workspaces.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public WorkspaceService(IAppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
        var workspaceType = ParseEnum<WorkspaceType>(dto.WorkspaceType, nameof(dto.WorkspaceType));
        var profileType = ParseEnum<ProfileType>(dto.ProfileType, nameof(dto.ProfileType));
        var adminRole = await _dbContext.Roles.FirstAsync(x => x.Code == "admin", cancellationToken);
        var now = DateTime.UtcNow;

        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            WorkspaceType = workspaceType,
            ProfileType = profileType,
            IsActive = true,
            CreatedAt = now
        };

        workspace.Members.Add(new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = adminRole.Id,
            WorkspaceId = workspace.Id,
            Status = WorkspaceMemberStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
            JoinedAt = now
        });

        _dbContext.Workspaces.Add(workspace);
        await _dbContext.SaveChangesAsync(cancellationToken);

        workspace.Members.First().Role = adminRole;

        return Map(workspace, ["admin"]);
    }

    public async Task<WorkspaceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

        var workspace = await _dbContext.Workspaces
            .AsNoTracking()
            .Include(x => x.Members)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.Id == id
                    && x.Members.Any(member =>
                        member.UserId == userId
                        && member.Status == WorkspaceMemberStatus.Active),
                cancellationToken);

        if (workspace is null)
            return null;

        var roles = workspace.Members
            .Where(x => x.UserId == userId && x.Status == WorkspaceMemberStatus.Active)
            .Select(x => x.Role.Code)
            .OrderBy(x => x)
            .ToArray();

        return Map(workspace, roles);
    }

    public async Task<List<WorkspaceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

        var memberships = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Include(x => x.Workspace)
            .Include(x => x.Role)
            .Where(x => x.UserId == userId && x.Status == WorkspaceMemberStatus.Active && x.Workspace.IsActive)
            .ToListAsync(cancellationToken);

        return memberships
            .GroupBy(x => x.WorkspaceId)
            .Select(group =>
            {
                var workspace = group.First().Workspace;
                var roles = group.Select(x => x.Role.Code).OrderBy(x => x).ToArray();
                return Map(workspace, roles);
            })
            .OrderBy(x => x.Name)
            .ToList();
    }

    private static TEnum ParseEnum<TEnum>(string value, string fieldName)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"{fieldName} '{value}' no es valido.");
    }

    private static WorkspaceDto Map(Workspace workspace, string[] roles) => new()
    {
        Id = workspace.Id,
        Name = workspace.Name,
        Email = workspace.Email,
        WorkspaceType = workspace.WorkspaceType.ToString(),
        ProfileType = workspace.ProfileType.ToString(),
        IsActive = workspace.IsActive,
        CreatedAt = workspace.CreatedAt,
        Roles = roles
    };
}
