using WolfPage.Api.Application.Features.Workspaces.Dtos;

namespace WolfPage.Api.Application.Features.Workspaces.Services;

public interface IWorkspaceService
{
    Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto dto, CancellationToken cancellationToken = default);
    Task<WorkspaceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<WorkspaceDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
