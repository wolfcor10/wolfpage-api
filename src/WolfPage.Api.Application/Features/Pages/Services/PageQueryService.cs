using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Pages.Dtos;
using WolfPage.Api.Application.Persistence;

namespace WolfPage.Api.Application.Features.Pages.Services;

public class PageQueryService : IPageQueryService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PageQueryService(IAppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<List<PageResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        return await _dbContext.Pages
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(page => new PageResponseDto
            {
                Id = page.Id,
                TenantId = page.TenantId,
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
        var tenantId = GetTenantId();

        var page = await _dbContext.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == pageId && x.TenantId == tenantId, cancellationToken);

        if (page is null)
            return null;

        return new PageResponseDto
        {
            Id = page.Id,
            TenantId = page.TenantId,
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

    private Guid GetTenantId() =>
        _currentUser.TenantId ?? throw new UnauthorizedAccessException("Usuario sin tenant.");
}
