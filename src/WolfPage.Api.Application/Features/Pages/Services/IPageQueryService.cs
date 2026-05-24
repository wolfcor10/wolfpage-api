using WolfPage.Api.Application.Features.Pages.Dtos;

namespace WolfPage.Api.Application.Features.Pages.Services;

public interface IPageQueryService
{
    Task<List<PageResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PageResponseDto?> GetByIdAsync(Guid pageId, CancellationToken cancellationToken = default);
}
