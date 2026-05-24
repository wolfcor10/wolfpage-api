namespace WolfPage.Api.Application.Auth;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? WorkspaceId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
}
