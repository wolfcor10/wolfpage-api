using WolfPage.Api.Application.Features.Pages.Dtos;

namespace WolfPage.Api.Application.Features.Pages.Services;

public interface IPageRequestService
{
    Task<PageResponseDto> CreateDraftAsync(BusinessPageCreateRequest request, CancellationToken cancellationToken = default);
    Task<PageResponseDto?> UpdateDraftAsync(Guid pageId, BusinessPageCreateRequest request, CancellationToken cancellationToken = default);
    Task<PageRequestResponseDto> PublishAsync(Guid pageId, CancellationToken cancellationToken = default);
    Task<PageRequestResponseDto> CreateBusinessPageAsync(BusinessPageCreateRequest request, CancellationToken cancellationToken = default);
    Task<PageRequestResponseDto> CreateAsync(CreatePageRequestDto dto, CancellationToken cancellationToken = default);
    Task<PageRequestResponseDto?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<PageResponseDto?> UploadHeroImageAsync(Guid pageId, PageHeroImageUpload upload, CancellationToken cancellationToken = default);
    Task<PageHeroImageContent?> GetHeroImageAsync(Guid pageId, CancellationToken cancellationToken = default);
    Task<bool> DeleteHeroImageAsync(Guid pageId, CancellationToken cancellationToken = default);
}
