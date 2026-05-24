using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Application.Auth;

public class WorkspaceAccessService : IWorkspaceAccessService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public WorkspaceAccessService(IAppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<bool> IsMemberAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not Guid userId)
            return false;

        return await _dbContext.WorkspaceMembers
            .AnyAsync(
                x => x.WorkspaceId == workspaceId
                    && x.UserId == userId
                    && x.Status == WorkspaceMemberStatus.Active
                    && x.Workspace.IsActive,
                cancellationToken);
    }

    public async Task<bool> HasRoleAsync(Guid workspaceId, string roleCode, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not Guid userId)
            return false;

        return await _dbContext.WorkspaceMembers
            .AnyAsync(
                x => x.WorkspaceId == workspaceId
                    && x.UserId == userId
                    && x.Status == WorkspaceMemberStatus.Active
                    && x.Workspace.IsActive
                    && x.Role.Code == roleCode,
                cancellationToken);
    }

    public async Task<string[]> GetRolesAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not Guid userId)
            return [];

        return await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(
                x => x.WorkspaceId == workspaceId
                    && x.UserId == userId
                    && x.Status == WorkspaceMemberStatus.Active
                    && x.Workspace.IsActive)
            .Select(x => x.Role.Code)
            .OrderBy(x => x)
            .ToArrayAsync(cancellationToken);
    }
}
