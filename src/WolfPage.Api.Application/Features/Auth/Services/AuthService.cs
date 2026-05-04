using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Auth.Dtos;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentUser _currentUser;

    public AuthService(
        IAppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _currentUser = currentUser;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Workspace)
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email && x.IsActive, cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Credenciales invalidas.");

        if (!_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            throw new UnauthorizedAccessException("Credenciales invalidas.");

        var activeMemberships = GetActiveMemberships(user);
        if (!activeMemberships.Any())
            throw new UnauthorizedAccessException("El usuario no tiene workspaces activos.");

        var activeWorkspaceId = ResolveActiveWorkspaceId(activeMemberships, request.WorkspaceId);

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = user.LastLoginAt.Value;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            User = MapCurrentUser(user, activeWorkspaceId)
        };
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return null;

        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Workspace)
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == _currentUser.UserId.Value && x.IsActive, cancellationToken);

        if (user is null)
            return null;

        var activeMemberships = GetActiveMemberships(user);
        var activeWorkspaceId = ResolveActiveWorkspaceId(activeMemberships, _currentUser.WorkspaceId);

        return MapCurrentUser(user, activeWorkspaceId);
    }

    private static Guid? ResolveActiveWorkspaceId(
        IReadOnlyCollection<WorkspaceMember> activeMemberships,
        Guid? requestedWorkspaceId)
    {
        if (!activeMemberships.Any())
            return null;

        if (requestedWorkspaceId.HasValue
            && activeMemberships.Any(x => x.WorkspaceId == requestedWorkspaceId.Value))
        {
            return requestedWorkspaceId.Value;
        }

        return activeMemberships
            .OrderBy(x => x.Workspace.Name)
            .Select(x => x.WorkspaceId)
            .First();
    }

    private static WorkspaceMember[] GetActiveMemberships(User user) =>
        user.WorkspaceMemberships
            .Where(x => x.Status == WorkspaceMemberStatus.Active && x.Workspace.IsActive)
            .ToArray();

    private static CurrentUserDto MapCurrentUser(User user, Guid? activeWorkspaceId)
    {
        var memberships = GetActiveMemberships(user);
        var roles = activeWorkspaceId.HasValue
            ? memberships
                .Where(x => x.WorkspaceId == activeWorkspaceId.Value)
                .Select(x => x.Role.Code)
                .Distinct()
                .OrderBy(x => x)
                .ToArray()
            : [];

        return new CurrentUserDto
        {
            Id = user.Id,
            ActiveWorkspaceId = activeWorkspaceId,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles,
            Workspaces = memberships
                .GroupBy(x => x.WorkspaceId)
                .Select(group =>
                {
                    var workspace = group.First().Workspace;
                    return new CurrentUserWorkspaceDto
                    {
                        Id = workspace.Id,
                        Name = workspace.Name,
                        Email = workspace.Email,
                        WorkspaceType = workspace.WorkspaceType.ToString(),
                        ProfileType = workspace.ProfileType.ToString(),
                        Roles = group
                            .Select(x => x.Role.Code)
                            .Distinct()
                            .OrderBy(x => x)
                            .ToArray()
                    };
                })
                .OrderBy(x => x.Name)
                .ToArray()
        };
    }
}
