using WolfPage.Api.Application.Features.Pages.Dtos;

namespace WolfPage.Api.Application.Features.Pages.Services;

public interface IPageRequestService
{
    Task<PageRequestResponseDto> CreateAsync(CreatePageRequestDto dto, CancellationToken cancellationToken = default);
    Task<PageRequestResponseDto?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default);
}
