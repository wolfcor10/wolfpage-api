using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Workspaces.Dtos;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;
using WolfPage.Api.Domain.Enums;
using WolfPage.Api.Application.Storage;

namespace WolfPage.Api.Application.Features.Workspaces.Services;

public class WorkspaceContentService : IWorkspaceContentService
{
    private readonly IAppDbContext _dbContext;
    private readonly IWorkspaceAccessService _workspaceAccess;
    private readonly IFileStorage _fileStorage;

    private const long MaxImageBytes = 5 * 1024 * 1024;

    public WorkspaceContentService(
        IAppDbContext dbContext,
        IWorkspaceAccessService workspaceAccess,
        IFileStorage fileStorage)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccess;
        _fileStorage = fileStorage;
    }

    public async Task<WorkspaceProfileDto?> GetProfileAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(workspaceId, cancellationToken);

        var profile = await _dbContext.WorkspaceProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId, cancellationToken);

        return profile is null ? null : Map(profile);
    }

    public async Task<WorkspaceProfileDto> UpsertProfileAsync(
        Guid workspaceId,
        UpsertWorkspaceProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanManageAsync(workspaceId, cancellationToken);

        var now = DateTime.UtcNow;
        var profile = await _dbContext.WorkspaceProfiles
            .FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId, cancellationToken);

        if (profile is null)
        {
            profile = new WorkspaceProfile
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                CreatedAt = now
            };

            _dbContext.WorkspaceProfiles.Add(profile);
        }

        profile.DisplayName = request.DisplayName.Trim();
        profile.LegalName = TrimOrNull(request.LegalName);
        profile.Category = TrimOrNull(request.Category);
        profile.Description = request.Description.Trim();
        profile.LogoUrl = TrimOrNull(request.LogoUrl);
        profile.CoverImageUrl = TrimOrNull(request.CoverImageUrl);
        profile.Phone = TrimOrNull(request.Phone);
        profile.Email = NormalizeEmailOrNull(request.Email);
        profile.WhatsApp = TrimOrNull(request.WhatsApp);
        profile.Address = TrimOrNull(request.Address);
        profile.OpeningHours = TrimOrNull(request.OpeningHours);
        profile.WebsiteUrl = TrimOrNull(request.WebsiteUrl);
        profile.FacebookUrl = TrimOrNull(request.FacebookUrl);
        profile.InstagramUrl = TrimOrNull(request.InstagramUrl);
        profile.TiktokUrl = TrimOrNull(request.TiktokUrl);
        profile.LinkedinUrl = TrimOrNull(request.LinkedinUrl);
        profile.PrimaryColor = TrimOrNull(request.PrimaryColor);
        profile.SecondaryColor = TrimOrNull(request.SecondaryColor);
        profile.AccentColor = TrimOrNull(request.AccentColor);
        profile.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(profile);
    }

    public async Task<List<WorkspaceCatalogItemDto>> GetCatalogItemsAsync(
        Guid workspaceId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(workspaceId, cancellationToken);

        var query = _dbContext.WorkspaceCatalogItems
            .AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId);

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    public async Task<WorkspaceCatalogItemDto> CreateCatalogItemAsync(
        Guid workspaceId,
        UpsertWorkspaceCatalogItemRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanManageAsync(workspaceId, cancellationToken);

        var now = DateTime.UtcNow;
        var item = new WorkspaceCatalogItem
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            CreatedAt = now
        };

        Apply(item, request, now);
        _dbContext.WorkspaceCatalogItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(item);
    }

    public async Task<WorkspaceCatalogItemDto?> UpdateCatalogItemAsync(
        Guid workspaceId,
        Guid itemId,
        UpsertWorkspaceCatalogItemRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanManageAsync(workspaceId, cancellationToken);

        var item = await _dbContext.WorkspaceCatalogItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.WorkspaceId == workspaceId, cancellationToken);

        if (item is null)
            return null;

        Apply(item, request, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(item);
    }

    public async Task<bool> DeleteCatalogItemAsync(
        Guid workspaceId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanManageAsync(workspaceId, cancellationToken);

        var item = await _dbContext.WorkspaceCatalogItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.WorkspaceId == workspaceId, cancellationToken);

        if (item is null)
            return false;

        item.IsActive = false;
        item.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<WorkspaceCatalogItemDto?> UploadCatalogItemImageAsync(
        Guid workspaceId,
        Guid itemId,
        CatalogItemImageUpload upload,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanManageAsync(workspaceId, cancellationToken);

        var item = await _dbContext.WorkspaceCatalogItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.WorkspaceId == workspaceId, cancellationToken);

        if (item is null)
            return null;

        var contentType = NormalizeImageContentType(upload.ContentType);
        if (upload.Length <= 0 || upload.Length > MaxImageBytes)
            throw new InvalidOperationException("La imagen debe pesar entre 1 byte y 5 MB.");

        await using var bufferedContent = new MemoryStream((int)upload.Length);
        await upload.Content.CopyToAsync(bufferedContent, cancellationToken);
        if (bufferedContent.Length != upload.Length)
            throw new InvalidOperationException("No fue posible leer la imagen completa.");

        bufferedContent.Position = 0;
        ValidateImageSignature(bufferedContent, contentType);
        bufferedContent.Position = 0;

        var extension = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new InvalidOperationException("Formato de imagen no permitido.")
        };
        var newStoragePath = $"workspaces/{workspaceId:N}/catalog/{itemId:N}/{Guid.NewGuid():N}{extension}";
        var previousStoragePath = item.ImageStoragePath;

        await _fileStorage.UploadAsync(newStoragePath, bufferedContent, contentType, cancellationToken);

        try
        {
            item.ImageStoragePath = newStoragePath;
            item.ImageUrl = null;
            item.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _fileStorage.DeleteIfExistsAsync(newStoragePath, cancellationToken);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(previousStoragePath))
            await _fileStorage.DeleteIfExistsAsync(previousStoragePath, cancellationToken);

        return Map(item);
    }

    public async Task<CatalogItemImageContent?> GetCatalogItemImageAsync(
        Guid workspaceId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(workspaceId, cancellationToken);

        var storagePath = await _dbContext.WorkspaceCatalogItems
            .AsNoTracking()
            .Where(x => x.Id == itemId && x.WorkspaceId == workspaceId)
            .Select(x => x.ImageStoragePath)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(storagePath))
            return null;

        var storedFile = await _fileStorage.OpenReadAsync(storagePath, cancellationToken);
        return storedFile is null
            ? null
            : new CatalogItemImageContent(storedFile.Content, storedFile.ContentType);
    }

    public async Task<bool> DeleteCatalogItemImageAsync(
        Guid workspaceId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanManageAsync(workspaceId, cancellationToken);

        var item = await _dbContext.WorkspaceCatalogItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.WorkspaceId == workspaceId, cancellationToken);

        if (item is null)
            return false;

        var storagePath = item.ImageStoragePath;
        item.ImageStoragePath = null;
        item.ImageUrl = null;
        item.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(storagePath))
            await _fileStorage.DeleteIfExistsAsync(storagePath, cancellationToken);

        return true;
    }

    private async Task EnsureMemberAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        if (!await _workspaceAccess.IsMemberAsync(workspaceId, cancellationToken))
            throw new UnauthorizedAccessException("Workspace no autorizado.");
    }

    private async Task EnsureCanManageAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var isAdmin = await _workspaceAccess.HasRoleAsync(workspaceId, "admin", cancellationToken);
        var isEditor = await _workspaceAccess.HasRoleAsync(workspaceId, "editor", cancellationToken);

        if (!isAdmin && !isEditor)
            throw new UnauthorizedAccessException("Se requiere rol admin o editor en el workspace.");
    }

    private static void Apply(WorkspaceCatalogItem item, UpsertWorkspaceCatalogItemRequest request, DateTime now)
    {
        item.Type = ParseEnum<CatalogItemType>(request.Type, nameof(request.Type));
        item.Name = request.Name.Trim();
        item.Description = request.Description.Trim();
        item.Category = TrimOrNull(request.Category);
        item.PriceLabel = TrimOrNull(request.PriceLabel);
        item.ImageUrl = TrimOrNull(request.ImageUrl);
        item.CtaLabel = TrimOrNull(request.CtaLabel);
        item.CtaUrl = TrimOrNull(request.CtaUrl);
        item.IsFeatured = request.IsFeatured;
        item.IsActive = request.IsActive;
        item.SortOrder = request.SortOrder;
        item.UpdatedAt = now;
    }

    private static WorkspaceProfileDto Map(WorkspaceProfile profile) => new()
    {
        Id = profile.Id,
        WorkspaceId = profile.WorkspaceId,
        DisplayName = profile.DisplayName,
        LegalName = profile.LegalName,
        Category = profile.Category,
        Description = profile.Description,
        LogoUrl = profile.LogoUrl,
        CoverImageUrl = profile.CoverImageUrl,
        Phone = profile.Phone,
        Email = profile.Email,
        WhatsApp = profile.WhatsApp,
        Address = profile.Address,
        OpeningHours = profile.OpeningHours,
        WebsiteUrl = profile.WebsiteUrl,
        FacebookUrl = profile.FacebookUrl,
        InstagramUrl = profile.InstagramUrl,
        TiktokUrl = profile.TiktokUrl,
        LinkedinUrl = profile.LinkedinUrl,
        PrimaryColor = profile.PrimaryColor,
        SecondaryColor = profile.SecondaryColor,
        AccentColor = profile.AccentColor,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };

    private static WorkspaceCatalogItemDto Map(WorkspaceCatalogItem item) => new()
    {
        Id = item.Id,
        WorkspaceId = item.WorkspaceId,
        Type = item.Type.ToString(),
        Name = item.Name,
        Description = item.Description,
        Category = item.Category,
        PriceLabel = item.PriceLabel,
        ImageUrl = item.ImageUrl,
        HasStoredImage = !string.IsNullOrWhiteSpace(item.ImageStoragePath),
        CtaLabel = item.CtaLabel,
        CtaUrl = item.CtaUrl,
        IsFeatured = item.IsFeatured,
        IsActive = item.IsActive,
        SortOrder = item.SortOrder,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };

    private static TEnum ParseEnum<TEnum>(string value, string fieldName)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"{fieldName} '{value}' no es valido.");
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeEmailOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string NormalizeImageContentType(string contentType)
    {
        var normalized = contentType.Trim().ToLowerInvariant();
        if (normalized is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new InvalidOperationException("Solo se permiten imagenes JPG, PNG o WebP.");

        return normalized;
    }

    private static void ValidateImageSignature(Stream content, string contentType)
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
            throw new InvalidOperationException("El contenido del archivo no corresponde a una imagen valida.");
    }
}
