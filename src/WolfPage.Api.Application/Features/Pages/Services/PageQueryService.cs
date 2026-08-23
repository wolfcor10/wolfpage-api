using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Pages.Dtos;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Features.Pages.Services;

public class PageQueryService : IPageQueryService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IWorkspaceAccessService _workspaceAccess;

    public PageQueryService(
        IAppDbContext dbContext,
        ICurrentUser currentUser,
        IWorkspaceAccessService workspaceAccess)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<List<PageResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var workspaceId = await GetWorkspaceIdAsync(cancellationToken);

        var pages = await _dbContext.Pages
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return pages.Select(PageDtoMapper.Map).ToList();
    }

    public async Task<PageResponseDto?> GetByIdAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await GetWorkspaceIdAsync(cancellationToken);

        var page = await _dbContext.Pages
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == pageId && x.WorkspaceId == workspaceId, cancellationToken);

        if (page is null)
            return null;

        return PageDtoMapper.Map(page);
    }

    private async Task<Guid> GetWorkspaceIdAsync(CancellationToken cancellationToken)
    {
        var workspaceId = _currentUser.WorkspaceId ?? throw new UnauthorizedAccessException("Workspace no especificado.");

        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        return workspaceId;
    }

}
