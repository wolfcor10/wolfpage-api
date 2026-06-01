using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Pages.Dtos;
using WolfPage.Api.Application.Features.Templates.Services;
using WolfPage.Api.Application.Messaging;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Application.Features.Pages.Services;

public class PageRequestService : IPageRequestService
{
    private const string PageGenerationQueue = "site.generate";
    private static readonly Regex SlugUnsafeChars = new("[^a-z0-9-]+", RegexOptions.Compiled);
    private static readonly Regex SlugRepeatedDashes = new("-+", RegexOptions.Compiled);

    private readonly IAppDbContext _dbContext;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<PageRequestService> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IWorkspaceAccessService _workspaceAccess;

    public PageRequestService(
        IAppDbContext dbContext,
        IMessagePublisher publisher,
        ILogger<PageRequestService> logger,
        ICurrentUser currentUser,
        IWorkspaceAccessService workspaceAccess)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
        _currentUser = currentUser;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<PageRequestResponseDto> CreateAsync(CreatePageRequestDto dto, CancellationToken cancellationToken = default)
    {
        var content = dto.Content.ToDictionary(x => x.Key, x => x.Value);
        var businessName = ReadString(content, "businessName")
            ?? ReadString(content, "title")
            ?? dto.PageName;

        var request = new BusinessPageCreateRequest
        {
            WorkspaceId = dto.WorkspaceId,
            Slug = dto.Slug,
            BusinessName = businessName,
            Category = ReadString(content, "category"),
            Description = ReadString(content, "description")
                ?? ReadString(content, "heroText")
                ?? "Pagina generada desde WolfPage.",
            LogoUrl = ReadString(content, "logoUrl"),
            HeroTitle = ReadString(content, "heroTitle") ?? dto.PageName,
            HeroSubtitle = ReadString(content, "heroSubtitle") ?? ReadString(content, "heroText"),
            HeroImageUrl = ReadString(content, "heroImageUrl"),
            Phone = ReadString(content, "phone"),
            Email = ReadString(content, "email"),
            Address = ReadString(content, "address"),
            WhatsApp = ReadString(content, "whatsapp"),
            OpeningHours = ReadString(content, "openingHours"),
            SelectedTemplateId = dto.SelectedTemplateId,
            Items =
            [
                new BusinessItemDto
                {
                    Name = dto.PageName,
                    Description = ReadString(content, "description") ?? "Oferta principal",
                    Enabled = true
                }
            ]
        };

        return await CreateBusinessPageAsync(request, cancellationToken);
    }

