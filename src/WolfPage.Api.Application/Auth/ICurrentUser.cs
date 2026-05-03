namespace WolfPage.Api.Application.Auth;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
}
