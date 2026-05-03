using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WolfPage.Api.Application.Auth;

namespace WolfPage.Api.Auth;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Guid? UserId => ReadGuid(JwtRegisteredClaimNames.Sub)
        ?? ReadGuid(ClaimTypes.NameIdentifier);

    public Guid? TenantId => ReadGuid("tenant_id");

    public string? Email => ReadClaim(JwtRegisteredClaimNames.Email)
        ?? ReadClaim(ClaimTypes.Email);

    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext?.User.FindAll("role").Select(x => x.Value).ToArray()
        ?? [];

    private string? ReadClaim(string claimType) =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);

    private Guid? ReadGuid(string claimType) =>
        Guid.TryParse(ReadClaim(claimType), out var value) ? value : null;
}
