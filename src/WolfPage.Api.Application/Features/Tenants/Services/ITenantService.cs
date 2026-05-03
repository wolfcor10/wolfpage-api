using WolfPage.Api.Application.Features.Tenants.Dtos;

namespace WolfPage.Api.Application.Features.Tenants.Services;

public interface ITenantService
{
    Task<TenantDto> CreateAsync(CreateTenantDto dto, CancellationToken cancellationToken = default);
    Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
