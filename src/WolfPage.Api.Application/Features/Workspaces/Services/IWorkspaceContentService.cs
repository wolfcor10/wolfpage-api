using WolfPage.Api.Application.Features.Workspaces.Dtos;

namespace WolfPage.Api.Application.Features.Workspaces.Services;

public interface IWorkspaceContentService
{
    Task<WorkspaceProfileDto?> GetProfileAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<WorkspaceProfileDto> UpsertProfileAsync(Guid workspaceId, UpsertWorkspaceProfileRequest request, CancellationToken cancellationToken = default);
    Task<WorkspaceProfileDto?> UploadProfileLogoAsync(Guid workspaceId, CatalogItemImageUpload upload, CancellationToken cancellationToken = default);
    Task<CatalogItemImageContent?> GetProfileLogoAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<bool> DeleteProfileLogoAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<WorkspaceProfileDto?> UploadProfileCoverAsync(Guid workspaceId, CatalogItemImageUpload upload, CancellationToken cancellationToken = default);
    Task<CatalogItemImageContent?> GetProfileCoverAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<bool> DeleteProfileCoverAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<List<WorkspaceCatalogItemDto>> GetCatalogItemsAsync(Guid workspaceId, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<WorkspaceCatalogItemDto> CreateCatalogItemAsync(Guid workspaceId, UpsertWorkspaceCatalogItemRequest request, CancellationToken cancellationToken = default);
    Task<WorkspaceCatalogItemDto?> UpdateCatalogItemAsync(Guid workspaceId, Guid itemId, UpsertWorkspaceCatalogItemRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCatalogItemAsync(Guid workspaceId, Guid itemId, CancellationToken cancellationToken = default);
    Task<WorkspaceCatalogItemDto?> UploadCatalogItemImageAsync(Guid workspaceId, Guid itemId, CatalogItemImageUpload upload, CancellationToken cancellationToken = default);
    Task<CatalogItemImageContent?> GetCatalogItemImageAsync(Guid workspaceId, Guid itemId, CancellationToken cancellationToken = default);
    Task<bool> DeleteCatalogItemImageAsync(Guid workspaceId, Guid itemId, CancellationToken cancellationToken = default);
    Task<WorkspaceCatalogItemDto?> AddCatalogItemImageAsync(Guid workspaceId, Guid itemId, CatalogItemImageUpload upload, CancellationToken cancellationToken = default);
    Task<CatalogItemImageContent?> GetCatalogItemImageAsync(Guid workspaceId, Guid itemId, Guid imageId, CancellationToken cancellationToken = default);
    Task<WorkspaceCatalogItemDto?> SetPrimaryCatalogItemImageAsync(Guid workspaceId, Guid itemId, Guid imageId, CancellationToken cancellationToken = default);
    Task<WorkspaceCatalogItemDto?> DeleteCatalogItemImageAsync(Guid workspaceId, Guid itemId, Guid imageId, CancellationToken cancellationToken = default);
}
