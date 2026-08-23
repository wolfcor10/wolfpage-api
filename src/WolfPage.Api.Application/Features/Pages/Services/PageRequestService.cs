using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Pages.Dtos;
using WolfPage.Api.Application.Features.Templates.Services;
using WolfPage.Api.Application.Messaging;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Application.Storage;
using WolfPage.Api.Domain.Entities;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Application.Features.Pages.Services;

public class PageRequestService : IPageRequestService
{
    private const string PageGenerationQueue = "site.generate";
    private const long MaxHeroImageBytes = 5 * 1024 * 1024;
    private static readonly Regex SlugUnsafeChars = new("[^a-z0-9-]+", RegexOptions.Compiled);
    private static readonly Regex SlugRepeatedDashes = new("-+", RegexOptions.Compiled);

    private readonly IAppDbContext _dbContext;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<PageRequestService> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IWorkspaceAccessService _workspaceAccess;
    private readonly IFileStorage _fileStorage;

    public PageRequestService(
        IAppDbContext dbContext,
        IMessagePublisher publisher,
        ILogger<PageRequestService> logger,
        ICurrentUser currentUser,
        IWorkspaceAccessService workspaceAccess,
        IFileStorage fileStorage)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
        _currentUser = currentUser;
        _workspaceAccess = workspaceAccess;
        _fileStorage = fileStorage;
    }

    public async Task<PageRequestResponseDto> CreateAsync(
        CreatePageRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var content = dto.Content.ToDictionary(x => x.Key, x => x.Value);
        var businessName = ReadString(content, "businessName")
            ?? ReadString(content, "title")
            ?? dto.PageName;

        var request = new BusinessPageCreateRequest
        {
            WorkspaceId = dto.WorkspaceId,
            Slug = dto.Slug,
            UseWorkspaceProfile = false,
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

    // Compatibilidad con el endpoint /generate: crea el borrador y lo publica en una sola operacion.
    public async Task<PageRequestResponseDto> CreateBusinessPageAsync(
        BusinessPageCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = await CreateDraftAsync(request, cancellationToken);
        return await PublishAsync(page.Id, cancellationToken);
    }

    public async Task<PageResponseDto> CreateDraftAsync(
        BusinessPageCreateRequest dto,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = ResolveWorkspaceId(dto.WorkspaceId);
        var workspace = await GetWorkspaceAsync(workspaceId, cancellationToken);
        var content = await BuildContentAsync(workspace, dto, allowInactiveCatalogItems: false, cancellationToken);

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? CreateSlug(content.BusinessName)
            : dto.Slug.Trim().ToLowerInvariant();

        if (await _dbContext.Pages.AnyAsync(
                x => x.WorkspaceId == workspaceId && x.Slug == slug,
                cancellationToken))
        {
            slug = AddUniqueSuffix(slug);
        }

        var now = DateTime.UtcNow;
        var page = new Page
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Slug = slug,
            RoutePath = string.Empty,
            Status = PageStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };
        page.RoutePath = $"/{page.Id:D}/";

        ApplyContent(page, content);
        ReplaceItems(page, content.Items);
        page.ContentSnapshotJson = SerializeSnapshot(page);

        _dbContext.Pages.Add(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Borrador de pagina creado. PageId={PageId}, WorkspaceId={WorkspaceId}", page.Id, workspaceId);
        return PageDtoMapper.Map(page);
    }

    public async Task<PageResponseDto?> UpdateDraftAsync(
        Guid pageId,
        BusinessPageCreateRequest dto,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = ResolveWorkspaceId(dto.WorkspaceId);
        var workspace = await GetWorkspaceAsync(workspaceId, cancellationToken);
        var page = await _dbContext.Pages
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == pageId && x.WorkspaceId == workspaceId, cancellationToken);

        if (page is null)
            return null;

        var content = await BuildContentAsync(workspace, dto, allowInactiveCatalogItems: true, cancellationToken);
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? page.Slug
            : dto.Slug.Trim().ToLowerInvariant();

        if (await _dbContext.Pages.AnyAsync(
                x => x.WorkspaceId == workspaceId && x.Id != pageId && x.Slug == slug,
                cancellationToken))
        {
            throw new InvalidOperationException($"El slug '{slug}' ya esta en uso.");
        }

        page.Slug = slug;
        ApplyContent(page, content);
        ReplaceItems(page, content.Items);

        // La edicion representa una version posterior a cualquier generacion en curso.
        page.RequestId = null;
        page.Status = PageStatus.Draft;
        page.UpdatedAt = DateTime.UtcNow;
        page.ContentSnapshotJson = SerializeSnapshot(page);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Borrador de pagina actualizado. PageId={PageId}", page.Id);
        return PageDtoMapper.Map(page);
    }

    public async Task<PageRequestResponseDto> PublishAsync(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = ResolveWorkspaceId(_currentUser.WorkspaceId);
        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        var page = await _dbContext.Pages
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == pageId && x.WorkspaceId == workspaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Pagina {pageId} no encontrada.");

        var hasActiveRequest = await _dbContext.PageGenerationRequests.AnyAsync(
            x => x.PageId == pageId
                && (x.Status == RequestStatus.Pending || x.Status == RequestStatus.Processing),
            cancellationToken);

        if (hasActiveRequest)
            throw new InvalidOperationException("La pagina ya tiene una publicacion en proceso.");

        var now = DateTime.UtcNow;
        var request = new PageGenerationRequest
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            PageId = page.Id,
            TemplateVersionId = page.TemplateVersionId,
            SelectedTemplateId = page.SelectedTemplateId,
            CorrelationId = Guid.NewGuid().ToString("N"),
            PageName = page.Title,
            Slug = page.Slug,
            ContentJson = SerializeSnapshot(page),
            Status = RequestStatus.Pending,
            CreatedAt = now
        };

        page.RequestId = request.Id;
        page.ContentSnapshotJson = request.ContentJson;
        page.Status = PageStatus.PendingGeneration;
        page.UpdatedAt = now;

        _dbContext.PageGenerationRequests.Add(request);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _publisher.PublishAsync(
                new CreatePageRequestedMessage
                {
                    RequestId = request.Id,
                    CorrelationId = request.CorrelationId
                },
                PageGenerationQueue,
                cancellationToken);
        }
        catch (Exception ex)
        {
            request.Status = RequestStatus.Failed;
            request.ErrorMessage = "No se pudo enviar la solicitud al bus de mensajes.";
            request.ProcessedAt = DateTime.UtcNow;
            page.RequestId = null;
            page.Status = PageStatus.Draft;
            page.UpdatedAt = request.ProcessedAt.Value;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogError(ex, "No se pudo publicar la solicitud {RequestId} en el bus.", request.Id);
            throw;
        }

        _logger.LogInformation(
            "Publicacion solicitada. RequestId={RequestId}, PageId={PageId}, CorrelationId={CorrelationId}",
            request.Id, page.Id, request.CorrelationId);

        return MapRequest(request);
    }

    public async Task<PageRequestResponseDto?> GetByIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = ResolveWorkspaceId(_currentUser.WorkspaceId);
        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        var request = await _dbContext.PageGenerationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == requestId && x.WorkspaceId == workspaceId, cancellationToken);

        return request is null ? null : MapRequest(request);
    }

    public async Task<PageResponseDto?> UploadHeroImageAsync(
        Guid pageId,
        PageHeroImageUpload upload,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = ResolveWorkspaceId(_currentUser.WorkspaceId);
        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        var page = await _dbContext.Pages
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == pageId && x.WorkspaceId == workspaceId, cancellationToken);

        if (page is null)
            return null;

        var contentType = NormalizeHeroImageContentType(upload.ContentType);
        if (upload.Length <= 0 || upload.Length > MaxHeroImageBytes)
            throw new InvalidOperationException("La imagen de portada debe pesar entre 1 byte y 5 MB.");

        await using var bufferedContent = new MemoryStream((int)upload.Length);
        await upload.Content.CopyToAsync(bufferedContent, cancellationToken);
        if (bufferedContent.Length != upload.Length)
            throw new InvalidOperationException("No fue posible leer la imagen de portada completa.");

        bufferedContent.Position = 0;
        ValidateHeroImageSignature(bufferedContent, contentType);
        bufferedContent.Position = 0;

        var extension = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new InvalidOperationException("Formato de imagen no permitido.")
        };
        var newStoragePath = $"workspaces/{workspaceId:N}/pages/{pageId:N}/hero/{Guid.NewGuid():N}{extension}";
        var previousStoragePath = page.HeroImageStoragePath;

        await _fileStorage.UploadAsync(newStoragePath, bufferedContent, contentType, cancellationToken);

        try
        {
            page.HeroImageStoragePath = newStoragePath;
            page.HeroImageUrl = null;
            page.Status = PageStatus.Draft;
            page.UpdatedAt = DateTime.UtcNow;
            page.ContentSnapshotJson = SerializeSnapshot(page);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _fileStorage.DeleteIfExistsAsync(newStoragePath, cancellationToken);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(previousStoragePath))
            await _fileStorage.DeleteIfExistsAsync(previousStoragePath, cancellationToken);

        return PageDtoMapper.Map(page);
    }

    public async Task<PageHeroImageContent?> GetHeroImageAsync(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = ResolveWorkspaceId(_currentUser.WorkspaceId);
        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        var storagePath = await _dbContext.Pages
            .AsNoTracking()
            .Where(x => x.Id == pageId && x.WorkspaceId == workspaceId)
            .Select(x => x.HeroImageStoragePath)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(storagePath))
            return null;

        var storedFile = await _fileStorage.OpenReadAsync(storagePath, cancellationToken);
        return storedFile is null
            ? null
            : new PageHeroImageContent(storedFile.Content, storedFile.ContentType);
    }

    public async Task<bool> DeleteHeroImageAsync(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = ResolveWorkspaceId(_currentUser.WorkspaceId);
        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        var page = await _dbContext.Pages
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == pageId && x.WorkspaceId == workspaceId, cancellationToken);

        if (page is null)
            return false;

        var storagePath = page.HeroImageStoragePath;
        page.HeroImageStoragePath = null;
        page.HeroImageUrl = null;
        page.Status = PageStatus.Draft;
        page.UpdatedAt = DateTime.UtcNow;
        page.ContentSnapshotJson = SerializeSnapshot(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(storagePath))
            await _fileStorage.DeleteIfExistsAsync(storagePath, cancellationToken);

        return true;
    }

    private async Task<Workspace> GetWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");

        return await _dbContext.Workspaces
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Id == workspaceId && x.IsActive, cancellationToken)
            ?? throw new InvalidOperationException($"Workspace {workspaceId} no existe o esta inactivo.");
    }

    private async Task<PageContent> BuildContentAsync(
        Workspace workspace,
        BusinessPageCreateRequest dto,
        bool allowInactiveCatalogItems,
        CancellationToken cancellationToken)
    {
        var profile = dto.UseWorkspaceProfile ? workspace.Profile : null;
        var selectedTemplateId = NormalizeTemplateId(dto.SelectedTemplateId);

        if (!string.Equals(selectedTemplateId, TemplateService.GeneralBusinessTemplateId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Template '{dto.SelectedTemplateId}' no esta disponible.");

        var businessName = RequiredText("businessName", dto.BusinessName, profile?.DisplayName, workspace.Name);
        var description = RequiredText("description", dto.Description, profile?.Description);
        var heroTitle = RequiredText("heroTitle", dto.HeroTitle, businessName);
        var socialLinks = MergeSocialLinks(dto.SocialLinks, profile);
        var items = await BuildPageItemsAsync(workspace.Id, dto, allowInactiveCatalogItems, cancellationToken);

        return new PageContent(
            selectedTemplateId,
            businessName,
            TrimOrNull(FirstText(dto.Category, profile?.Category)),
            description,
            TrimOrNull(FirstText(dto.LogoUrl, profile?.LogoUrl)),
            heroTitle,
            TrimOrNull(FirstText(dto.HeroSubtitle, profile?.Description)),
            TrimOrNull(FirstText(dto.HeroImageUrl, profile?.CoverImageUrl)),
            TrimOrNull(FirstText(dto.Phone, profile?.Phone)),
            TrimOrNull(FirstText(dto.Email, profile?.Email)),
            TrimOrNull(FirstText(dto.Address, profile?.Address)),
            TrimOrNull(FirstText(dto.WhatsApp, profile?.WhatsApp)),
            TrimOrNull(FirstText(dto.OpeningHours, profile?.OpeningHours)),
            JsonSerializer.Serialize(socialLinks),
            items);
    }

    private void ReplaceItems(Page page, IReadOnlyCollection<PageItemInput> items)
    {
        if (page.Items.Count > 0)
        {
            _dbContext.PageItems.RemoveRange(page.Items);
            page.Items.Clear();
        }

        var sortOrder = 0;
        foreach (var item in items)
        {
            page.Items.Add(new PageItem
            {
                Id = Guid.NewGuid(),
                PageId = page.Id,
                SourceCatalogItemId = item.SourceCatalogItemId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                Category = item.Category,
                Enabled = true,
                SortOrder = sortOrder++
            });
        }
    }

    private static void ApplyContent(Page page, PageContent content)
    {
        page.SelectedTemplateId = content.SelectedTemplateId;
        page.Title = content.HeroTitle;
        page.BusinessName = content.BusinessName;
        page.BusinessCategory = content.BusinessCategory;
        page.BusinessDescription = content.BusinessDescription;
        page.LogoUrl = content.LogoUrl;
        page.HeroTitle = content.HeroTitle;
        page.HeroSubtitle = content.HeroSubtitle;
        page.HeroImageUrl = content.HeroImageUrl;
        page.Phone = content.Phone;
        page.Email = content.Email;
        page.Address = content.Address;
        page.WhatsApp = content.WhatsApp;
        page.OpeningHours = content.OpeningHours;
        page.SocialLinksJson = content.SocialLinksJson;
    }

    private static string SerializeSnapshot(Page page) => JsonSerializer.Serialize(new PageGenerationSnapshotDto
    {
        PageId = page.Id,
        WorkspaceId = page.WorkspaceId,
        TemplateVersionId = page.TemplateVersionId,
        SelectedTemplateId = page.SelectedTemplateId,
        Title = page.Title,
        Slug = page.Slug,
        BusinessName = page.BusinessName,
        BusinessCategory = page.BusinessCategory,
        BusinessDescription = page.BusinessDescription,
        LogoUrl = page.LogoUrl,
        HeroTitle = page.HeroTitle,
        HeroSubtitle = page.HeroSubtitle,
        HeroImageUrl = page.HeroImageUrl,
        HeroImageStoragePath = page.HeroImageStoragePath,
        Phone = page.Phone,
        Email = page.Email,
        Address = page.Address,
        WhatsApp = page.WhatsApp,
        OpeningHours = page.OpeningHours,
        SocialLinksJson = page.SocialLinksJson,
        Items = page.Items
            .OrderBy(x => x.SortOrder)
            .Select(item => new PageGenerationSnapshotItemDto
            {
                SourceCatalogItemId = item.SourceCatalogItemId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                Category = item.Category,
                Enabled = item.Enabled,
                SortOrder = item.SortOrder
            })
            .ToList()
    });

    private Guid ResolveWorkspaceId(Guid? requestedWorkspaceId)
    {
        if (requestedWorkspaceId is Guid workspaceId && workspaceId != Guid.Empty)
            return workspaceId;

        if (_currentUser.WorkspaceId is Guid currentWorkspaceId && currentWorkspaceId != Guid.Empty)
            return currentWorkspaceId;

        throw new UnauthorizedAccessException("Workspace no especificado.");
    }

    private static PageRequestResponseDto MapRequest(PageGenerationRequest request) => new()
    {
        RequestId = request.Id,
        PageId = request.PageId,
        CorrelationId = request.CorrelationId,
        Status = request.Status.ToString(),
        ErrorMessage = request.ErrorMessage,
        CreatedAt = request.CreatedAt,
        ProcessedAt = request.ProcessedAt
    };

    private static string NormalizeTemplateId(string value) => value.Trim().ToLowerInvariant();

    private static string CreateSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        slug = SlugUnsafeChars.Replace(slug, "-");
        slug = SlugRepeatedDashes.Replace(slug, "-").Trim('-');

        return string.IsNullOrWhiteSpace(slug)
            ? $"page-{Guid.NewGuid():N}"[..13]
            : slug[..Math.Min(slug.Length, 140)];
    }

    private static string AddUniqueSuffix(string slug)
    {
        var suffix = $"-{Guid.NewGuid():N}"[..9];
        var prefixLength = Math.Min(slug.Length, 150 - suffix.Length);
        return $"{slug[..prefixLength]}{suffix}";
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<List<PageItemInput>> BuildPageItemsAsync(
        Guid workspaceId,
        BusinessPageCreateRequest dto,
        bool allowInactiveCatalogItems,
        CancellationToken cancellationToken)
    {
        var items = new List<PageItemInput>();
        var catalogItemIds = dto.CatalogItemIds.Distinct().ToArray();

        if (catalogItemIds.Length > 0)
        {
            var catalogItems = await _dbContext.WorkspaceCatalogItems
                .AsNoTracking()
                .Where(x => x.WorkspaceId == workspaceId
                    && (allowInactiveCatalogItems || x.IsActive)
                    && catalogItemIds.Contains(x.Id))
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            if (catalogItems.Count != catalogItemIds.Length)
                throw new InvalidOperationException("Uno o mas items del catalogo no existen o estan inactivos.");

            items.AddRange(catalogItems.Select(item => new PageItemInput(
                item.Id,
                item.Name.Trim(),
                item.Description.Trim(),
                TrimOrNull(item.PriceLabel),
                TrimOrNull(item.ImageUrl),
                TrimOrNull(item.Category))));
        }

        items.AddRange(dto.Items
            .Where(x => x.Enabled)
            .Select(item => new PageItemInput(
                null,
                item.Name.Trim(),
                item.Description.Trim(),
                TrimOrNull(item.Price),
                TrimOrNull(item.ImageUrl),
                TrimOrNull(item.Category))));

        if (items.Count == 0)
            throw new InvalidOperationException("Debes agregar al menos un item habilitado o seleccionar un item del catalogo.");

        return items;
    }

    private static SocialLinksDto MergeSocialLinks(SocialLinksDto socialLinks, WorkspaceProfile? profile) => new()
    {
        Facebook = FirstText(socialLinks.Facebook, profile?.FacebookUrl),
        Instagram = FirstText(socialLinks.Instagram, profile?.InstagramUrl),
        Tiktok = FirstText(socialLinks.Tiktok, profile?.TiktokUrl),
        Linkedin = FirstText(socialLinks.Linkedin, profile?.LinkedinUrl),
        Website = FirstText(socialLinks.Website, profile?.WebsiteUrl)
    };

    private static string RequiredText(string fieldName, params string?[] values) =>
        FirstText(values) ?? throw new InvalidOperationException($"{fieldName} es requerido.");

    private static string? FirstText(params string?[] values) =>
        values.Select(TrimOrNull).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

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

    private static string NormalizeHeroImageContentType(string contentType)
    {
        var normalized = contentType.Trim().ToLowerInvariant();
        if (normalized is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new InvalidOperationException("Solo se permiten portadas JPG, PNG o WebP.");

        return normalized;
    }

    private static void ValidateHeroImageSignature(Stream content, string contentType)
    {
        Span<byte> header = stackalloc byte[12];
        var bytesRead = content.Read(header);
        content.Position = 0;

        var valid = contentType switch
        {
            "image/jpeg" => bytesRead >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
            "image/png" => bytesRead >= 8
                && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }),
            "image/webp" => bytesRead >= 12
                && header[..4].SequenceEqual("RIFF"u8)
                && header[8..12].SequenceEqual("WEBP"u8),
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException("El archivo seleccionado no es una imagen de portada valida.");
    }

    private sealed record PageContent(
        string SelectedTemplateId,
        string BusinessName,
        string? BusinessCategory,
        string BusinessDescription,
        string? LogoUrl,
        string HeroTitle,
        string? HeroSubtitle,
        string? HeroImageUrl,
        string? Phone,
        string? Email,
        string? Address,
        string? WhatsApp,
        string? OpeningHours,
        string SocialLinksJson,
        IReadOnlyCollection<PageItemInput> Items);

    private sealed record PageItemInput(
        Guid? SourceCatalogItemId,
        string Name,
        string Description,
        string? Price,
        string? ImageUrl,
        string? Category);
}