    public async Task<PageRequestResponseDto> CreateBusinessPageAsync(
        BusinessPageCreateRequest dto,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = ResolveWorkspaceId(dto.WorkspaceId);
        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        var workspaceExists = await _dbContext.Workspaces
            .AnyAsync(x => x.Id == workspaceId && x.IsActive, cancellationToken);

        if (!workspaceExists)
            throw new InvalidOperationException($"Workspace {workspaceId} no existe o esta inactivo.");

        var selectedTemplateId = NormalizeTemplateId(dto.SelectedTemplateId);
        if (!string.Equals(selectedTemplateId, TemplateService.GeneralBusinessTemplateId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Template '{dto.SelectedTemplateId}' no esta disponible.");

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? CreateSlug(dto.BusinessName)
            : dto.Slug.Trim().ToLowerInvariant();
        var slugTaken = await _dbContext.Pages
            .AnyAsync(x => x.WorkspaceId == workspaceId && x.Slug == slug, cancellationToken);
        if (slugTaken)
        {
            slug = $"{slug}-{Guid.NewGuid():N}"[..Math.Min(150, slug.Length + 9)];
        }

        var now = DateTime.UtcNow;
        var pageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString("N");
        var requestId = Guid.NewGuid();
        var socialLinksJson = JsonSerializer.Serialize(dto.SocialLinks);

        var page = new Page
        {
            Id = pageId,
            WorkspaceId = workspaceId,
            RequestId = requestId,
            SelectedTemplateId = selectedTemplateId,
            Title = string.IsNullOrWhiteSpace(dto.HeroTitle) ? dto.BusinessName.Trim() : dto.HeroTitle.Trim(),
            Slug = slug,
            RoutePath = $"/{pageId:D}/",
            HtmlContent = null,
            CssContent = null,
            JsContent = null,
            BusinessName = dto.BusinessName.Trim(),
            BusinessCategory = TrimOrNull(dto.Category),
            BusinessDescription = dto.Description.Trim(),
            LogoUrl = TrimOrNull(dto.LogoUrl),
            HeroTitle = string.IsNullOrWhiteSpace(dto.HeroTitle) ? dto.BusinessName.Trim() : dto.HeroTitle.Trim(),
            HeroSubtitle = TrimOrNull(dto.HeroSubtitle),
            HeroImageUrl = TrimOrNull(dto.HeroImageUrl),
            Phone = TrimOrNull(dto.Phone),
            Email = TrimOrNull(dto.Email),
            Address = TrimOrNull(dto.Address),
            WhatsApp = TrimOrNull(dto.WhatsApp),
            OpeningHours = TrimOrNull(dto.OpeningHours),
            SocialLinksJson = socialLinksJson,
            Status = PageStatus.PendingGeneration,
            CreatedAt = now,
            UpdatedAt = now
        };

        var sortOrder = 0;
        foreach (var item in dto.Items)
        {
            page.Items.Add(new PageItem
            {
                Id = Guid.NewGuid(),
                PageId = page.Id,
                Name = item.Name.Trim(),
                Description = item.Description.Trim(),
                Price = TrimOrNull(item.Price),
                ImageUrl = TrimOrNull(item.ImageUrl),
                Category = TrimOrNull(item.Category),
                Enabled = item.Enabled,
                SortOrder = sortOrder++
            });
        }

        var request = new PageGenerationRequest
        {
            Id = requestId,
            WorkspaceId = workspaceId,
            PageId = page.Id,
            SelectedTemplateId = selectedTemplateId,
            CorrelationId = correlationId,
            PageName = page.Title,
            Slug = page.Slug,
            ContentJson = JsonSerializer.Serialize(dto),
            Status = RequestStatus.Pending,
            CreatedAt = now
        };

        _dbContext.Pages.Add(page);
        _dbContext.PageGenerationRequests.Add(request);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var message = new CreatePageRequestedMessage
        {
            RequestId = request.Id,
            CorrelationId = request.CorrelationId
        };

        await _publisher.PublishAsync(message, PageGenerationQueue, cancellationToken);

        _logger.LogInformation(
            "PageGenerationRequest creado y publicado. RequestId={RequestId}, CorrelationId={CorrelationId}",
            request.Id, request.CorrelationId);

        return new PageRequestResponseDto
        {
            RequestId = request.Id,
            PageId = page.Id,
            CorrelationId = request.CorrelationId,
            Status = request.Status.ToString(),
            CreatedAt = request.CreatedAt
        };
    }

    public async Task<PageRequestResponseDto?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var workspaceId = ResolveWorkspaceId(_currentUser.WorkspaceId);
        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        var request = await _dbContext.PageGenerationRequests
            .AsNoTracking()
            .Include(x => x.Page)
            .FirstOrDefaultAsync(x => x.Id == requestId && x.WorkspaceId == workspaceId, cancellationToken);

        if (request is null)
            return null;

        return new PageRequestResponseDto
        {
            RequestId = request.Id,
            PageId = request.PageId,
            CorrelationId = request.CorrelationId,
            Status = request.Status.ToString(),
            ErrorMessage = request.ErrorMessage,
            CreatedAt = request.CreatedAt,
            ProcessedAt = request.ProcessedAt
        };
    }

    private Guid ResolveWorkspaceId(Guid? requestedWorkspaceId)
    {
        if (requestedWorkspaceId is Guid workspaceId && workspaceId != Guid.Empty)
            return workspaceId;

        if (_currentUser.WorkspaceId is Guid currentWorkspaceId && currentWorkspaceId != Guid.Empty)
            return currentWorkspaceId;

        throw new UnauthorizedAccessException("Workspace no especificado.");
    }

    private static string NormalizeTemplateId(string value) => value.Trim().ToLowerInvariant();

    private static string CreateSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        slug = SlugUnsafeChars.Replace(slug, "-");
        slug = SlugRepeatedDashes.Replace(slug, "-").Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? $"page-{Guid.NewGuid():N}"[..13] : slug[..Math.Min(slug.Length, 140)];
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? ReadString(Dictionary<string, object> content, string key)
    {
        if (!content.TryGetValue(key, out var value) || value is null)
            return null;

        return value switch
        {
            string text => text,
            JsonElement element when element.ValueKind == JsonValueKind.String => element.GetString(),
            JsonElement element => element.ToString(),
            _ => value.ToString()
        };
    }
}
