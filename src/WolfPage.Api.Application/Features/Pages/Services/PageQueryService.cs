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

        return pages.Select(Map).ToList();
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

        return Map(page);
    }

    private async Task<Guid> GetWorkspaceIdAsync(CancellationToken cancellationToken)
    {
        var workspaceId = _currentUser.WorkspaceId ?? throw new UnauthorizedAccessException("Workspace no especificado.");

        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        return workspaceId;
    }

    private static PageResponseDto Map(Page page) => new()
    {
        Id = page.Id,
        WorkspaceId = page.WorkspaceId,
        TemplateVersionId = page.TemplateVersionId,
        RequestId = page.RequestId,
        SelectedTemplateId = page.SelectedTemplateId,
        Title = page.Title,
        Slug = page.Slug,
        RoutePath = page.RoutePath,
        HtmlContent = page.HtmlContent,
        CssContent = page.CssContent,
        JsContent = page.JsContent,
        BusinessName = page.BusinessName,
        BusinessCategory = page.BusinessCategory,
        BusinessDescription = page.BusinessDescription,
        LogoUrl = page.LogoUrl,
        HeroTitle = page.HeroTitle,
        HeroSubtitle = page.HeroSubtitle,
        HeroImageUrl = page.HeroImageUrl,
        Phone = page.Phone,
        Email = page.Email,
        Address = page.Address,
        WhatsApp = page.WhatsApp,
        OpeningHours = page.OpeningHours,
        SocialLinksJson = page.SocialLinksJson,
        GeneratedFilePath = page.GeneratedFilePath,
        Status = page.Status.ToString(),
        PublishedUrl = page.PublishedUrl,
        CreatedAt = page.CreatedAt,
        UpdatedAt = page.UpdatedAt,
        Items = page.Items
            .OrderBy(x => x.SortOrder)
            .Select(item => new PageItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                Category = item.Category,
                Enabled = item.Enabled,
                SortOrder = item.SortOrder
            })
            .ToList()
    };
}
