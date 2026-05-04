using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Pages.Dtos;
using WolfPage.Api.Application.Persistence;

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

        return await _dbContext.Pages
            .AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(page => new PageResponseDto
            {
                Id = page.Id,
                WorkspaceId = page.WorkspaceId,
                TemplateVersionId = page.TemplateVersionId,
                RequestId = page.RequestId,
                Title = page.Title,
                Slug = page.Slug,
                RoutePath = page.RoutePath,
                HtmlContent = page.HtmlContent,
                CssContent = page.CssContent,
                JsContent = page.JsContent,
                Status = page.Status.ToString(),
                PublishedUrl = page.PublishedUrl,
                CreatedAt = page.CreatedAt,
                UpdatedAt = page.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PageResponseDto?> GetByIdAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await GetWorkspaceIdAsync(cancellationToken);

        var page = await _dbContext.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == pageId && x.WorkspaceId == workspaceId, cancellationToken);

        if (page is null)
            return null;

        return new PageResponseDto
        {
            Id = page.Id,
            WorkspaceId = page.WorkspaceId,
            TemplateVersionId = page.TemplateVersionId,
            RequestId = page.RequestId,
            Title = page.Title,
            Slug = page.Slug,
            RoutePath = page.RoutePath,
            HtmlContent = page.HtmlContent,
            CssContent = page.CssContent,
            JsContent = page.JsContent,
            Status = page.Status.ToString(),
            PublishedUrl = page.PublishedUrl,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }

    private async Task<Guid> GetWorkspaceIdAsync(CancellationToken cancellationToken)
    {
        var workspaceId = _currentUser.WorkspaceId ?? throw new UnauthorizedAccessException("Workspace no especificado.");

        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        return workspaceId;
    }
}
