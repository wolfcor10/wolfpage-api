namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class LoginRequest
{
    public Guid? TenantId { get; set; }
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
