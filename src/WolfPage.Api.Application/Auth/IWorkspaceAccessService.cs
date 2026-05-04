namespace WolfPage.Api.Application.Auth;

public interface IWorkspaceAccessService
{
    Task<bool> IsMemberAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<bool> HasRoleAsync(Guid workspaceId, string roleCode, CancellationToken cancellationToken = default);
    Task<string[]> GetRolesAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}
